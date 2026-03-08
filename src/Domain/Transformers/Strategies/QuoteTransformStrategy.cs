using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// 引用変換ストラテジー
/// </summary>
public class QuoteTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Quote;

    public string Transform(NotionBlockTransformContext context)
    {
        // 子ブロックが存在する場合、子ブロックを変換
        var children = context.CurrentBlock.HasChildren
            ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
            : string.Empty;

        // 引用のテキストを取得して改行を追加
        var text = MarkdownBlockUtils.LineBreak(
            MarkdownRichTextUtils.RichTextsToMarkdown(
                BlockAccessor.GetOriginalBlock<QuoteBlock>(context.CurrentBlock).Quote.RichText));

        // 子ブロックが存在しない場合、引用のテキストを返す
        return MarkdownBlockUtils.Blockquote(
            string.IsNullOrEmpty(children) 
                ? text 
                : $"{text}\n{children}");
    }
} 