using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Domain.Enums;
using NotionMarkdownConverter.Domain.Mappers;
using NotionMarkdownConverter.Domain.Models;
using System.Text;

namespace NotionMarkdownConverter.Application.Services;

/// <summary>
/// Notionデータベースからページを取得し、Markdownファイルとしてエクスポートするサービス。
/// 公開ステータスと公開日時を判定し、対象ページのみを処理します。
/// </summary>
public class NotionExporter(
    IOptions<NotionExportOptions> _config,
    INotionClientWrapper _notionClient,
    MarkdownAssembler _markdownAssembler,
    IGitHubEnvironmentUpdater _githubEnvironmentUpdater,
    IOutputDirectoryBuilder _outputDirectoryBuilder,
    IPagePropertyMapper _pagePropertyMapper,
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
    public async Task ExportPagesAsync()
    {
        try
        {
            // 公開可能なページを取得
            var pages = await _notionClient.GetPagesForPublishingAsync(_config.Value.NotionDatabaseId);
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
                    await _notionClient.UpdatePagePropertiesAsync(page.Id, now);

                    exportedCount++;
                }
            }

            // GitHub Actions の環境変数を更新
            _githubEnvironmentUpdater.UpdateEnvironment(exportedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ページ一覧の取得に失敗しました");
            throw;
        }
    }

    /// <summary>
    /// 単一ページのエクスポート処理を実行します。
    /// </summary>
    /// <param name="page">エクスポート対象のNotionページ</param>
    /// <param name="now">処理実行時刻。公開日時の判定に使用します。</param>
    /// <returns>エクスポートが実行された場合は <c>true</c>、スキップまたは失敗した場合は <c>false</c></returns>
    private async Task<bool> ExportPageAsync(Page page, DateTime now)
    {
        try
        {
            // ページのプロパティをコピー
            var pageData = _pagePropertyMapper.CopyPageProperties(page);

            // ページをエクスポートするかどうかを判定
            if (!ShouldExportPage(pageData, now))
            {
                return false;
            }

            // 出力ディレクトリを構築
            var outputDirectory = _outputDirectoryBuilder.Build(pageData);

            // Markdownを組み立てる
            var markdown = await _markdownAssembler.AssembleAsync(pageData, outputDirectory);

            // マークダウンを出力
            await File.WriteAllTextAsync(
                Path.Combine(outputDirectory, "index.md"),
                markdown,
                new UTF8Encoding(false));

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

    /// <summary>
    /// ページをエクスポートするかどうかを判定します。
    /// 公開ステータスが公開待ち、かつ公開日時が現在日時以前の場合にエクスポート対象と判断します。
    /// </summary>
    /// <param name="pageProperty">判定対象のページプロパティ</param>
    /// <param name="now">判定基準時刻</param>
    /// <returns>エクスポート対象の場合は <c>true</c></returns>
    private bool ShouldExportPage(PageProperty pageProperty, DateTime now)
    {
        // 公開ステータスが公開待ちでない場合はスキップ
        if (pageProperty.PublicStatus != PublicStatus.Queued)
        {
            _logger.LogInformation("スキップ: PageId={PageId}, Title={Title}, 理由=公開待ち以外のステータス",
                pageProperty.PageId, pageProperty.Title);
            return false;
        }

        // 公開日時が未設定の場合はスキップ
        if (!pageProperty.PublishedDateTime.HasValue)
        {
            _logger.LogInformation("スキップ: PageId={PageId}, Title={Title}, 理由=公開日時未設定",
                  pageProperty.PageId, pageProperty.Title);
            return false;
        }

        // 公開日時が未来の場合はスキップ
        if (now < pageProperty.PublishedDateTime.Value)
        {
            _logger.LogInformation("スキップ: PageId={PageId}, Title={Title}, 理由=公開日時未到達",
                pageProperty.PageId, pageProperty.Title);
            return false;
        }

        return true;
    }
}
