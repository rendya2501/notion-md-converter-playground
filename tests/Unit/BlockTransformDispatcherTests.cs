using Notion.Client;
using NotionMarkdownConverter.Shared.Models;
using NotionMarkdownConverter.Transform;
using NotionMarkdownConverter.Transform.Context;
using NotionMarkdownConverter.Transform.Strategies;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// BlockTransformDispatcherのユニットテスト
/// </summary>
public class BlockTransformDispatcherTests
{
    // ── スタブ ────────────────────────────────────────────────────────

    /// <summary>
    /// 指定したBlockTypeに対して固定の文字列を返すスタブストラテジー
    /// </summary>
    private sealed class StubStrategy(BlockType blockType, string output) : IBlockTransformStrategy
    {
        public BlockType BlockType => blockType;
        public string Transform(NotionBlockTransformContext context) => output;
    }

    /// <summary>
    /// 呼び出し時のコンテキストを記録するスタブストラテジー
    /// </summary>
    private sealed class CapturingStrategy(BlockType blockType) : IBlockTransformStrategy
    {
        public BlockType BlockType => blockType;
        public NotionBlockTransformContext? CapturedContext { get; private set; }

        public string Transform(NotionBlockTransformContext context)
        {
            CapturedContext = context;
            return "captured";
        }
    }

    // ── ヘルパー ──────────────────────────────────────────────────────

    private static NotionBlock MakeParagraphBlock()
    {
        var block = new ParagraphBlock { Paragraph = new ParagraphBlock.Info { RichText = [] } };
        return new NotionBlock(block);
    }

    private static NotionBlock MakeHeadingOneBlock()
    {
        var block = new HeadingOneBlock { Heading_1 = new HeadingOneBlock.Info { RichText = [] } };
        return new NotionBlock(block);
    }

    private static NotionBlockTransformContext MakeContext(NotionBlock block) => new()
    {
        ExecuteTransformBlocks = _ => string.Empty,
        Blocks = [block],
        CurrentBlock = block,
        CurrentBlockIndex = 0
    };

    // ── テスト ────────────────────────────────────────────────────────

    [Fact]
    public void Transform_MatchingStrategy_ReturnsStrategyOutput()
    {
        var strategy = new StubStrategy(BlockType.Paragraph, "paragraph-output");
        var dispatcher = new BlockTransformDispatcher([strategy], new DefaultTransformStrategy());

        var result = dispatcher.Transform(MakeContext(MakeParagraphBlock()));

        Assert.Equal("paragraph-output", result);
    }

    [Fact]
    public void Transform_NoMatchingStrategy_UsesDefaultAndReturnsEmpty()
    {
        // Paragraph用のストラテジーを登録しない
        var dispatcher = new BlockTransformDispatcher([], new DefaultTransformStrategy());

        var result = dispatcher.Transform(MakeContext(MakeParagraphBlock()));

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Transform_PassesContextUnchangedToStrategy()
    {
        var capturingStrategy = new CapturingStrategy(BlockType.Paragraph);
        var dispatcher = new BlockTransformDispatcher([capturingStrategy], new DefaultTransformStrategy());
        var context = MakeContext(MakeParagraphBlock());

        dispatcher.Transform(context);

        Assert.Same(context, capturingStrategy.CapturedContext);
    }

    [Fact]
    public void Transform_MultipleStrategies_SelectsCorrectStrategyByBlockType()
    {
        var strategies = new IBlockTransformStrategy[]
        {
            new StubStrategy(BlockType.Paragraph, "paragraph"),
            new StubStrategy(BlockType.Heading_1, "heading1"),
        };
        var dispatcher = new BlockTransformDispatcher(strategies, new DefaultTransformStrategy());

        var paragraphResult = dispatcher.Transform(MakeContext(MakeParagraphBlock()));
        var headingResult = dispatcher.Transform(MakeContext(MakeHeadingOneBlock()));

        Assert.Equal("paragraph", paragraphResult);
        Assert.Equal("heading1", headingResult);
    }

    [Fact]
    public void Transform_DuplicateBlockTypeInStrategies_ThrowsOnConstruction()
    {
        // 同じBlockTypeのストラテジーを2つ登録するとDictionaryがArgumentExceptionをスローする
        var strategies = new IBlockTransformStrategy[]
        {
            new StubStrategy(BlockType.Paragraph, "first"),
            new StubStrategy(BlockType.Paragraph, "second"),
        };

        Assert.Throws<ArgumentException>(() =>
            new BlockTransformDispatcher(strategies, new DefaultTransformStrategy()));
    }
}
