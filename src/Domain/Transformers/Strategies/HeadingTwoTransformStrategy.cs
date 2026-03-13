using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Shared.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// 見出し2（H2）変換ストラテジー
/// </summary>
public class HeadingTwoTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Heading_2;

    public string Transform(NotionBlockTransformContext context)
    {
        // ブロックを取得
        var block = BlockAccessor.GetOriginalBlock<HeadingTwoBlock>(context.CurrentBlock);
        // テキストを取得
        var text = MarkdownRichTextUtils.RichTextsToMarkdown(block.Heading_2.RichText);

        // 見出しを生成
        return MarkdownBlockUtils.Heading(text, 2);
    }
} 