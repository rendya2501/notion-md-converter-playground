using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Context;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// 段落変換ストラテジー
/// </summary>
public class ParagraphTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Paragraph;

    public string Transform(NotionBlockTransformContext context)
    {
        // 現在のブロックを取得
        var currentBlock = context.CurrentBlock;

        // 子ブロックが存在する場合、子ブロックを変換
        var children = currentBlock.HasChildren
            ? context.ExecuteTransformBlocks(currentBlock.Children)
            : string.Empty;

        // 段落のテキストを取得して改行を追加
        var text = MarkdownBlockUtils.ApplyLineBreaks(
            MarkdownRichTextUtils.RichTextsToMarkdown(
                BlockAccessor.GetOriginalBlock<ParagraphBlock>(currentBlock).Paragraph.RichText));

        // 子ブロックが存在しない場合、段落のテキストを返す
        return string.IsNullOrEmpty(children)
            ? text
            : $"{text}{Environment.NewLine}{children}";
    }
} 