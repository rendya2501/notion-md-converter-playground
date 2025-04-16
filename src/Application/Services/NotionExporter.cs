using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Core.Enums;
using NotionMarkdownConverter.Core.Models;
using NotionMarkdownConverter.Core.Services.Markdown;
using NotionMarkdownConverter.Infrastructure.FileSystem.Services;
using NotionMarkdownConverter.Infrastructure.GitHub.Services;
using NotionMarkdownConverter.Infrastructure.Notion.Clients;
using System.Text;

namespace NotionMarkdownConverter.Application.Services;

/// <summary>
/// Notionのページをエクスポートするサービス
/// </summary>
public class NotionExporter(
    IOptions<AppConfiguration> _config,
    INotionClientWrapper _notionClient,
    IMarkdownGenerator _markdownGenerator,
    IGitHubEnvironmentUpdater _githubEnvironmentUpdater,
    IOutputDirectoryBuilder _outputDirectoryBuilder,
    ILogger<NotionExporter> _logger,
    IEventPublisher _eventPublisher) : INotionExporter
{
    /// <summary>
    /// Notionのページをエクスポートします。
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
    /// ページをエクスポートします。
    /// </summary>
    /// <param name="page"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    private async Task<bool> ExportPageAsync(Page page, DateTime now)
    {
        try
        {
            // ページのプロパティをコピー
            var pageData = _notionClient.CopyPageProperties(page);

            // ページをエクスポートするかどうかを判定
            if (!ShouldExportPage(pageData, now))
            {
                return false;
            }

            // 出力ディレクトリを構築
            var outputDirectory = _outputDirectoryBuilder.Build(pageData);

            // Notify subscribers
            _eventPublisher.Publish(new OutputDirectoryChangedEvent(outputDirectory));

            // マークダウンを生成
            var markdown = await _markdownGenerator.GenerateMarkdownAsync(pageData, outputDirectory);

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
    /// </summary>
    /// <param name="pageProperty"></param>
    /// <param name="now"></param>
    /// <returns></returns>
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