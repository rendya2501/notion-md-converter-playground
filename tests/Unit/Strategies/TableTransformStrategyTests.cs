using Notion.Client;
using NotionMarkdownConverter.Domain.Models;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies;

namespace NotionMarkdownConverter.Tests.Unit.Strategies;

/// <summary>
/// TableTransformStrategyのユニットテスト
/// </summary>
public class TableTransformStrategyTests
{
    private readonly TableTransformStrategy _sut = new();

    // ── ヘルパー ──────────────────────────────────────────────────────

    private static RichTextText MakeText(string text) =>
        new() { PlainText = text, Text = new Text { Content = text }, Annotations = new Annotations() };

    /// <summary>
    /// 指定したセルテキストで TableRowBlock を包んだ NotionBlock を生成します。
    /// </summary>
    private static NotionBlock MakeRow(IEnumerable<string> cellTexts)
    {
        // 各セルは IEnumerable<RichTextBase> 型で、Cells は List<IEnumerable<RichTextBase>>
        var cells = cellTexts
            .Select(t => (IEnumerable<RichTextText>)[MakeText(t)])
            .ToList();

        var rowBlock = new TableRowBlock
        {
            TableRow = new TableRowBlock.Info { Cells = cells }
        };
        return new NotionBlock(rowBlock);
    }

    /// <summary>
    /// ヘッダー行とデータ行を組み合わせた TableBlock を包んだ NotionBlock を生成します。
    /// </summary>
    private static NotionBlock MakeTable(NotionBlock headerRow, IEnumerable<NotionBlock>? dataRows = null)
    {
        var tableBlock = new TableBlock();
        var notionBlock = new NotionBlock(tableBlock);
        notionBlock.Children = [headerRow, .. (dataRows ?? [])];
        return notionBlock;
    }

    /// <summary>
    /// 子なしの空テーブルを生成します。
    /// </summary>
    private static NotionBlock MakeEmptyTable()
    {
        var notionBlock = new NotionBlock(new TableBlock());
        // Children はデフォルトで空リスト
        return notionBlock;
    }

    private static NotionBlockTransformContext MakeContext(NotionBlock block) => new()
    {
        ExecuteTransformBlocks = _ => string.Empty,
        Blocks = [block],
        CurrentBlock = block,
        CurrentBlockIndex = 0
    };

    // ── 子ブロックなし ────────────────────────────────────────────────

    [Fact]
    public void Transform_EmptyChildren_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, _sut.Transform(MakeContext(MakeEmptyTable())));
    }

    // ── ヘッダーのみ ──────────────────────────────────────────────────

    [Fact]
    public void Transform_HeaderOnly_ReturnsTwoLines()
    {
        var table = MakeTable(MakeRow(["名前", "年齢"]));
        var lines = _sut.Transform(MakeContext(table)).Split('\n');
        Assert.Equal(2, lines.Length);
    }

    [Fact]
    public void Transform_HeaderOnly_FirstLineContainsHeaders()
    {
        var table = MakeTable(MakeRow(["名前", "年齢"]));
        var headerLine = _sut.Transform(MakeContext(table)).Split('\n')[0];

        Assert.Contains("名前", headerLine);
        Assert.Contains("年齢", headerLine);
    }

    [Fact]
    public void Transform_HeaderOnly_SecondLineIsSeparator()
    {
        var table = MakeTable(MakeRow(["Col1", "Col2"]));
        var separatorLine = _sut.Transform(MakeContext(table)).Split('\n')[1];

        Assert.Matches(@"^\|[\s\-\|]+\|$", separatorLine);
    }

    // ── データ行あり ──────────────────────────────────────────────────

    [Fact]
    public void Transform_OneDataRow_ProducesThreeLines()
    {
        var table = MakeTable(
            MakeRow(["名前", "年齢"]),
            [MakeRow(["Alice", "30"])]);

        var lines = _sut.Transform(MakeContext(table)).Split('\n');
        Assert.Equal(3, lines.Length);
    }

    [Fact]
    public void Transform_MultipleDataRows_AllRendered()
    {
        var table = MakeTable(
            MakeRow(["Name"]),
            [MakeRow(["Alice"]), MakeRow(["Bob"]), MakeRow(["Charlie"])]);

        var result = _sut.Transform(MakeContext(table));
        var lines = result.Split('\n');

        // ヘッダー + セパレータ + 3データ行
        Assert.Equal(5, lines.Length);
        Assert.Contains("Alice", result);
        Assert.Contains("Bob", result);
        Assert.Contains("Charlie", result);
    }

    // ── バグ修正の検証 ────────────────────────────────────────────────

    [Fact]
    public void Transform_DataRowWithFewerCellsThanHeader_DoesNotThrow()
    {
        // 修正前: データ行のセル数がヘッダーより少ない場合に IndexOutOfRangeException が発生していた
        var table = MakeTable(
            MakeRow(["Col1", "Col2", "Col3"]),
            [MakeRow(["A"])]); // セルが1つしかない

        var ex = Record.Exception(() => _sut.Transform(MakeContext(table)));
        Assert.Null(ex);
    }

    [Fact]
    public void Transform_DataRowWithFewerCells_ProducesCorrectLineCount()
    {
        var table = MakeTable(
            MakeRow(["Col1", "Col2"]),
            [MakeRow(["A"])]); // Col2 が欠落

        var lines = _sut.Transform(MakeContext(table)).Split('\n');
        Assert.Equal(3, lines.Length);
    }

    [Fact]
    public void Transform_DataRowWithZeroCells_DoesNotThrow()
    {
        var table = MakeTable(
            MakeRow(["Col1", "Col2"]),
            [MakeRow([])]); // セルが0個

        var ex = Record.Exception(() => _sut.Transform(MakeContext(table)));
        Assert.Null(ex);
    }

    // ── 書式 ──────────────────────────────────────────────────────────

    [Fact]
    public void Transform_AllLines_StartAndEndWithPipe()
    {
        var table = MakeTable(
            MakeRow(["A", "B"]),
            [MakeRow(["1", "2"])]);

        foreach (var line in _sut.Transform(MakeContext(table)).Split('\n'))
        {
            Assert.StartsWith("|", line);
            Assert.EndsWith("|", line);
        }
    }

    [Fact]
    public void Transform_ColumnWidth_AtLeastThreeForSeparator()
    {
        // セパレータ行の最低幅は3（---）
        var table = MakeTable(MakeRow(["A"])); // 1文字のヘッダー
        var separatorLine = _sut.Transform(MakeContext(table)).Split('\n')[1];

        Assert.Contains("---", separatorLine);
    }

    [Fact]
    public void Transform_LongHeaderCell_PadsDataCellsToSameWidth()
    {
        // ヘッダーが長い場合、ヘッダー行とデータ行の長さが一致する（整列されている）
        var table = MakeTable(
            MakeRow(["LongHeaderText", "B"]),
            [MakeRow(["A", "2"])]);

        var lines = _sut.Transform(MakeContext(table)).Split('\n');
        Assert.Equal(lines[0].Length, lines[2].Length);
    }
}
