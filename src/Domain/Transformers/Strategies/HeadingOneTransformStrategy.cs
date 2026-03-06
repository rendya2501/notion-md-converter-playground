using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// 見出し変換ストラテジー
/// </summary>
public class HeadingOneTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Heading_1;

    public string Transform(NotionBlockTransformContext context)
    {
        // ブロックを取得
        var block = BlockConverter.GetOriginalBlock<HeadingOneBlock>(context.CurrentBlock);
        // テキストを取得
        var text = MarkdownRichTextUtils.RichTextsToMarkdown(block.Heading_1.RichText);

        // 見出しを生成
        return MarkdownBlockUtils.Heading(text, 1);
    }
} 