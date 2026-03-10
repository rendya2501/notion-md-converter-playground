using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using System.Text;

namespace NotionMarkdownConverter.Application.Services;

/// <summary>
/// Notionデータベースからページを取得し、Markdownファイルとしてエクスポートするサービス。
/// 公開ステータスと公開日時を判定し、対象ページのみを処理します。
/// </summary>
public class NotionExporter(
    IOptions<NotionExportOptions> _config,
    INotionClientWrapper _notionClient,
    IMarkdownAssembler _markdownAssembler,
    IGitHubEnvironmentUpdater _githubEnvironmentUpdater,
    IOutputDirectoryProvider _outputDirectoryProvider,
    IPagePropertyMapper _pagePropertyMapper,
    PageExportEligibilityChecker _eligibilityChecker,
    ILogger<NotionExporter> _logger) : INotionExporter
{
    /// <summary>
    /// 公開対象のNotionページを取得し、Markdownファイルとしてエクスポートします。
    /// エクスポート完了後、GitHub Actions の環境変数にエクスポート件数を書き込みます。
    /// </summary>
    /// <remarks>
    /// ページ単位の失敗は握りつぶして処理を継続します。
    /// データベース取得自体が失敗した場合は例外を再スローします。
    /// </remarks>
    /// <returns>エクスポート処理が完了したときに解決するタスク</returns>
    public async Task ExportPagesAsync()
    {
        // 公開可能なページを取得
        var pages = await FetchPagesAsync();
        // GitHub ActionsのランナーはUTCで動作するため、公開日時の比較にはUTCを使用します。
        var now = DateTime.UtcNow;
        var exportedCount = 0;

        foreach (var page in pages)
        {
            // ページをエクスポート
            if (await ExportPageAsync(page, now))
            {
                // エクスポート成功後にNotion側の公開ステータスを更新します。
                // 次回実行時に同じページが再エクスポートされることを防ぎます。
                try
                {
                    await _notionClient.UpdatePagePropertiesAsync(page.Id, now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "公開ステータスの更新に失敗しました。次回実行時に再エクスポートされる可能性があります。PageId={PageId}",
                        page.Id);
                }

                exportedCount++;
            }
        }

        // GitHub Actions の環境変数を更新
        _githubEnvironmentUpdater.UpdateEnvironment(exportedCount);

        // ページ一覧の取得失敗だけを明示的にログに残して再スローします。
        // 外側の try-catch に含めると UpdatePagePropertiesAsync など後続の失敗と
        // エラーメッセージが混同されるため、ローカル関数として切り出しています。
        async Task<List<Page>> FetchPagesAsync()
        {
            try
            {
                return await _notionClient.GetPagesForPublishingAsync(_config.Value.NotionDatabaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ページ一覧の取得に失敗しました");
                throw;
            }
        }
    }

    /// <summary>
    /// 単一ページのMarkdown生成とファイル書き出しを実行します。
    /// </summary>
    /// <remarks>
    /// このメソッドの責務はエクスポート（Markdown生成・ファイル書き出し）に限定します。
    /// エクスポート後のNotion側ステータス更新は呼び出し元（<see cref="ExportPagesAsync"/>）が担います。
    /// 責務を分けることで、書き出し失敗とステータス更新失敗を独立してハンドリングできます。
    /// </remarks>
    /// <param name="page">エクスポート対象のNotionページ</param>
    /// <param name="now">処理実行時刻。公開日時の判定に使用します。</param>
    /// <returns>エクスポートが実行された場合は <c>true</c>、スキップまたは失敗した場合は <c>false</c></returns>
    private async Task<bool> ExportPageAsync(Page page, DateTime now)
    {
        try
        {
            // Notionページのプロパティをドメインモデルに変換
            var pageProperty = _pagePropertyMapper.Map(page);

            // ページをエクスポートするかどうかを判定
            if (!_eligibilityChecker.ShouldExport(pageProperty, now))
            {
                return false;
            }

            // 出力ディレクトリを構築
            var outputDirectory = _outputDirectoryProvider.BuildAndCreate(pageProperty);

            // Markdownを組み立てる
            var markdown = await _markdownAssembler.AssembleAsync(pageProperty, outputDirectory);

            // BOMなしUTF-8でマークダウンファイルを書き出します。
            // GitHub Pagesなど多くの静的サイトジェネレーターはBOMを想定しないため、
            // new UTF8Encoding(false) でBOMを明示的に除外しています。
            await File.WriteAllTextAsync(
                Path.Combine(outputDirectory, "index.md"),
                markdown,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

            _logger.LogInformation("エクスポート成功: PageId={PageId}, OutputDirectory={OutputDirectory}",
                page.Id, outputDirectory);

            return true;
        }
        catch (Exception ex)
        {
            // 1ページの失敗で残りの処理を止めないよう、例外を握りつぶしてfalseを返します。
            // ページIDをログに残すことで、GitHub Actionsのログから失敗箇所を特定できます。
            _logger.LogError(ex, "エクスポート失敗: PageId={PageId}", page.Id);
            return false;
        }
    }
}
