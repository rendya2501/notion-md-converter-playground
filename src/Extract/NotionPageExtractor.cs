using Microsoft.Extensions.Logging;
using NotionMarkdownConverter.Infrastructure.Notion;
using NotionMarkdownConverter.Pipeline.Models;

namespace NotionMarkdownConverter.Extract;

/// <summary>
/// Extractステージ。
/// Notionから公開対象ページを取得し、ブロックツリーを付与して返します。
/// </summary>
public class NotionPageExtractor(
    INotionClientWrapper _notionClient,
    IPagePropertyMapper _pagePropertyMapper,
    PageExportEligibilityChecker _eligibilityChecker,
    ILogger<NotionPageExtractor> _logger)
{
    /// <summary>
    /// 指定データベースから公開対象ページを取得し、ブロックツリーを付与して返します。
    /// </summary>
    /// <param name="databaseId">取得対象のNotionデータベースID</param>
    /// <returns>公開対象と判定されたページのリスト</returns>
    public async Task<List<ExtractedPage>> ExtractAsync(string databaseId)
    {
        var pages = await _notionClient.GetPagesForPublishingAsync(databaseId);
        var now = DateTime.UtcNow;
        var result = new List<ExtractedPage>();

        foreach (var page in pages)
        {
            try
            {
                var pageProperty = _pagePropertyMapper.Map(page);

                if (!_eligibilityChecker.ShouldExport(pageProperty, now))
                {
                    _logger.LogDebug("スキップ: PageId={PageId}", page.Id);
                    continue;
                }

                var blocks = await _notionClient.FetchBlockTreeAsync(pageProperty.PageId);
                result.Add(new ExtractedPage(pageProperty, blocks));

                _logger.LogInformation("Extract成功: PageId={PageId}", page.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Extract失敗: PageId={PageId}", page.Id);
            }
        }

        return result;
    }
}
