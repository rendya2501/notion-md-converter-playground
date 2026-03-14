using Notion.Client;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;

namespace NotionMarkdownConverter.Transform.Strategies;

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
