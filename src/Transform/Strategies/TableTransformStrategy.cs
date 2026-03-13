using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Enums;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

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

        // テーブルの最初の行をヘッダーとして取得 (Children[0]はヘッダー行)
        var headers = BlockAccessor.GetOriginalBlock<TableRowBlock>(context.CurrentBlock.Children[0]);
        // テーブルの残りの行を取得 (Children[1]以降はデータ行)
        var rows = context.CurrentBlock.Children.Skip(1);

        // ヘッダー行のセルを取得
        var headerCells = headers.TableRow.Cells
            .Select(cell => MarkdownRichTextUtils.RichTextsToMarkdown(cell))
            .ToList();

        // データ行のセルを取得
        var rowsCells = rows
            .Select(s => BlockAccessor.GetOriginalBlock<TableRowBlock>(s).
                TableRow.Cells
                .Select(cell => MarkdownRichTextUtils.RichTextsToMarkdown(cell))
                .ToList())
            .ToList();

        // テーブルを生成
        return Table(headerCells, rowsCells);
    }

    /// <summary>
    /// Markdown形式のテーブル文字列を生成します。
    /// </summary>
    /// <param name="headers">ヘッダー行のセル文字列リスト</param>
    /// <param name="rows">データ行ごとのセル文字列リスト。各行のセル数がヘッダーより少ない場合は空文字で補完します。</param>
    /// <returns>Markdown形式のテーブル文字列</returns>
    private static string Table(List<string> headers, List<List<string>> rows)
    {
        // 各列の最大長を計算
        // データ行のセル数がヘッダーより少ない場合は空文字で補完し、IndexOutOfRangeを防ぎます。
        var columnWidths = headers.Select((header, index) =>
        {
            var cellsInColumn = new[] { header }.Concat(
                rows.Select(row => index < row.Count ? row[index] : string.Empty));
            var maxLength = cellsInColumn.Max(content => content.Length);
            // セパレータ行の最低幅として3を確保
            return Math.Max(maxLength, 3);
        }).ToList();

        // ヘッダー行を生成（パディングを追加）
        var headerRow = "| " + string.Join(" | ", headers.Select((h, i) => h.PadRight(columnWidths[i]))) + " |";

        // セパレータ行を生成（長さを合わせる）
        var alignmentRow = "| " + string.Join(" | ", headers.Select((_, i) => new string('-', columnWidths[i]))) + " |";

        // データ行がない場合は、ヘッダーとセパレータ行のみを返す
        if (rows.Count == 0)
        {
            return $"{headerRow}\n{alignmentRow}";
        }

        // データ行を生成（セル数がヘッダーより少ない行は空文字で補完）
        var dataRows = rows.Select(row =>
            "| " + string.Join(" | ", headers.Select((_, i) =>
            {
                var cell = i < row.Count ? row[i] : string.Empty;
                return MarkdownBlockUtils.ApplyLineBreaks(cell, LineBreakStyle.BR).PadRight(columnWidths[i]);
            })) + " |");

        return $"{headerRow}\n{alignmentRow}\n{string.Join("\n", dataRows)}";
    }
}
