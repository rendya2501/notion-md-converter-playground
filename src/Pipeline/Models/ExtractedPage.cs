using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Pipeline.Models;

/// <summary>
/// Extractステージの出力。
/// 公開対象と判定済みのページプロパティとブロックツリーを保持します。
/// </summary>
public record ExtractedPage(
    PageProperty PageProperty,
    List<NotionBlock> Blocks
);
