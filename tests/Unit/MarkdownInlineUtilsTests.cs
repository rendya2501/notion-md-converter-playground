using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// MarkdownInlineUtilsのユニットテスト
/// </summary>
public class MarkdownInlineUtilsTests
{
    // ── Decoration ────────────────────────────────────────────────────

    [Fact]
    public void Decoration_NormalText_WrapsWithDecoration()
    {
        var result = MarkdownInlineUtils.Decoration("テキスト", "**");
        Assert.Equal("**テキスト**", result);
    }

    [Fact]
    public void Decoration_TextWithLeadingSpace_PreservesLeadingSpace()
    {
        var result = MarkdownInlineUtils.Decoration(" テキスト", "**");
        Assert.Equal(" **テキスト**", result);
    }

    [Fact]
    public void Decoration_TextWithTrailingSpace_PreservesTrailingSpace()
    {
        var result = MarkdownInlineUtils.Decoration("テキスト ", "**");
        Assert.Equal("**テキスト** ", result);
    }

    [Fact]
    public void Decoration_WhitespaceOnly_ReturnsOriginal()
    {
        var result = MarkdownInlineUtils.Decoration("   ", "**");
        Assert.Equal("   ", result);
    }

    [Fact]
    public void Decoration_EmptyString_ReturnsEmpty()
    {
        var result = MarkdownInlineUtils.Decoration("", "**");
        Assert.Equal("", result);
    }

    // ── テキスト装飾 ──────────────────────────────────────────────────

    [Fact]
    public void Bold_ReturnsDoubleAsteriskWrapped()
    {
        Assert.Equal("**テキスト**", MarkdownInlineUtils.Bold("テキスト"));
    }

    [Fact]
    public void Italic_ReturnsSingleAsteriskWrapped()
    {
        Assert.Equal("*テキスト*", MarkdownInlineUtils.Italic("テキスト"));
    }

    [Fact]
    public void BoldItalic_ReturnsTripleAsteriskWrapped()
    {
        Assert.Equal("***テキスト***", MarkdownInlineUtils.BoldItalic("テキスト"));
    }

    [Fact]
    public void Strikethrough_ReturnsTildaWrapped()
    {
        Assert.Equal("~~テキスト~~", MarkdownInlineUtils.Strikethrough("テキスト"));
    }

    [Fact]
    public void InlineCode_ReturnsBacktickWrapped()
    {
        Assert.Equal("`コード`", MarkdownInlineUtils.InlineCode("コード"));
    }

    [Fact]
    public void Underline_ReturnsUnderscoreWrapped()
    {
        Assert.Equal("_テキスト_", MarkdownInlineUtils.Underline("テキスト"));
    }

    [Fact]
    public void InlineEquation_ReturnsDollarWrapped()
    {
        Assert.Equal("$E=mc^2$", MarkdownInlineUtils.InlineEquation("E=mc^2"));
    }

    // ── リンク・メディア ──────────────────────────────────────────────

    [Fact]
    public void Link_ReturnsMarkdownLinkSyntax()
    {
        var result = MarkdownInlineUtils.Link("テキスト", "https://example.com");
        Assert.Equal("[テキスト](https://example.com)", result);
    }

    [Fact]
    public void Image_WithoutWidth_ReturnsImageSyntax()
    {
        var result = MarkdownInlineUtils.Image("代替テキスト", "https://example.com/image.png");
        Assert.Equal("![代替テキスト](https://example.com/image.png)", result);
    }

    [Fact]
    public void Image_WithWidth_ReturnsImageSyntaxWithWidth()
    {
        var result = MarkdownInlineUtils.Image("代替テキスト", "https://example.com/image.png", "200");
        Assert.Equal("![代替テキスト](https://example.com/image.png =200x)", result);
    }

    [Fact]
    public void Comment_ReturnsHtmlComment()
    {
        var result = MarkdownInlineUtils.Comment("コメント");
        Assert.Equal("<!-- コメント -->", result);
    }

    [Fact]
    public void AppendTrailingSpaces_AppendsTwoTrailingSpaces()
    {
        var result = MarkdownInlineUtils.AppendTrailingSpaces("テキスト");
        Assert.Equal("テキスト  ", result);
    }

    // ── Color ─────────────────────────────────────────────────────────

    [Fact]
    public void Color_DefaultColor_ReturnsOriginalText()
    {
        var result = MarkdownInlineUtils.Color("テキスト", Notion.Client.Color.Default);
        Assert.Equal("テキスト", result);
    }

    [Fact]
    public void Color_TextColor_ReturnsSpanWithColor()
    {
        var result = MarkdownInlineUtils.Color("テキスト", Notion.Client.Color.Red);
        Assert.Contains("<span", result);
        Assert.Contains("color:", result);
        Assert.Contains("テキスト", result);
        Assert.Contains("</span>", result);
    }

    [Fact]
    public void Color_BackgroundColor_ReturnsSpanWithBackgroundColor()
    {
        var result = MarkdownInlineUtils.Color("テキスト", Notion.Client.Color.RedBackground);
        Assert.Contains("<span", result);
        Assert.Contains("background-color:", result);
        Assert.Contains("テキスト", result);
        Assert.Contains("</span>", result);
    }

    [Fact]
    public void Color_WhitespaceOnly_ReturnsOriginal()
    {
        var result = MarkdownInlineUtils.Color("   ", Notion.Client.Color.Red);
        Assert.Equal("   ", result);
    }

    [Fact]
    public void Color_NullColor_ReturnsOriginalText()
    {
        var result = MarkdownInlineUtils.Color("テキスト", null);
        Assert.Equal("テキスト", result);
    }
}
