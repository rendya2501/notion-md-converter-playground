using Notion.Client;

namespace hoge.Models;

/// <summary>
/// Notionのブロックを表します。
/// </summary>
/// <param name="block"></param>
public class NotionBlock(Block block)
{
    public string Id { get; set; } = block.Id;
    public BlockType Type { get; set; } = block.Type;
    public bool HasChildren { get; set; } = block.HasChildren;
    public List<NotionBlock> Children { get; set; } = [];
    public object OriginalBlock { get; set; } = block;

    /// <summary>
    /// ブロックのオリジナルを取得します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetOriginalBlock<T>() where T : Block
    {
        return (T)OriginalBlock;
    }
}