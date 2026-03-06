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
/// Notionのページをエクスポートするサービス
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
    /// </summary>
    /// <returns></returns>
    public async Task ExportPagesAsync()
    {
        try
        {
            // 公開可能なページを取得
            var pages = await _notionClient.GetPagesForPublishingAsync(_config.Value.NotionDatabaseId);
            // 現在の日時
            var now = DateTime.Now;
            // エクスポート成功数
            var exportedCount = 0;

            foreach (var page in pages)
            {
                try
                {
                    // ページをエクスポート
                    if (await ExportPageAsync(page, now))
                    {
                        // ページのプロパティを更新
                        await _notionClient.UpdatePagePropertiesAsync(page.Id, now);
                        // エクスポート成功数をインクリメント
                        exportedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to export page {PageId}", page.Id);
                }
            }

            // GitHub Actions の環境変数を更新
            _githubEnvironmentUpdater.UpdateEnvironment(exportedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export pages");
            throw;
        }
    }

    /// <summary>
    /// 単一ページのエクスポート処理を実行します。
    /// </summary>
    /// <param name="page">エクスポート対象のNotionページ</param>
    /// <param name="now">処理実行時刻。公開日時の判定と更新に使用します。</param>
    /// <returns>エクスポートが実行された場合は <c>true</c>、スキップされた場合は <c>false</c></returns>
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

            _logger.LogInformation("Successfully exported page {PageId} to {OutputDirectory}",
                page.Id, outputDirectory);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export page {PageId}", page.Id);
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
            _logger.LogInformation("Skipping page {PageId} (title = {Title}): Not published.",
                pageProperty.PageId, pageProperty.Title);
            return false;
        }

        // 公開日時が未設定の場合はスキップ
        if (!pageProperty.PublishedDateTime.HasValue)
        {
            _logger.LogInformation("Skipping page {PageId} (title = {Title}): Missing publish date.",
                pageProperty.PageId, pageProperty.Title);
            return false;
        }

        // 公開日時が未来の場合はスキップ
        if (now < pageProperty.PublishedDateTime.Value)
        {
            _logger.LogInformation("Skipping page {PageId} (title = {Title}): Publication date not reached.",
                pageProperty.PageId, pageProperty.Title);
            return false;
        }

        return true;
    }
}
