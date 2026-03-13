using Notion.Client;
using NotionMarkdownConverter.Transform.Context;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// 同期ブロック変換ストラテジー
/// </summary>
public class SyncedBlockTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.SyncedBlock;

    public string Transform(NotionBlockTransformContext context)
    {
        return string.Empty;
    }
}