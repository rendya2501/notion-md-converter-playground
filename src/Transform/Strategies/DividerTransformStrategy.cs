using Notion.Client;
using NotionMarkdownConverter.Transform.Context;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

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
