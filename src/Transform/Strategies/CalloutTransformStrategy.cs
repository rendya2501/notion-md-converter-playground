using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Context;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// コールアウト変換ストラテジー
/// </summary>
public class CalloutTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Callout;

    public string Transform(NotionBlockTransformContext context)
    {
        // 子ブロックが存在する場合、子ブロックを変換
        var children = context.CurrentBlock.HasChildren
            ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
            : string.Empty;

        // コールアウトのテキストをMarkdown形式に変換
        var text = MarkdownBlockUtils.ApplyLineBreaks(
            MarkdownRichTextUtils.RichTextsToMarkdown(
                BlockAccessor.GetOriginalBlock<CalloutBlock>(context.CurrentBlock).Callout.RichText));

        // 子ブロックが存在しない場合、コールアウトのテキストを返す
        var result = string.IsNullOrEmpty(children) ? text : $"{text}\n{children}";

        // コールアウトをブロック引用に変換
        return MarkdownBlockUtils.Blockquote(result);
    }
}
