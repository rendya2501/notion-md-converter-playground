using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// 区切り線変換ストラテジー
/// </summary>
public class DividerTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Divider;

    public string Transform(NotionBlockTransformContext context)
    {
        // 水平線を生成
        return MarkdownBlockUtils.HorizontalRule();
    }
}
