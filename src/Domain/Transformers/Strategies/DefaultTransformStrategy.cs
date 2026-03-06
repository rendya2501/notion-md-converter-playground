using Notion.Client;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// デフォルトの変換ストラテジー
/// </summary>
public class DefaultTransformStrategy : IDefaultBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Unsupported;

    public string Transform(NotionBlockTransformContext context)
    {
        return string.Empty;
    }
}