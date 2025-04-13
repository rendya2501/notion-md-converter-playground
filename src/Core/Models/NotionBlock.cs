using Notion.Client;

namespace NotionMarkdownConverter.Core.Models;

/// <summary>
/// Notionのブロック
/// </summary>
/// <param name="block"></param>
public class NotionBlock(IBlock block)
{
    /// <summary>
    /// ブロックID
    /// </summary>
    public string Id { get; } = block.Id;

    /// <summary>
    /// ブロックタイプ
    /// </summary>
    public BlockType Type { get; } = block.Type;

    /// <summary>
    /// 子ブロック
    /// </summary>
    public List<NotionBlock> Children { get; set; } = [];

    /// <summary>
    /// 子ブロックを持つかどうか
    /// </summary>
    public bool HasChildren { get; } = block.HasChildren;

    /// <summary>
    /// オリジナルのブロック
    /// </summary>
    public object OriginalBlock { get; } = block;
} 