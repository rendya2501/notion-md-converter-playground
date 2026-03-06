using Notion.Client;

namespace NotionMarkdownConverter.Domain.Models;

/// <summary>
/// Notionのブロック
/// </summary>
/// <param name="_block"></param>
public class NotionBlock(IBlock _block)
{
    /// <summary>
    /// ブロックID
    /// </summary>
    public string Id { get; } = _block.Id;

    /// <summary>
    /// ブロックタイプ
    /// </summary>
    public BlockType Type { get; } = _block.Type;

    /// <summary>
    /// 子ブロック
    /// </summary>
    public List<NotionBlock> Children { get; set; } = [];

    /// <summary>
    /// 子ブロックを持つかどうか
    /// </summary>
    public bool HasChildren { get; } = _block.HasChildren;

    /// <summary>
    /// オリジナルのブロック
    /// </summary>
    public object OriginalBlock { get; } = _block;
} 