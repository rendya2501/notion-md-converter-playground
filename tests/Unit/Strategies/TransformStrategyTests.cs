using Notion.Client;
using NotionMarkdownConverter.Domain.Models;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies;

namespace NotionMarkdownConverter.Tests.Unit.Strategies;

/// <summary>
/// 各 TransformStrategy のユニットテスト。
/// 「子ブロックなし」の変換結果と、StrategyがBlockTypeを正しく宣言しているかを検証します。
/// 子ブロックを含む変換パスは IntegrationTests でカバーします。
/// </summary>
public class TransformStrategyTests
{
    // ── ヘルパー ──────────────────────────────────────────────────────

    private static RichTextText MakeText(string text) =>
        new() { PlainText = text, Text = new Text { Content = text }, Annotations = new Annotations() };

    /// <summary>
    /// NotionBlock を生成します。HasChildren は Block.HasChildren プロパティで制御します。
    /// </summary>
    private static NotionBlock Wrap(Block block, bool hasChildren = false)
    {
        block.HasChildren = hasChildren;
        return new NotionBlock(block);
    }

    private static NotionBlockTransformContext MakeContext(
        NotionBlock block,
        List<NotionBlock>? allBlocks = null,
        int index = 0,
        Func<List<NotionBlock>, string>? executeTransform = null) => new()
        {
            ExecuteTransformBlocks = executeTransform ?? (_ => string.Empty),
            Blocks = allBlocks ?? [block],
            CurrentBlock = block,
            CurrentBlockIndex = index
        };

    // ── BlockType 宣言 ────────────────────────────────────────────────

    [Fact]
    public void Paragraph_BlockType_IsParagraph()
    {
        Assert.Equal(BlockType.Paragraph, new ParagraphTransformStrategy().BlockType);
    }

    [Fact]
    public void HeadingOne_BlockType_IsHeading1()
    {
        Assert.Equal(BlockType.Heading_1, new HeadingOneTransformStrategy().BlockType);
    }

    [Fact]
    public void Code_BlockType_IsCode()
    {
        Assert.Equal(BlockType.Code, new CodeTransformStrategy().BlockType);
    }

    [Fact]
    public void BulletedList_BlockType_IsBulletedListItem()
    {
        Assert.Equal(BlockType.BulletedListItem, new BulletedListItemTransformStrategy().BlockType);
    }

    [Fact]
    public void NumberedList_BlockType_IsNumberedListItem()
    {
        Assert.Equal(BlockType.NumberedListItem, new NumberedListItemTransformStrategy().BlockType);
    }

    // ── ParagraphTransformStrategy ────────────────────────────────────

    [Fact]
    public void Paragraph_SimpleText_ReturnsTextWithTrailingSpaces()
    {
        var block = Wrap(new ParagraphBlock
        {
            Paragraph = new ParagraphBlock.Info { RichText = [MakeText("段落テキスト")] }
        });
        var result = new ParagraphTransformStrategy().Transform(MakeContext(block));
        // ApplyLineBreaks がトレイリングスペース2つを付与する
        Assert.Equal("段落テキスト  ", result);
    }

    [Fact]
    public void Paragraph_EmptyRichText_ReturnsEmptyString()
    {
        var block = Wrap(new ParagraphBlock
        {
            Paragraph = new ParagraphBlock.Info { RichText = [] }
        });
        var result = new ParagraphTransformStrategy().Transform(MakeContext(block));
        // 空テキストは空行として出力（ApplyLineBreaks は空白のみの行をスキップ）
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Paragraph_WithChildren_CombinesTextAndChildren()
    {
        var block = Wrap(new ParagraphBlock
        {
            Paragraph = new ParagraphBlock.Info { RichText = [MakeText("親テキスト")] }
        }, hasChildren: true);

        var result = new ParagraphTransformStrategy().Transform(
            MakeContext(block, executeTransform: _ => "子コンテンツ"));

        Assert.Contains("親テキスト", result);
        Assert.Contains("子コンテンツ", result);
    }

    // ── HeadingOneTransformStrategy ───────────────────────────────────

    [Fact]
    public void HeadingOne_Transform_ReturnsH1Markdown()
    {
        var block = Wrap(new HeadingOneBlock
        {
            Heading_1 = new HeadingOneBlock.Info { RichText = [MakeText("見出し1")] }
        });
        Assert.Equal("# 見出し1", new HeadingOneTransformStrategy().Transform(MakeContext(block)));
    }

    // ── HeadingTwoTransformStrategy ───────────────────────────────────

    [Fact]
    public void HeadingTwo_Transform_ReturnsH2Markdown()
    {
        var block = Wrap(new HeadingTwoBlock
        {
            Heading_2 = new HeadingTwoBlock.Info { RichText = [MakeText("見出し2")] }
        });
        Assert.Equal("## 見出し2", new HeadingTwoTransformStrategy().Transform(MakeContext(block)));
    }

    // ── HeadingThreeTransformStrategy ─────────────────────────────────

    [Fact]
    public void HeadingThree_Transform_ReturnsH3Markdown()
    {
        var block = Wrap(new HeadingThreeBlock
        {
            Heading_3 = new HeadingThreeBlock.Info { RichText = [MakeText("見出し3")] }
        });
        Assert.Equal("### 見出し3", new HeadingThreeTransformStrategy().Transform(MakeContext(block)));
    }

    // ── CodeTransformStrategy ─────────────────────────────────────────

    [Fact]
    public void Code_WithLanguage_ReturnsCodeFenceWithLanguage()
    {
        var block = Wrap(new CodeBlock
        {
            Code = new CodeBlock.Info { RichText = [MakeText("var x = 1;")], Language = "csharp" }
        });
        Assert.Equal("```csharp\nvar x = 1;\n```",
            new CodeTransformStrategy().Transform(MakeContext(block)));
    }

    [Fact]
    public void Code_EmptyLanguage_ReturnsCodeFenceWithoutLanguage()
    {
        var block = Wrap(new CodeBlock
        {
            Code = new CodeBlock.Info { RichText = [MakeText("echo hello")], Language = string.Empty }
        });
        Assert.Equal("```\necho hello\n```",
            new CodeTransformStrategy().Transform(MakeContext(block)));
    }

    // ── BulletedListItemTransformStrategy ─────────────────────────────

    [Fact]
    public void BulletedList_SimpleItem_ReturnsBulletWithTrailingSpaces()
    {
        var block = Wrap(new BulletedListItemBlock
        {
            BulletedListItem = new BulletedListItemBlock.Info { RichText = [MakeText("項目テキスト")] }
        });
        var result = new BulletedListItemTransformStrategy().Transform(MakeContext(block));
        Assert.Equal("- 項目テキスト  ", result);
    }

    [Fact]
    public void BulletedList_WithChildren_IndentsChildContent()
    {
        var block = Wrap(new BulletedListItemBlock
        {
            BulletedListItem = new BulletedListItemBlock.Info { RichText = [MakeText("親項目")] }
        }, hasChildren: true);

        var result = new BulletedListItemTransformStrategy().Transform(
            MakeContext(block, executeTransform: _ => "- 子項目  "));

        Assert.Contains("- 親項目", result);
        // 子はデフォルト2スペースでインデント
        var lines = result.Split('\n');
        Assert.True(lines[1].StartsWith("  "), $"Expected indented child, got: '{lines[1]}'");
    }

    // ── NumberedListItemTransformStrategy ─────────────────────────────

    [Fact]
    public void NumberedList_FirstItem_StartsAtOne()
    {
        var block = Wrap(new NumberedListItemBlock
        {
            NumberedListItem = new NumberedListItemBlock.Info { RichText = [MakeText("項目")] }
        });
        var result = new NumberedListItemTransformStrategy().Transform(MakeContext(block));
        Assert.StartsWith("1. ", result);
    }

    [Fact]
    public void NumberedList_SecondConsecutiveItem_HasNumberTwo()
    {
        var block1 = Wrap(new NumberedListItemBlock
        {
            NumberedListItem = new NumberedListItemBlock.Info { RichText = [MakeText("1つ目")] }
        });
        var block2 = Wrap(new NumberedListItemBlock
        {
            NumberedListItem = new NumberedListItemBlock.Info { RichText = [MakeText("2つ目")] }
        });
        var context = MakeContext(block2, allBlocks: [block1, block2], index: 1);
        var result = new NumberedListItemTransformStrategy().Transform(context);
        Assert.StartsWith("2. ", result);
    }

    [Fact]
    public void NumberedList_ThreeConsecutiveItems_CountsCorrectly()
    {
        var blocks = Enumerable.Range(0, 3).Select(i =>
            Wrap(new NumberedListItemBlock
            {
                NumberedListItem = new NumberedListItemBlock.Info { RichText = [MakeText($"項目{i + 1}")] }
            })).ToList();

        var results = blocks.Select((b, i) =>
            new NumberedListItemTransformStrategy().Transform(
                MakeContext(b, allBlocks: blocks, index: i))).ToList();

        Assert.StartsWith("1. ", results[0]);
        Assert.StartsWith("2. ", results[1]);
        Assert.StartsWith("3. ", results[2]);
    }

    [Fact]
    public void NumberedList_AfterDifferentBlockType_ResetsToOne()
    {
        // 異なるブロックタイプの後ではカウンターがリセットされる
        var paraBlock = Wrap(new ParagraphBlock
        {
            Paragraph = new ParagraphBlock.Info { RichText = [MakeText("段落")] }
        });
        var numBlock = Wrap(new NumberedListItemBlock
        {
            NumberedListItem = new NumberedListItemBlock.Info { RichText = [MakeText("1つ目")] }
        });
        var context = MakeContext(numBlock, allBlocks: [paraBlock, numBlock], index: 1);
        var result = new NumberedListItemTransformStrategy().Transform(context);
        Assert.StartsWith("1. ", result);
    }

    [Fact]
    public void NumberedList_WithChildren_IndentsChildContent()
    {
        var block = Wrap(new NumberedListItemBlock
        {
            NumberedListItem = new NumberedListItemBlock.Info { RichText = [MakeText("親")] }
        }, hasChildren: true);

        var result = new NumberedListItemTransformStrategy().Transform(
            MakeContext(block, executeTransform: _ => "- 子項目"));

        Assert.Contains("1. 親", result);
        // 子は3スペースでインデント（"1. " の幅に合わせる）
        Assert.Contains("   - 子項目", result);
    }

    // ── DividerTransformStrategy ──────────────────────────────────────

    [Fact]
    public void Divider_Transform_ReturnsHorizontalRule()
    {
        var block = Wrap(new DividerBlock());
        Assert.Equal("---", new DividerTransformStrategy().Transform(MakeContext(block)));
    }

    // ── QuoteTransformStrategy ────────────────────────────────────────

    [Fact]
    public void Quote_SimpleText_ReturnsBlockquote()
    {
        var block = Wrap(new QuoteBlock
        {
            Quote = new QuoteBlock.Info { RichText = [MakeText("引用テキスト")] }
        });
        var result = new QuoteTransformStrategy().Transform(MakeContext(block));
        Assert.StartsWith("> ", result);
        Assert.Contains("引用テキスト", result);
    }

    // ── CalloutTransformStrategy ──────────────────────────────────────

    [Fact]
    public void Callout_SimpleText_ReturnsBlockquote()
    {
        var block = Wrap(new CalloutBlock
        {
            Callout = new CalloutBlock.Info { RichText = [MakeText("コールアウト")] }
        });
        var result = new CalloutTransformStrategy().Transform(MakeContext(block));
        Assert.StartsWith("> ", result);
        Assert.Contains("コールアウト", result);
    }

    // ── TodoListItemTransformStrategy ─────────────────────────────────

    [Fact]
    public void TodoList_Checked_ReturnsCheckedCheckbox()
    {
        var block = Wrap(new ToDoBlock
        {
            ToDo = new ToDoBlock.Info { RichText = [MakeText("完了タスク")], IsChecked = true }
        });
        var result = new TodoListItemTransformStrategy().Transform(MakeContext(block));
        Assert.Contains("[x]", result);
        Assert.Contains("完了タスク", result);
    }

    [Fact]
    public void TodoList_Unchecked_ReturnsUncheckedCheckbox()
    {
        var block = Wrap(new ToDoBlock
        {
            ToDo = new ToDoBlock.Info { RichText = [MakeText("未完了タスク")], IsChecked = false }
        });
        var result = new TodoListItemTransformStrategy().Transform(MakeContext(block));
        Assert.Contains("[ ]", result);
        Assert.Contains("未完了タスク", result);
    }

    // ── ToggleTransformStrategy ───────────────────────────────────────

    [Fact]
    public void Toggle_Transform_ReturnsDetailsHtmlStructure()
    {
        var block = Wrap(new ToggleBlock
        {
            Toggle = new ToggleBlock.Info { RichText = [MakeText("トグルタイトル")] }
        }, hasChildren: true);
        block.Children = [];

        var result = new ToggleTransformStrategy().Transform(
            MakeContext(block, executeTransform: _ => "コンテンツ行"));

        Assert.Contains("<details>", result);
        Assert.Contains("<summary>", result);
        Assert.Contains("トグルタイトル", result);
        Assert.Contains("</summary>", result);
        Assert.Contains("コンテンツ行", result);
        Assert.Contains("</details>", result);
    }

    [Fact]
    public void Toggle_SummaryAppearsBeforeContent()
    {
        var block = Wrap(new ToggleBlock
        {
            Toggle = new ToggleBlock.Info { RichText = [MakeText("タイトル")] }
        });
        var result = new ToggleTransformStrategy().Transform(
            MakeContext(block, executeTransform: _ => "コンテンツ"));

        Assert.True(result.IndexOf("<summary>", StringComparison.Ordinal)
            < result.IndexOf("コンテンツ", StringComparison.Ordinal));
    }

    // ── 空文字を返すストラテジー群 ────────────────────────────────────

    [Fact]
    public void Breadcrumb_Transform_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty,
            new BreadcrumbTransformStrategy().Transform(
                MakeContext(Wrap(new BreadcrumbBlock()))));
    }

    [Fact]
    public void TableOfContents_Transform_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty,
            new TableOfContentsTransformStrategy().Transform(
                MakeContext(Wrap(new TableOfContentsBlock()))));
    }

    [Fact]
    public void SyncedBlock_Transform_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty,
            new SyncedBlockTransformStrategy().Transform(
                MakeContext(Wrap(new SyncedBlockBlock()))));
    }

    [Fact]
    public void Default_Transform_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty,
            new DefaultTransformStrategy().Transform(
                MakeContext(Wrap(new ParagraphBlock
                {
                    Paragraph = new ParagraphBlock.Info { RichText = [] }
                }))));
    }
}
