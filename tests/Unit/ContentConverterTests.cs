using Notion.Client;
using NotionMarkdownConverter.Shared.Models;
using NotionMarkdownConverter.Transform;
using NotionMarkdownConverter.Transform.Context;
using NotionMarkdownConverter.Transform.Converters;
using NotionMarkdownConverter.Transform.Strategies;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// ContentConverterのユニットテスト
/// </summary>
public class ContentConverterTests
{
    // ── スタブ ────────────────────────────────────────────────────────

    /// <summary>
    /// 固定の文字列を返すスタブストラテジー
    /// </summary>
    private sealed class StubStrategy(BlockType blockType, string output) : IBlockTransformStrategy
    {
        public BlockType BlockType => blockType;
        public string Transform(NotionBlockTransformContext context) => output;
    }

    /// <summary>
    /// 呼び出し時のインデックスとブロックを記録するスタブストラテジー
    /// </summary>
    private sealed class CapturingStrategy(BlockType blockType) : IBlockTransformStrategy
    {
        public BlockType BlockType => blockType;
        public List<(int Index, NotionBlock Block, List<NotionBlock> AllBlocks)> Calls { get; } = [];

        public string Transform(NotionBlockTransformContext context)
        {
            Calls.Add((context.CurrentBlockIndex, context.CurrentBlock, context.Blocks));
            return $"block-{context.CurrentBlockIndex}";
        }
    }

    /// <summary>
    /// ExecuteTransformBlocks デリゲートをキャプチャするスタブストラテジー
    /// </summary>
    private sealed class ExecutorCapturingStrategy(BlockType blockType) : IBlockTransformStrategy
    {
        public BlockType BlockType => blockType;
        public Func<List<NotionBlock>, string>? CapturedExecutor { get; private set; }

        public string Transform(NotionBlockTransformContext context)
        {
            CapturedExecutor = context.ExecuteTransformBlocks;
            return "result";
        }
    }

    // ── ヘルパー ──────────────────────────────────────────────────────

    private static NotionBlock MakeParagraphBlock()
    {
        var block = new ParagraphBlock { Paragraph = new ParagraphBlock.Info { RichText = [] } };
        return new NotionBlock(block);
    }

    private static ContentConverter CreateConverter(params IBlockTransformStrategy[] strategies)
    {
        var dispatcher = new BlockTransformDispatcher(strategies, new DefaultTransformStrategy());
        return new ContentConverter(dispatcher);
    }

    // ── 空リスト ──────────────────────────────────────────────────────

    [Fact]
    public void Convert_EmptyList_ReturnsEmptyString()
    {
        var converter = CreateConverter();

        var result = converter.Convert([]);

        Assert.Equal(string.Empty, result);
    }

    // ── 単一ブロック ──────────────────────────────────────────────────

    [Fact]
    public void Convert_SingleBlock_ReturnsSingleResult()
    {
        var converter = CreateConverter(new StubStrategy(BlockType.Paragraph, "content"));

        var result = converter.Convert([MakeParagraphBlock()]);

        Assert.Equal("content", result);
    }

    // ── 複数ブロック ──────────────────────────────────────────────────

    [Fact]
    public void Convert_MultipleBlocks_JoinsResultsWithNewline()
    {
        var converter = CreateConverter(new StubStrategy(BlockType.Paragraph, "line"));

        var result = converter.Convert([MakeParagraphBlock(), MakeParagraphBlock()]);

        Assert.Equal("line\nline", result);
    }

    [Fact]
    public void Convert_ThreeBlocks_JoinsWithTwoNewlines()
    {
        var converter = CreateConverter(new StubStrategy(BlockType.Paragraph, "x"));

        var result = converter.Convert([MakeParagraphBlock(), MakeParagraphBlock(), MakeParagraphBlock()]);

        Assert.Equal("x\nx\nx", result);
    }

    // ── コンテキスト設定 ──────────────────────────────────────────────

    [Fact]
    public void Convert_SetsCurrentBlockOnContext()
    {
        var capturing = new CapturingStrategy(BlockType.Paragraph);
        var converter = CreateConverter(capturing);
        var block1 = MakeParagraphBlock();
        var block2 = MakeParagraphBlock();

        converter.Convert([block1, block2]);

        Assert.Equal(2, capturing.Calls.Count);
        Assert.Same(block1, capturing.Calls[0].Block);
        Assert.Same(block2, capturing.Calls[1].Block);
    }

    [Fact]
    public void Convert_SetsCurrentBlockIndexOnContext()
    {
        var capturing = new CapturingStrategy(BlockType.Paragraph);
        var converter = CreateConverter(capturing);

        converter.Convert([MakeParagraphBlock(), MakeParagraphBlock(), MakeParagraphBlock()]);

        Assert.Equal(0, capturing.Calls[0].Index);
        Assert.Equal(1, capturing.Calls[1].Index);
        Assert.Equal(2, capturing.Calls[2].Index);
    }

    [Fact]
    public void Convert_SetsAllBlocksOnContext()
    {
        var capturing = new CapturingStrategy(BlockType.Paragraph);
        var converter = CreateConverter(capturing);
        var blocks = new List<NotionBlock> { MakeParagraphBlock(), MakeParagraphBlock() };

        converter.Convert(blocks);

        // コンテキストの Blocks は変換対象リスト全体を参照している
        Assert.Same(blocks, capturing.Calls[0].AllBlocks);
        Assert.Same(blocks, capturing.Calls[1].AllBlocks);
    }

    // ── ExecuteTransformBlocks ────────────────────────────────────────

    [Fact]
    public void Convert_ExecuteTransformBlocks_ReturnsEmptyForEmptyList()
    {
        // コンテキストの ExecuteTransformBlocks が空リストに対して空文字を返すことを確認
        var executorCapturing = new ExecutorCapturingStrategy(BlockType.Paragraph);
        var converter = CreateConverter(executorCapturing);

        converter.Convert([MakeParagraphBlock()]);

        Assert.NotNull(executorCapturing.CapturedExecutor);
        var result = executorCapturing.CapturedExecutor!([]);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Convert_ExecuteTransformBlocks_DelegatesToConvertRecursively()
    {
        // ExecuteTransformBlocks が Convert と同等の動作をすることを確認
        var executorCapturing = new ExecutorCapturingStrategy(BlockType.Paragraph);
        var converter = CreateConverter(
            executorCapturing,
            new StubStrategy(BlockType.Heading_1, "child")
        );

        converter.Convert([MakeParagraphBlock()]);

        // ExecuteTransformBlocks に別のブロックリストを渡すと Convert と同じ結果を返す
        var headingBlock = new NotionBlock(new HeadingOneBlock { Heading_1 = new HeadingOneBlock.Info { RichText = [] } });
        var childResult = executorCapturing.CapturedExecutor!([headingBlock]);
        Assert.Equal("child", childResult);
    }

    // ── 未知のブロックタイプ ──────────────────────────────────────────

    [Fact]
    public void Convert_UnknownBlockType_ReturnsEmptyFromDefaultStrategy()
    {
        // ストラテジー未登録のブロックはデフォルトストラテジーが処理し、空文字を返す
        var converter = CreateConverter(); // ストラテジーなし

        var result = converter.Convert([MakeParagraphBlock()]);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Convert_MixedKnownAndUnknownBlocks_JoinsAll()
    {
        var converter = CreateConverter(new StubStrategy(BlockType.Paragraph, "known"));
        var headingBlock = new NotionBlock(new HeadingOneBlock { Heading_1 = new HeadingOneBlock.Info { RichText = [] } });

        // known + unknown(空文字) の2ブロック
        var result = converter.Convert([MakeParagraphBlock(), headingBlock]);

        Assert.Equal("known\n", result);
    }
}
