using Notion.Client;
using NotionMarkdownConverter.Transform.Enums;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// MarkdownListUtilsのユニットテスト
/// </summary>
public class MarkdownListUtilsTests
{
    [Theory]
    [InlineData("テキスト", BulletStyle.Hyphen, "- テキスト")]
    [InlineData("テキスト", BulletStyle.Asterisk, "* テキスト")]
    [InlineData("テキスト", BulletStyle.Plus, "+ テキスト")]
    public void BulletList_EachStyle_ReturnsCorrectFormat(string text, BulletStyle style, string expected)
    {
        var result = MarkdownListUtils.BulletList(text, style);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BulletList_DefaultStyle_UsesHyphen()
    {
        var result = MarkdownListUtils.BulletList("テキスト");
        Assert.Equal("- テキスト", result);
    }

    [Theory]
    [InlineData("テキスト", 1, "1. テキスト")]
    [InlineData("テキスト", 5, "5. テキスト")]
    [InlineData("テキスト", 10, "10. テキスト")]
    public void NumberedList_ReturnsNumberPrefixed(string text, int number, string expected)
    {
        var result = MarkdownListUtils.NumberedList(text, number);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CheckList_Checked_ReturnsCheckedCheckbox()
    {
        var result = MarkdownListUtils.CheckList("タスク", true);
        Assert.Equal("- [x] タスク", result);
    }

    [Fact]
    public void CheckList_Unchecked_ReturnsUncheckedCheckbox()
    {
        var result = MarkdownListUtils.CheckList("タスク", false);
        Assert.Equal("- [ ] タスク", result);
    }
}

/// <summary>
/// MarkdownRichTextUtilsのユニットテスト
/// </summary>
public class MarkdownRichTextUtilsTests
{
    /// <summary>
    /// テスト用のシンプルなRichTextオブジェクトを生成します。
    /// </summary>
    private static RichTextText MakeText(
        string plainText,
        bool bold = false,
        bool italic = false,
        bool strikethrough = false,
        bool underline = false,
        bool isCode = false,
        string? href = null,
        Color color = Color.Default)
    {
        return new RichTextText
        {
            PlainText = plainText,
            Text = new Text { Content = plainText },
            Annotations = new Annotations
            {
                IsBold = bold,
                IsItalic = italic,
                IsStrikeThrough = strikethrough,
                IsUnderline = underline,
                IsCode = isCode,
                Color = color
            },
            Href = href
        };
    }

    /// <summary>
    /// テスト用のRichTextEquationオブジェクトを生成します。
    /// Notion.Clientでは Equation プロパティの型は独立した Equation クラスです。
    /// RichTextEquation.RichTextEquationInfo は存在しません。
    /// </summary>
    private static RichTextEquation MakeEquation(string expression) =>
        new()
        {
            PlainText = expression,
            Equation = new Equation { Expression = expression },
            Annotations = new Annotations()
        };

    [Fact]
    public void RichTextsToMarkdown_PlainText_ReturnsPlainText()
    {
        var richTexts = new List<RichTextBase> { MakeText("テキスト") };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("テキスト", result);
    }

    [Fact]
    public void RichTextsToMarkdown_BoldText_ReturnsBoldMarkdown()
    {
        var richTexts = new List<RichTextBase> { MakeText("テキスト", bold: true) };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("**テキスト**", result);
    }

    [Fact]
    public void RichTextsToMarkdown_ItalicText_ReturnsItalicMarkdown()
    {
        var richTexts = new List<RichTextBase> { MakeText("テキスト", italic: true) };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("*テキスト*", result);
    }

    [Fact]
    public void RichTextsToMarkdown_BoldAndItalic_ReturnsBoldItalicMarkdown()
    {
        var richTexts = new List<RichTextBase> { MakeText("テキスト", bold: true, italic: true) };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("***テキスト***", result);
    }

    [Fact]
    public void RichTextsToMarkdown_Strikethrough_ReturnsStrikethroughMarkdown()
    {
        var richTexts = new List<RichTextBase> { MakeText("テキスト", strikethrough: true) };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("~~テキスト~~", result);
    }

    [Fact]
    public void RichTextsToMarkdown_Underline_ReturnsUnderlineMarkdown()
    {
        var richTexts = new List<RichTextBase> { MakeText("テキスト", underline: true) };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("_テキスト_", result);
    }

    [Fact]
    public void RichTextsToMarkdown_Code_ReturnsInlineCodeMarkdown()
    {
        var richTexts = new List<RichTextBase> { MakeText("var x = 1;", isCode: true) };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("`var x = 1;`", result);
    }

    [Fact]
    public void RichTextsToMarkdown_WithHttpsLink_ReturnsLinkMarkdown()
    {
        var richTexts = new List<RichTextBase> { MakeText("リンク", href: "https://example.com") };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("[リンク](https://example.com)", result);
    }

    [Fact]
    public void RichTextsToMarkdown_WithNonHttpLink_DoesNotCreateLink()
    {
        // notion:// などのhrefはリンクにならない
        var richTexts = new List<RichTextBase> { MakeText("テキスト", href: "notion://page-id") };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("テキスト", result);
    }

    [Fact]
    public void RichTextsToMarkdown_MultipleRichTexts_ConcatenatesAll()
    {
        var richTexts = new List<RichTextBase>
        {
            MakeText("通常"),
            MakeText("太字", bold: true),
            MakeText("通常2")
        };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("通常**太字**通常2", result);
    }

    [Fact]
    public void RichTextsToMarkdown_EmptyList_ReturnsEmpty()
    {
        var result = MarkdownRichTextUtils.RichTextsToMarkdown([]);
        Assert.Equal("", result);
    }

    [Fact]
    public void RichTextsToMarkdown_ColorDisabledByDefault_DoesNotWrapSpan()
    {
        var richTexts = new List<RichTextBase> { MakeText("テキスト", color: Color.Red) };
        // デフォルトではColorは無効
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.DoesNotContain("<span", result);
        Assert.Equal("テキスト", result);
    }

    [Fact]
    public void RichTextsToMarkdown_ColorEnabledViaOptions_WrapsWithSpan()
    {
        var richTexts = new List<RichTextBase> { MakeText("テキスト", color: Color.Red) };
        var options = new MarkdownRichTextUtils.AnnotationOptions { Color = true };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts, options);
        Assert.Contains("<span", result);
    }

    [Fact]
    public void RichTextsToMarkdown_BoldDisabledViaOptions_DoesNotBold()
    {
        var richTexts = new List<RichTextBase> { MakeText("テキスト", bold: true) };
        var options = new MarkdownRichTextUtils.AnnotationOptions { Bold = false };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts, options);
        Assert.Equal("テキスト", result);
    }

    // ── インライン数式（RichTextEquation型）────────────────────────────
    // MarkdownRichTextUtils内の以下の分岐が未テストだったため追加：
    //   if (text.Type == RichTextType.Equation && enableAnnotations.Equation)
    //       markdown = MarkdownInlineUtils.InlineEquation(markdown);

    [Fact]
    public void RichTextsToMarkdown_EquationRichText_ReturnsInlineEquation()
    {
        var richTexts = new List<RichTextBase> { MakeEquation("E=mc^2") };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("$E=mc^2$", result);
    }

    [Fact]
    public void RichTextsToMarkdown_EquationDisabledViaOptions_ReturnsPlainText()
    {
        // Equation = false のとき数式マークアップは適用されない
        var richTexts = new List<RichTextBase> { MakeEquation("E=mc^2") };
        var options = new MarkdownRichTextUtils.AnnotationOptions { Equation = false };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts, options);
        // 数式記号なしのプレーンテキストが返る
        Assert.Equal("E=mc^2", result);
        Assert.DoesNotContain("$", result);
    }

    [Fact]
    public void RichTextsToMarkdown_MixedTextAndEquation_ConcatenatesCorrectly()
    {
        // 通常テキストとインライン数式が混在するケース（実際のNotionページで頻出）
        var richTexts = new List<RichTextBase>
        {
            MakeText("質量エネルギー等価式は "),
            MakeEquation("E=mc^2"),
            MakeText(" です。")
        };
        var result = MarkdownRichTextUtils.RichTextsToMarkdown(richTexts);
        Assert.Equal("質量エネルギー等価式は $E=mc^2$ です。", result);
    }
}
