using Notion.Client;
using NotionMarkdownConverter.Domain.Enums;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// PropertyParserのユニットテスト
/// </summary>
public class PropertyParserTests
{
    // ── TryParseAsDateTime ────────────────────────────────────────────

    [Fact]
    public void TryParseAsDateTime_DatePropertyWithValue_ReturnsTrue()
    {
        var value = new DatePropertyValue
        {
            Date = new Date { Start = new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.Zero) }
        };

        var result = PropertyParser.TryParseAsDateTime(value, out var dateTime);

        Assert.True(result);
        Assert.Equal(new DateTime(2024, 1, 15, 12, 0, 0), dateTime);
    }

    [Fact]
    public void TryParseAsDateTime_DatePropertyWithNullDate_ReturnsFalse()
    {
        var value = new DatePropertyValue { Date = null };

        var result = PropertyParser.TryParseAsDateTime(value, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseAsDateTime_CreatedTimeProperty_ReturnsTrue()
    {
        var value = new CreatedTimePropertyValue { CreatedTime = "2024-01-15T12:00:00.000Z" };

        var result = PropertyParser.TryParseAsDateTime(value, out var dateTime);

        Assert.True(result);
        Assert.Equal(2024, dateTime.Year);
        Assert.Equal(1, dateTime.Month);
        Assert.Equal(15, dateTime.Day);
    }

    // ── TryParseAsPlainText ───────────────────────────────────────────

    [Fact]
    public void TryParseAsPlainText_RichTextProperty_ReturnsJoinedText()
    {
        var value = new RichTextPropertyValue
        {
            RichText = [new RichTextText { PlainText = "テキスト", Text = new Text { Content = "テキスト" } }]
        };

        var result = PropertyParser.TryParseAsPlainText(value, out var text);

        Assert.True(result);
        Assert.Equal("テキスト", text);
    }

    [Fact]
    public void TryParseAsPlainText_TitleProperty_ReturnsTitle()
    {
        var value = new TitlePropertyValue
        {
            Title = [new RichTextText { PlainText = "タイトル", Text = new Text { Content = "タイトル" } }]
        };

        var result = PropertyParser.TryParseAsPlainText(value, out var text);

        Assert.True(result);
        Assert.Equal("タイトル", text);
    }

    [Fact]
    public void TryParseAsPlainText_SelectProperty_ReturnsSelectName()
    {
        var value = new SelectPropertyValue
        {
            Select = new SelectOption { Name = "選択肢A" }
        };

        var result = PropertyParser.TryParseAsPlainText(value, out var text);

        Assert.True(result);
        Assert.Equal("選択肢A", text);
    }

    [Fact]
    public void TryParseAsPlainText_SelectPropertyWithNullSelect_ReturnsEmpty()
    {
        var value = new SelectPropertyValue { Select = null };

        var result = PropertyParser.TryParseAsPlainText(value, out var text);

        Assert.True(result);
        Assert.Equal(string.Empty, text);
    }

    [Fact]
    public void TryParseAsPlainText_UnsupportedType_ReturnsFalse()
    {
        var value = new CheckboxPropertyValue { Checkbox = true };

        var result = PropertyParser.TryParseAsPlainText(value, out var text);

        Assert.False(result);
        Assert.Equal(string.Empty, text);
    }

    // ── TryParseAsStringList ──────────────────────────────────────────

    [Fact]
    public void TryParseAsStringList_MultiSelectProperty_ReturnsAllItems()
    {
        var value = new MultiSelectPropertyValue
        {
            MultiSelect =
            [
                new SelectOption { Name = "タグ1" },
                new SelectOption { Name = "タグ2" },
                new SelectOption { Name = "タグ3" }
            ]
        };

        var result = PropertyParser.TryParseAsStringList(value, out var items);

        Assert.True(result);
        Assert.Equal(3, items.Count);
        Assert.Contains("タグ1", items);
        Assert.Contains("タグ2", items);
        Assert.Contains("タグ3", items);
    }

    [Fact]
    public void TryParseAsStringList_NonMultiSelectProperty_ReturnsFalse()
    {
        var value = new RichTextPropertyValue { RichText = [] };

        var result = PropertyParser.TryParseAsStringList(value, out var items);

        Assert.False(result);
        Assert.Empty(items);
    }

    // ── TryParseAsBoolean ─────────────────────────────────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TryParseAsBoolean_CheckboxProperty_ReturnsBooleanValue(bool expected)
    {
        var value = new CheckboxPropertyValue { Checkbox = expected };

        var result = PropertyParser.TryParseAsBoolean(value, out var boolResult);

        Assert.True(result);
        Assert.Equal(expected, boolResult);
    }

    [Fact]
    public void TryParseAsBoolean_NonCheckboxProperty_ReturnsFalse()
    {
        var value = new RichTextPropertyValue { RichText = [] };

        var result = PropertyParser.TryParseAsBoolean(value, out _);

        Assert.False(result);
    }

    // ── TryParseAsEnum ────────────────────────────────────────────────

    [Theory]
    [InlineData("Published", PublicStatus.Published)]
    [InlineData("Queued", PublicStatus.Queued)]
    [InlineData("Unpublished", PublicStatus.Unpublished)]
    public void TryParseAsEnum_ValidSelectValue_ReturnsParsedEnum(string name, PublicStatus expected)
    {
        var value = new SelectPropertyValue
        {
            Select = new SelectOption { Name = name }
        };

        var result = PropertyParser.TryParseAsEnum<PublicStatus>(value, out var enumResult);

        Assert.True(result);
        Assert.Equal(expected, enumResult);
    }

    [Fact]
    public void TryParseAsEnum_CaseInsensitive_ReturnsTrue()
    {
        var value = new SelectPropertyValue
        {
            Select = new SelectOption { Name = "published" }
        };

        var result = PropertyParser.TryParseAsEnum<PublicStatus>(value, out var enumResult);

        Assert.True(result);
        Assert.Equal(PublicStatus.Published, enumResult);
    }

    [Fact]
    public void TryParseAsEnum_InvalidValue_ReturnsFalse()
    {
        var value = new SelectPropertyValue
        {
            Select = new SelectOption { Name = "存在しないステータス" }
        };

        var result = PropertyParser.TryParseAsEnum<PublicStatus>(value, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseAsEnum_NullSelect_ReturnsFalse()
    {
        var value = new SelectPropertyValue { Select = null };

        var result = PropertyParser.TryParseAsEnum<PublicStatus>(value, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseAsEnum_NonSelectProperty_ReturnsFalse()
    {
        var value = new CheckboxPropertyValue { Checkbox = true };

        var result = PropertyParser.TryParseAsEnum<PublicStatus>(value, out _);

        Assert.False(result);
    }
}
