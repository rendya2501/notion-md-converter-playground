using Notion.Client;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

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