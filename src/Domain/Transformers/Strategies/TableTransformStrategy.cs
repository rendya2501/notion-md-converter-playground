using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Enums;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// テーブル変換ストラテジー
/// </summary>
public class TableTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Table;

    public string Transform(NotionBlockTransformContext context)
    {
        // 子ブロックが存在しない場合は空文字を返す
        if (context.CurrentBlock.Children.Count == 0)
        {
            return string.Empty;
        }

        // テーブルブロックに親ブロックの子ブロックとして存在するため、親ブロックは取得しても意味がないので取得しない。
        // var tableBlock = BlockConverter.GetOriginalBlock<TableBlock>(context.CurrentBlock);

        // テーブルの最初の行をヘッダーとして取得 (Children[0]はヘッダー行)
        var headers = BlockCaster.GetOriginalBlock<TableRowBlock>(context.CurrentBlock.Children[0]);
        // テーブルの残りの行を取得 (Children[1]以降はデータ行)
        var rows = context.CurrentBlock.Children.Skip(1);

        // ヘッダー行のセルを取得
        var headerCells = headers.TableRow.Cells
            .Select(cell => MarkdownRichTextUtils.RichTextsToMarkdown(cell))
            .ToList();

        // データ行のセルを取得
        var rowsCells = rows
            .Select(s => BlockCaster.GetOriginalBlock<TableRowBlock>(s).
                TableRow.Cells
                .Select(cell => MarkdownRichTextUtils.RichTextsToMarkdown(cell))
                .ToList())
            .ToList();

        // テーブルを生成
        return Table(headerCells, rowsCells);
    }

    /// <summary>
    /// テーブルを生成します。
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="rows"></param>
    /// <returns></returns>
    private static string Table(List<string> headers, List<List<string>> rows)
    {
        // 各列の最大長を計算
        var columnWidths = headers.Select((header, index) =>
        {
            var cellsInColumn = new[] { header }.Concat(rows.Select(row => row[index]));
            var maxLength = cellsInColumn.Max(content => content.Length);
            // 3行分の間隔を開ける為の最低値
            return Math.Max(maxLength, 3);
        }).ToList();

        // ヘッダー行を生成（パディングを追加）
        var headerRow = "| " + string.Join(" | ", headers.Select((h, i) => h.PadRight(columnWidths[i]))) + " |";

        // セパレータ行を生成（長さを合わせる）
        var alignmentRow = "| " + string.Join(" | ", headers.Select((h, i) => new string('-', columnWidths[i]))) + " |";

        // データ行がない場合は、ヘッダーとセパレータ行のみを返す
        if (rows.Count == 0)
        {
            return $"{headerRow}\n{alignmentRow}";
        }

        // データ行を生成（パディングを追加）
        var dataRows = rows.Select(row => 
            "| " + string.Join(" | ", row.Select((cell, i) => MarkdownBlockUtils.LineBreak(cell, LineBreakStyle.BR).PadRight(columnWidths[i]))) + " |");

        return $"{headerRow}\n{alignmentRow}\n{string.Join("\n", dataRows)}";
    }
}