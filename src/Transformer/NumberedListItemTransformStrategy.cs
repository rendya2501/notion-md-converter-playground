using Notion.Client;
using NotionMarkdownConverter.Models;
using NotionMarkdownConverter.Utils;

namespace NotionMarkdownConverter.Transformer;

/// <summary>
/// 番号付きリスト変換ストラテジー
/// </summary>
public class NumberedListItemTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.NumberedListItem;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        // 番号付きリストのリスト数を取得
        var listCount = context.Blocks
            .Take(context.CurrentBlockIndex)
            .Reverse()
            .TakeWhile(b => b.OriginalBlock is NumberedListItemBlock)
            .Count() + 1;

        // 番号付きリストのブロックを取得   
        var block = context.CurrentBlock.GetOriginalBlock<NumberedListItemBlock>();
        // 番号付きリストのテキストを取得して改行を追加
        var text = MarkdownUtils.LineBreak(
            MarkdownUtils.RichTextsToMarkdown(
                block.NumberedListItem.RichText));

        // テキストに改行が含まれている場合、2行目以降にインデントを適用
        var lines = text.Split('\n');
        var formattedText = lines.Length > 1
            ? $"{lines[0]}\n{string.Join("\n", lines.Skip(1).Select(line => MarkdownUtils.Indent(line, 3)))}"
            : text;

        // 子ブロックが存在する場合、子ブロックを変換
        var children = context.CurrentBlock.HasChildren
            ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
            : string.Empty;

        // 子ブロックが存在しない場合、番号付きリストを返す
        return string.IsNullOrEmpty(children)
            ? MarkdownUtils.NumberedList(formattedText, listCount)
            : $"{MarkdownUtils.NumberedList(formattedText, listCount)}\n{MarkdownUtils.Indent(children, 3)}";
    }
} 