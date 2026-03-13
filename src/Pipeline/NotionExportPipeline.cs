using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Extract;
using NotionMarkdownConverter.Load;
using NotionMarkdownConverter.Pipeline.Models;
using NotionMarkdownConverter.Transform;

namespace NotionMarkdownConverter.Pipeline;

/// <summary>
/// ETLパイプラインのオーケストレーター。
/// Extract → Transform → Load の順に各ステージを呼び出します。
/// </summary>
public class NotionExportPipeline(
    IOptions<NotionExportOptions> _config,
    NotionPageExtractor _extractor,
    NotionPageTransformer _transformer,
    NotionPageLoader _loader,
    ILogger<NotionExportPipeline> _logger)
{
    public async Task RunAsync()
    {
        // Extract
        _logger.LogInformation("Extractステージ開始");
        var extractedPages = await _extractor.ExtractAsync(_config.Value.NotionDatabaseId);
        _logger.LogInformation("Extractステージ完了: {Count}件", extractedPages.Count);

        // Transform
        _logger.LogInformation("Transformステージ開始");
        var transformedPages = new List<TransformedPage>();
        foreach (var extractedPage in extractedPages)
        {
            try
            {
                var transformed = await _transformer.TransformAsync(extractedPage);
                transformedPages.Add(transformed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transform失敗: PageId={PageId}",
                    extractedPage.PageProperty.PageId);
            }
        }
        _logger.LogInformation("Transformステージ完了: {Count}件", transformedPages.Count);

        // Load
        _logger.LogInformation("Loadステージ開始");
        var exportedCount = await _loader.LoadAsync(transformedPages);
        _logger.LogInformation("Loadステージ完了: {Count}件エクスポート", exportedCount);
    }
}
