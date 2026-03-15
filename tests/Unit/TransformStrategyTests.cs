using Notion.Client;
using NotionMarkdownConverter.Shared.Constants;
using NotionMarkdownConverter.Shared.Models;
using NotionMarkdownConverter.Transform.Strategies;
using NotionMarkdownConverter.Transform.Strategies.Context;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// 各 TransformStrategy のユニットテスト。
/// 「子ブロックなし」の変換結果と、StrategyがBlockTypeを正しく宣言しているかを検証します。
/// 子ブロックを含む変換パスは IntegrationTests でカバーします。
/// </summary>
public class TransformStrategyTests
{
    // ── ヘルパー ──────────────────────────────────────────────────────

    private static RichTextText MakeText(string text) =>
        new()
        {
            PlainText = text,
            Text = new Text { Content = text },
            Annotations = new Annotations()
        };

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

    [Fact]
    public void Quote_WithChildren_CombinesTextAndChildrenInBlockquote()
    {
        var block = Wrap(new QuoteBlock
        {
            Quote = new QuoteBlock.Info { RichText = [MakeText("引用テキスト")] }
        }, hasChildren: true);

        var result = new QuoteTransformStrategy().Transform(
            MakeContext(block, executeTransform: _ => "子コンテンツ"));

        Assert.StartsWith("> ", result);
        Assert.Contains("引用テキスト", result);
        Assert.Contains("子コンテンツ", result);
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

    [Fact]
    public void Callout_WithChildren_CombinesTextAndChildrenInBlockquote()
    {
        var block = Wrap(new CalloutBlock
        {
            Callout = new CalloutBlock.Info { RichText = [MakeText("コールアウト")] }
        }, hasChildren: true);

        var result = new CalloutTransformStrategy().Transform(
            MakeContext(block, executeTransform: _ => "子コンテンツ"));

        Assert.StartsWith("> ", result);
        Assert.Contains("コールアウト", result);
        Assert.Contains("子コンテンツ", result);
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

    [Fact]
    public void TodoList_WithChildren_IndentsChildContent()
    {
        var block = Wrap(new ToDoBlock
        {
            ToDo = new ToDoBlock.Info { RichText = [MakeText("親タスク")], IsChecked = false }
        }, hasChildren: true);

        var result = new TodoListItemTransformStrategy().Transform(
            MakeContext(block, executeTransform: _ => "- 子項目  "));

        Assert.Contains("[ ]", result);
        Assert.Contains("親タスク", result);
        var lines = result.Split('\n');
        Assert.True(lines[1].StartsWith("  "), $"子はデフォルト2スペースでインデントされるべき: '{lines[1]}'");
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

    // ── BookmarkTransformStrategy ─────────────────────────────────────

    [Fact]
    public void Bookmark_BlockType_IsBookmark()
    {
        Assert.Equal(BlockType.Bookmark, new BookmarkTransformStrategy().BlockType);
    }

    [Fact]
    public void Bookmark_WithCaption_UsesCaptionAsLinkText()
    {
        var block = Wrap(new BookmarkBlock
        {
            Bookmark = new BookmarkBlock.Info
            {
                Url = "https://example.com",
                Caption = [MakeText("キャプション")]
            }
        });

        var result = new BookmarkTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("[キャプション](https://example.com)", result);
    }

    [Fact]
    public void Bookmark_WithoutCaption_UsesUrlAsLinkText()
    {
        var block = Wrap(new BookmarkBlock
        {
            Bookmark = new BookmarkBlock.Info
            {
                Url = "https://example.com",
                Caption = []
            }
        });

        var result = new BookmarkTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("[https://example.com](https://example.com)", result);
    }

    // ── EmbedTransformStrategy ────────────────────────────────────────

    [Fact]
    public void Embed_BlockType_IsEmbed()
    {
        Assert.Equal(BlockType.Embed, new EmbedTransformStrategy().BlockType);
    }

    [Fact]
    public void Embed_WithCaption_UsesCaptionAsLinkText()
    {
        var block = Wrap(new EmbedBlock
        {
            Embed = new EmbedBlock.Info
            {
                Url = "https://example.com/embed",
                Caption = [MakeText("埋め込みキャプション")]
            }
        });

        var result = new EmbedTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("[埋め込みキャプション](https://example.com/embed)", result);
    }

    [Fact]
    public void Embed_WithoutCaption_ReturnsUrlAsIs()
    {
        var block = Wrap(new EmbedBlock
        {
            Embed = new EmbedBlock.Info
            {
                Url = "https://example.com/embed",
                Caption = []
            }
        });

        var result = new EmbedTransformStrategy().Transform(MakeContext(block));

        Assert.Equal("https://example.com/embed", result);
    }

    // ── EquationTransformStrategy ─────────────────────────────────────

    [Fact]
    public void Equation_BlockType_IsEquation()
    {
        Assert.Equal(BlockType.Equation, new EquationTransformStrategy().BlockType);
    }

    [Fact]
    public void Equation_Transform_ReturnsBlockEquation()
    {
        var block = Wrap(new EquationBlock
        {
            Equation = new EquationBlock.Info { Expression = "E = mc^2" }
        });

        var result = new EquationTransformStrategy().Transform(MakeContext(block));

        Assert.Equal("\n$$\nE = mc^2\n$$\n", result);
    }

    // ── ImageTransformStrategy ────────────────────────────────────────

    [Fact]
    public void Image_BlockType_IsImage()
    {
        Assert.Equal(BlockType.Image, new ImageTransformStrategy().BlockType);
    }

    [Fact]
    public void Image_ExternalFile_ReturnsImageTagWithUrl()
    {
        var image = new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://example.com/image.png" },
            Caption = []
        };
        var block = Wrap(new ImageBlock { Image = image });

        var result = new ImageTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("![](https://example.com/image.png)", result);
    }

    [Fact]
    public void Image_ExternalFile_WithCaption_UsesCaptionAsAltText()
    {
        var image = new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://example.com/image.png" },
            Caption = [MakeText("alt テキスト")]
        };
        var block = Wrap(new ImageBlock { Image = image });

        var result = new ImageTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("![alt テキスト](https://example.com/image.png)", result);
    }

    [Fact]
    public void Image_UploadedFile_ReturnsImageTagWithDownloadMarker()
    {
        var image = new UploadedFile
        {
            Caption = [],
            File = new() { Url = "https://cdn.notion.so/image.png" }
        };
        var block = Wrap(new ImageBlock { Image = image });

        var result = new ImageTransformStrategy().Transform(MakeContext(block));

        Assert.Contains(LinkConstants.DownloadMarker, result);
        Assert.Contains("https://cdn.notion.so/image.png", result);
    }

    [Fact]
    public void Image_UploadedFile_WithCaption_UsesCaptionAsAltText()
    {
        // ExternalFile はキャプションあり・なし両方テスト済み。UploadedFile も同様に網羅する。
        var image = new UploadedFile
        {
            Caption = [MakeText("アップロード画像")],
            File = new() { Url = "https://cdn.notion.so/image.png" }
        };
        var block = Wrap(new ImageBlock { Image = image });

        var result = new ImageTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("アップロード画像", result);
        Assert.Contains(LinkConstants.DownloadMarker, result);
    }

    // ── FileTransformStrategy ─────────────────────────────────────────

    [Fact]
    public void File_BlockType_IsFile()
    {
        Assert.Equal(BlockType.File, new FileTransformStrategy().BlockType);
    }

    [Fact]
    public void File_ExternalFile_WithoutCaption_UsesNameAsLinkText()
    {
        var file = new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://example.com/doc.pdf" },
            Caption = [],
            Name = "doc.pdf"
        };
        var block = Wrap(new FileBlock { File = file });

        var result = new FileTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("[doc.pdf](https://example.com/doc.pdf)", result);
    }

    [Fact]
    public void File_ExternalFile_WithCaption_UsesCaptionAsLinkText()
    {
        var file = new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://example.com/doc.pdf" },
            Caption = [MakeText("ドキュメント")],
            Name = "doc.pdf"
        };
        var block = Wrap(new FileBlock { File = file });

        var result = new FileTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("[ドキュメント](https://example.com/doc.pdf)", result);
    }

    [Fact]
    public void File_UploadedFile_ReturnsLinkWithDownloadMarker()
    {
        var file = new UploadedFile
        {
            Caption = [],
            Name = "uploaded.pdf",
            File = new() { Url = "https://cdn.notion.so/uploaded.pdf" }
        };
        var block = Wrap(new FileBlock { File = file });

        var result = new FileTransformStrategy().Transform(MakeContext(block));

        Assert.Contains(LinkConstants.DownloadMarker, result);
        Assert.Contains("https://cdn.notion.so/uploaded.pdf", result);
    }

    [Fact]
    public void File_UploadedFile_WithCaption_UsesCaptionAsLinkText()
    {
        // PDF は UploadedFile+キャプションあり までテスト済み。File も同様に網羅する。
        var file = new UploadedFile
        {
            Caption = [MakeText("アップロードファイル")],
            Name = "uploaded.pdf",
            File = new() { Url = "https://cdn.notion.so/uploaded.pdf" }
        };
        var block = Wrap(new FileBlock { File = file });

        var result = new FileTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("アップロードファイル", result);
        Assert.Contains(LinkConstants.DownloadMarker, result);
    }

    // ── LinkPreviewTransformStrategy ──────────────────────────────────

    [Fact]
    public void LinkPreview_BlockType_IsLinkPreview()
    {
        Assert.Equal(BlockType.LinkPreview, new LinkPreviewTransformStrategy().BlockType);
    }

    [Fact]
    public void LinkPreview_Transform_ReturnsUrlAsIs()
    {
        var block = Wrap(new LinkPreviewBlock
        {
            LinkPreview = new LinkPreviewBlock.Data { Url = "https://example.com" }
        });

        var result = new LinkPreviewTransformStrategy().Transform(MakeContext(block));

        Assert.Equal("https://example.com", result);
    }

    // ── PDFTransformStrategy ──────────────────────────────────────────

    [Fact]
    public void PDF_BlockType_IsPdf()
    {
        Assert.Equal(BlockType.PDF, new PDFTransformStrategy().BlockType);
    }

    [Fact]
    public void PDF_ExternalFile_UsesUrlAsDisplayText()
    {
        var pdf = new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://example.com/doc.pdf" },
            Caption = []
        };
        var block = Wrap(new PDFBlock { PDF = pdf });

        var result = new PDFTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("[https://example.com/doc.pdf](https://example.com/doc.pdf)", result);
    }

    [Fact]
    public void PDF_ExternalFile_WithCaption_UsesCaptionAsDisplayText()
    {
        var pdf = new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://example.com/doc.pdf" },
            Caption = [MakeText("資料")]
        };
        var block = Wrap(new PDFBlock { PDF = pdf });

        var result = new PDFTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("[資料](https://example.com/doc.pdf)", result);
    }

    [Fact]
    public void PDF_UploadedFile_UsesFilenameAsDisplayText()
    {
        var pdf = new UploadedFile
        {
            Caption = [],
            File = new() { Url = "https://cdn.notion.so/documents/report.pdf" }
        };
        var block = Wrap(new PDFBlock { PDF = pdf });

        var result = new PDFTransformStrategy().Transform(MakeContext(block));

        // UploadedFile の場合、URLからファイル名を抽出して表示テキストにします。
        Assert.Contains("[report.pdf]", result);
        Assert.Contains(LinkConstants.DownloadMarker, result);
    }

    [Fact]
    public void PDF_UploadedFile_WithCaption_UsesCaptionAsDisplayText()
    {
        var pdf = new UploadedFile
        {
            Caption = [MakeText("レポート")],
            File = new() { Url = "https://cdn.notion.so/documents/report.pdf" }
        };
        var block = Wrap(new PDFBlock { PDF = pdf });

        var result = new PDFTransformStrategy().Transform(MakeContext(block));

        Assert.Contains("[レポート]", result);
    }

    // ── ColumnListTransformStrategy ───────────────────────────────────

    [Fact]
    public void ColumnList_BlockType_IsColumnList()
    {
        Assert.Equal(BlockType.ColumnList, new ColumnListTransformStrategy().BlockType);
    }

    [Fact]
    public void ColumnList_TwoColumns_JoinsColumnContentsWithNewline()
    {
        // 2つのカラムをそれぞれ ExecuteTransformBlocks で変換し、"\n" で結合します。
        var col1 = new NotionBlock(new ParagraphBlock { Paragraph = new ParagraphBlock.Info { RichText = [] } })
        {
            Children = []
        };
        var col2 = new NotionBlock(new ParagraphBlock { Paragraph = new ParagraphBlock.Info { RichText = [] } })
        {
            Children = []
        };

        var columnList = Wrap(new ColumnListBlock());
        columnList.Children = [col1, col2];

        var callCount = 0;
        var result = new ColumnListTransformStrategy().Transform(
            MakeContext(columnList, executeTransform: _ => $"col{++callCount}"));

        Assert.Equal("col1\ncol2", result);
    }

    [Fact]
    public void ColumnList_EmptyColumns_ReturnsEmptyString()
    {
        var columnList = Wrap(new ColumnListBlock());
        columnList.Children = [];

        var result = new ColumnListTransformStrategy().Transform(MakeContext(columnList));

        Assert.Equal(string.Empty, result);
    }
}
