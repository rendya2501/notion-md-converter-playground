using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// 見出し3（H3）変換ストラテジー
/// </summary>
public class HeadingThreeTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Heading_3;

    public string Transform(NotionBlockTransformContext context)
    {
        // ブロックを取得
        var block = BlockAccessor.GetOriginalBlock<HeadingThreeBlock>(context.CurrentBlock);
        // テキストを取得
        var text = MarkdownRichTextUtils.RichTextsToMarkdown(block.Heading_3.RichText);

        // 見出しを生成
        return MarkdownBlockUtils.Heading(text, 3);
    }
}
