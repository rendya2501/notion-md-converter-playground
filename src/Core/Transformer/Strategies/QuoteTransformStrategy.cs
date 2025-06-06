using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// 引用変換ストラテジー
/// </summary>
public class QuoteTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Quote;

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

        // 引用のテキストを取得して改行を追加
        var text = MarkdownUtils.LineBreak(
            MarkdownUtils.RichTextsToMarkdown(
                BlockConverter.GetOriginalBlock<QuoteBlock>(context.CurrentBlock).Quote.RichText));

        // 子ブロックが存在しない場合、引用のテキストを返す
        return MarkdownUtils.Blockquote(
            string.IsNullOrEmpty(children) 
                ? text 
                : $"{text}\n{children}");
    }
} 