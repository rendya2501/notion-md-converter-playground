using Microsoft.Extensions.Logging;
using NotionMarkdownConverter.Infrastructure.FileSystem;
using NotionMarkdownConverter.Infrastructure.GitHub;
using NotionMarkdownConverter.Infrastructure.Notion;
using NotionMarkdownConverter.Pipeline.Models;
using System.Text;

namespace NotionMarkdownConverter.Load;

/// <summary>
/// Loadステージ。
/// TransformedPageを受け取り、ファイル書き出しとNotion側のステータス更新を行います。
/// </summary>
public class NotionPageLoader(
    INotionClientWrapper _notionClient,
    IGitHubEnvironmentUpdater _githubEnvironmentUpdater,
    IFileSystem _fileSystem,
    ILogger<NotionPageLoader> _logger)
{
    /// <summary>
    /// TransformedPageのリストをファイルに書き出し、Notionのプロパティを更新します。
    /// </summary>
    /// <param name="transformedPages">Transformステージの出力リスト</param>
    /// <returns>エクスポートに成功したページ数</returns>
    public async Task<int> LoadAsync(List<TransformedPage> transformedPages)
    {
        var now = DateTime.UtcNow;
        var exportedCount = 0;

        foreach (var page in transformedPages)
        {
            try
            {
                await _fileSystem.WriteAllTextAsync(
                   Path.Combine(page.OutputDirectory, "index.md"),
                   page.Markdown,
                   new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

                _logger.LogInformation("Load成功: PageId={PageId}, OutputDirectory={OutputDirectory}",
                    page.PageProperty.PageId, page.OutputDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Load失敗: PageId={PageId}", page.PageProperty.PageId);
                continue;
            }

            try
            {
                await _notionClient.UpdatePagePropertiesAsync(page.PageProperty.PageId, now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "公開ステータスの更新に失敗しました。次回実行時に再エクスポートされる可能性があります。PageId={PageId}",
                    page.PageProperty.PageId);
            }

            exportedCount++;
        }

        _githubEnvironmentUpdater.UpdateEnvironment(exportedCount);
        return exportedCount;
    }
}
