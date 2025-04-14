using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Infrastructure.Notion.Services;
using NotionMarkdownConverter.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

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
    public string Transform(NotionBlockTransformState context)
    {
        // 子ブロックが存在する場合、子ブロックを変換
        var children = context.CurrentBlock.HasChildren
            ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
            : string.Empty;

        // コールアウトのテキストをMarkdown形式に変換
        var text = MarkdownUtils.LineBreak(
            MarkdownUtils.RichTextsToMarkdown(
                BlockConverter.GetOriginalBlock<CalloutBlock>(context.CurrentBlock).Callout.RichText));

        // 子ブロックが存在しない場合、コールアウトのテキストを返す
        var result = string.IsNullOrEmpty(children) ? text : $"{text}\n{children}";

        // コールアウトをブロック引用に変換
        return MarkdownUtils.Blockquote(result);
    }
}
