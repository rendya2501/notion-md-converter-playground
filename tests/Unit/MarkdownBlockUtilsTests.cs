using NotionMarkdownConverter.Domain.Markdown.Enums;
using NotionMarkdownConverter.Domain.Markdown.Utils;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// MarkdownBlockUtilsのユニットテスト
/// </summary>
public class MarkdownBlockUtilsTests
{
    // ── Heading ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("見出し", 1, "# 見出し")]
    [InlineData("見出し", 2, "## 見出し")]
    [InlineData("見出し", 3, "### 見出し")]
    [InlineData("見出し", 6, "###### 見出し")]
    public void Heading_ValidLevel_ReturnsCorrectMarkdown(string text, int level, string expected)
    {
        var result = MarkdownBlockUtils.Heading(text, level);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Heading_LevelBelowOne_ClampsToOne()
    {
        var result = MarkdownBlockUtils.Heading("見出し", 0);
        Assert.Equal("# 見出し", result);
    }

    [Fact]
    public void Heading_LevelAboveSix_ClampsToSix()
    {
        var result = MarkdownBlockUtils.Heading("見出し", 7);
        Assert.Equal("###### 見出し", result);
    }

    // ── CodeBlock ─────────────────────────────────────────────────────

    [Fact]
    public void CodeBlock_WithLanguage_ReturnsCodeFenceWithLanguage()
    {
        var result = MarkdownBlockUtils.CodeBlock("var x = 1;", "csharp");
        Assert.Equal("```csharp\nvar x = 1;\n```", result);
    }

    [Fact]
    public void CodeBlock_WithoutLanguage_ReturnsCodeFenceWithoutTrailingSpace()
    {
        var result = MarkdownBlockUtils.CodeBlock("var x = 1;");
        Assert.Equal("```\nvar x = 1;\n```", result);
    }

    // ── BlockEquation ─────────────────────────────────────────────────

    [Fact]
    public void BlockEquation_ReturnsDoubleDollarWrapped()
    {
        var result = MarkdownBlockUtils.BlockEquation("E = mc^2");
        Assert.Equal("$$\nE = mc^2\n$$", result);
    }

    // ── Blockquote ────────────────────────────────────────────────────

    [Fact]
    public void Blockquote_SingleLine_PrefixesWithAngle()
    {
        var result = MarkdownBlockUtils.Blockquote("引用文");
        Assert.Equal("> 引用文", result);
    }

    [Fact]
    public void Blockquote_MultiLine_PrefixesEachLine()
    {
        var result = MarkdownBlockUtils.Blockquote("1行目\n2行目\n3行目");
        Assert.Equal("> 1行目\n> 2行目\n> 3行目", result);
    }

    // ── HorizontalRule ────────────────────────────────────────────────

    [Fact]
    public void HorizontalRule_DefaultStyle_ReturnsHyphen()
    {
        var result = MarkdownBlockUtils.HorizontalRule();
        Assert.Equal("---", result);
    }

    [Theory]
    [InlineData(HorizontalRuleStyle.Hyphen, "---")]
    [InlineData(HorizontalRuleStyle.Asterisk, "***")]
    [InlineData(HorizontalRuleStyle.Underscore, "___")]
    public void HorizontalRule_EachStyle_ReturnsCorrectString(HorizontalRuleStyle style, string expected)
    {
        var result = MarkdownBlockUtils.HorizontalRule(style);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void HorizontalRule_NullStyle_FallsBackToHyphen()
    {
        var result = MarkdownBlockUtils.HorizontalRule(null);
        Assert.Equal("---", result);
    }

    // ── Indent ────────────────────────────────────────────────────────

    [Fact]
    public void Indent_DefaultSpaces_AddsTwoSpaces()
    {
        var result = MarkdownBlockUtils.Indent("テキスト");
        Assert.Equal("  テキスト", result);
    }

    [Fact]
    public void Indent_CustomSpaces_AddsCorrectSpaces()
    {
        var result = MarkdownBlockUtils.Indent("テキスト", 4);
        Assert.Equal("    テキスト", result);
    }

    [Fact]
    public void Indent_MultiLine_IndentsEachNonEmptyLine()
    {
        var result = MarkdownBlockUtils.Indent("1行目\n\n3行目", 2);
        Assert.Equal("  1行目\n\n  3行目", result);
    }

    [Fact]
    public void Indent_EmptyLine_DoesNotIndentEmptyLines()
    {
        var result = MarkdownBlockUtils.Indent("", 2);
        Assert.Equal("", result);
    }

    // ── LineBreak ─────────────────────────────────────────────────────

    [Fact]
    public void LineBreak_SingleLine_AppendsTrailingSpaces()
    {
        var result = MarkdownBlockUtils.ApplyLineBreaks("テキスト");
        Assert.Equal("テキスト  ", result);
    }

    [Fact]
    public void LineBreak_WhitespaceOnlyLine_DoesNotAppendSpaces()
    {
        var result = MarkdownBlockUtils.ApplyLineBreaks("   ");
        Assert.Equal("   ", result);
    }

    [Fact]
    public void LineBreak_BrStyle_JoinsWithBrTag()
    {
        var result = MarkdownBlockUtils.ApplyLineBreaks("1行\n2行", LineBreakStyle.BR);
        Assert.Contains("<BR>", result);
    }

    // ── Details ───────────────────────────────────────────────────────

    [Fact]
    public void Details_ReturnsDetailsTagStructure()
    {
        var result = MarkdownBlockUtils.Details("タイトル", "コンテンツ");

        Assert.Contains("<details>", result);
        Assert.Contains("<summary>", result);
        Assert.Contains("タイトル", result);
        Assert.Contains("</summary>", result);
        Assert.Contains("コンテンツ", result);
        Assert.Contains("</details>", result);
    }

    [Fact]
    public void Details_OrderIsCorrect()
    {
        var result = MarkdownBlockUtils.Details("タイトル", "コンテンツ");
        var detailsStart = result.IndexOf("<details>");
        var summaryStart = result.IndexOf("<summary>");
        var summaryEnd = result.IndexOf("</summary>");
        var detailsEnd = result.IndexOf("</details>");

        Assert.True(detailsStart < summaryStart);
        Assert.True(summaryStart < summaryEnd);
        Assert.True(summaryEnd < detailsEnd);
    }
}
