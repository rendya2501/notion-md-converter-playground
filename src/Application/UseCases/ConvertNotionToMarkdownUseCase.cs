using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Application.Interfaces;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Core.Enums;
using NotionMarkdownConverter.Core.Mappers;
using NotionMarkdownConverter.Core.Models;
using NotionMarkdownConverter.Core.Services;
using System.Text;

namespace NotionMarkdownConverter.Application.UseCases;

/// <summary>
/// Notionのページをマークダウンに変換するユースケース
/// </summary>
/// <param name="_config"></param>
/// <param name="_notionClient"></param>
/// <param name="_markdownGenerator"></param>
/// <param name="_githubEnvironmentUpdater"></param>
/// <param name="_outputDirectoryBuilder"></param>
/// <param name="_logger"></param>
public class ConvertNotionToMarkdownUseCase(
    IOptions<AppConfiguration> _config,
    INotionWrapperClient _notionClient,
    MarkdownGenerator _markdownGenerator,
    IGitHubClient _githubEnvironmentUpdater,
    IDirectoryBuilder _outputDirectoryBuilder,
    DownloadLinkProcessor _downloadLinkProcessor,
    ILogger<ConvertNotionToMarkdownUseCase> _logger)
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
            var pageData = PagePropertyMapper.CopyPageProperties(page);

            // ページをエクスポートするかどうかを判定
            if (!ShouldExportPage(pageData, now))
            {
                return false;
            }

            // 出力ディレクトリを構築
            var outputDirectory = _outputDirectoryBuilder.Build(pageData);

            // マークダウンを生成
            var markdown = await GenerateMarkdownAsync(pageData, outputDirectory);

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


    private async Task<string> GenerateMarkdownAsync(PageProperty pageProperty, string outputDirectory)
    {
        // 1. ページ内容取得
        var pageContent = await _notionClient.GetPageFullContentAsync(pageProperty.PageId);

        // 2. マークダウン生成
        var markdown = _markdownGenerator.GenerateMarkdown(pageContent, pageProperty);

        // 3. リンク処理
        return await _downloadLinkProcessor.ProcessLinkAsync(markdown, outputDirectory);
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


//public class ConvertNotionToMarkdownUseCase(INotionService notionService)
//{
//    public async Task<UrlFilePair> ExecuteAsync(string pageId)
//    {
//        var properties = await notionService.GetPagePropertiesAsync(pageId);
//        var block = await notionService.GetBlockAsync(pageId);
//        return await notionService.ProcessBlockContentAsync(block);
//    }
//}

