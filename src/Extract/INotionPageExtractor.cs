using NotionMarkdownConverter.Pipeline.Models;

namespace NotionMarkdownConverter.Extract.Abstractions;

/// <summary>
/// NotionからExtractedPageのリストを取得するステージのインターフェース
/// </summary>
public interface INotionPageExtractor
{
    /// <summary>
    /// 公開対象ページを取得し、ブロックツリーを付与して返します。
    /// </summary>
    Task<List<ExtractedPage>> ExtractAsync(CancellationToken cancellationToken = default);
}
