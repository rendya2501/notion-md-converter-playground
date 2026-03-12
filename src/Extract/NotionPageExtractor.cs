using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Application.Services;
using NotionMarkdownConverter.Extract.Abstractions;
using NotionMarkdownConverter.Pipeline.Models;

namespace NotionMarkdownConverter.Extract;

/// <summary>
/// Extractステージ。
/// Notionから公開対象ページを取得し、ブロックツリーを付与して返します。
/// NotionExporterが持っていた「取得・判定」の責務を担います。
/// </summary>
public class NotionPageExtractor(
    IOptions<NotionExportOptions> _config,
    INotionClientWrapper _notionClient,
    IPagePropertyMapper _pagePropertyMapper,
    PageExportEligibilityChecker _eligibilityChecker,
    ILogger<NotionPageExtractor> _logger) : INotionPageExtractor
{
    public async Task<List<ExtractedPage>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        // 公開待ちページ一覧を取得（失敗時は呼び出し元に例外を伝播）
        var pages = await _notionClient.GetPagesForPublishingAsync(_config.Value.NotionDatabaseId);
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

                // ブロックツリーを取得してExtractedPageに詰める
                var blocks = await _notionClient.FetchBlockTreeAsync(pageProperty.PageId);
                result.Add(new ExtractedPage(pageProperty, blocks));

                _logger.LogInformation("Extract成功: PageId={PageId}", page.Id);
            }
            catch (Exception ex)
            {
                // 1ページの失敗で残りを止めない（NotionExporterと同じ方針）
                _logger.LogError(ex, "Extract失敗: PageId={PageId}", page.Id);
            }
        }

        return result;
    }
}
