using Notion.Client;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// 目次変換ストラテジー
/// </summary>
public class TableOfContentsTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.TableOfContents;

    public string Transform(NotionBlockTransformContext context)
    {
        return string.Empty;
    }
}