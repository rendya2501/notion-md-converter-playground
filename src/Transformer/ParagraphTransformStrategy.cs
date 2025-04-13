using Notion.Client;
using NotionMarkdownConverter.Infrastructure.Notion.Services;
using NotionMarkdownConverter.Models;
using NotionMarkdownConverter.Utils;

namespace NotionMarkdownConverter.Transformer;

/// <summary>
/// 段落変換ストラテジー
/// </summary>
public class ParagraphTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Paragraph;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        // 現在のブロックを取得
        var currentBlock = context.CurrentBlock;

        // 子ブロックが存在する場合、子ブロックを変換
        var children = currentBlock.HasChildren
            ? context.ExecuteTransformBlocks(currentBlock.Children)
            : string.Empty;

        // 段落のテキストを取得して改行を追加
        var text = MarkdownUtils.LineBreak(
            MarkdownUtils.RichTextsToMarkdown(
                BlockConverter.GetOriginalBlock<ParagraphBlock>(currentBlock).Paragraph.RichText));

        // 子ブロックが存在しない場合、段落のテキストを返す
        return string.IsNullOrEmpty(children)
            ? text
            : $"{text}{Environment.NewLine}{children}";
    }
} 