using Notion.Client;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// パンくずリスト変換ストラテジー
/// </summary>
public class BreadcrumbTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Breadcrumb;

    public string Transform(NotionBlockTransformContext context)
    {
        return string.Empty;
    }
}
