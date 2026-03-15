using Notion.Client;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// 同期ブロック変換ストラテジー
/// </summary>
public class SyncedBlockTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.SyncedBlock;

    public string Transform(NotionBlockTransformContext context)
    {
        // 同期ブロック自体は変換せず、子ブロックの変換結果をそのまま返す
        return context.ExecuteTransformBlocks(context.CurrentBlock.Children);
    }
}
