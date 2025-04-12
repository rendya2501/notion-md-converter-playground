using Microsoft.Extensions.Logging;
using Notion.Client;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Models;
using System.Text;

namespace NotionMarkdownConverter.Services;

/// <summary>
/// Notionのページをエクスポートするサービス
/// </summary>
public class NotionExporter(
    AppConfiguration _config,
    INotionClientWrapper _notionClient,
    IMarkdownGenerator _markdownGenerator,
    IImageProcessor _imageProcessor,
    ILogger<NotionExporter> _logger) : INotionExporter
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
            var pages = await _notionClient.GetPagesForPublishingAsync(_config.NotionDatabaseId);
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
            UpdateGitHubEnvironment(exportedCount);
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
            var outputDirectory = BuildOutputDirectory(pageData);
            Directory.CreateDirectory(outputDirectory);

            // マークダウンを生成
            var markdown = await _markdownGenerator.GenerateMarkdownAsync(pageData);
            // マークダウン内の画像URLを置換
            markdown = await _imageProcessor.ProcessMarkdownImagesAsync(markdown, outputDirectory);
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

    /// <summary>
    /// 出力ディレクトリを構築します。
    /// </summary>
    /// <param name="pageProperty"></param>
    /// <returns></returns>
    private string BuildOutputDirectory(PageProperty pageProperty)
    {
        // 出力ディレクトリのパスをテンプレートから生成
        var template = Scriban.Template.Parse(_config.OutputDirectoryPathTemplate);
        // スラグが設定されていない場合はタイトルを使用
        var slug = !string.IsNullOrEmpty(pageProperty.Slug)
            ? pageProperty.Slug
            : pageProperty.Title;

        // 出力ディレクトリパスをレンダリング
        return template.Render(new
        {
            publish = pageProperty.PublishedDateTime!.Value,
            title = pageProperty.Title,
            slug = slug
        });
    }

    /// <summary>
    /// GitHub Actions の環境変数を更新します。
    /// </summary>
    /// <param name="exportedCount"></param>
    private void UpdateGitHubEnvironment(int exportedCount)
    {
        // GitHub Actions の環境変数ファイルパスを取得
        var githubOutput  = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");

        // 環境変数が設定されていない場合は警告を出力
        if (string.IsNullOrEmpty(githubOutput))
        {
            _logger.LogWarning("GITHUB_OUTPUT not set, skipping environment update.");
            return;
        }

        using (StreamWriter writer = new(githubOutput, true))
        {
            writer.WriteLine($"exported_count={exportedCount}");
        }
        _logger.LogInformation("exported_count={exportedCount}", exportedCount);
    }
}

