using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// コールアウト変換ストラテジー
/// </summary>
public class CalloutTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Callout;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        // 子ブロックが存在する場合、子ブロックを変換
        var children = context.CurrentBlock.HasChildren
            ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
            : string.Empty;

        // コールアウトのテキストをMarkdown形式に変換
        var text = MarkdownBlockUtils.LineBreak(
            MarkdownRichTextUtils.RichTextsToMarkdown(
                BlockConverter.GetOriginalBlock<CalloutBlock>(context.CurrentBlock).Callout.RichText));

        // 子ブロックが存在しない場合、コールアウトのテキストを返す
        var result = string.IsNullOrEmpty(children) ? text : $"{text}\n{children}";

        // コールアウトをブロック引用に変換
        return MarkdownBlockUtils.Blockquote(result);
    }
}
