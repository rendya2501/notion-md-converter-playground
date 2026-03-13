using Notion.Client;
using NotionMarkdownConverter.Transform.Context;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;

namespace NotionMarkdownConverter.Transform.Strategies;

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