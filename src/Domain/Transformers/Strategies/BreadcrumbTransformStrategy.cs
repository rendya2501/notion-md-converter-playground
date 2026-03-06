using Notion.Client;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// ブラウザパス変換ストラテジー
/// </summary>
public class BreadcrumbTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Breadcrumb;

    public string Transform(NotionBlockTransformContext context)
    {
        return string.Empty;
    }
}
