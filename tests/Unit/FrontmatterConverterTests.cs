// tests/Unit/FrontmatterConverterTests.cs
using NotionMarkdownConverter.Domain.Enums;
using NotionMarkdownConverter.Domain.Markdown.Converters;
using NotionMarkdownConverter.Domain.Models;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// FrontmatterConverterのユニットテスト。
/// 外部依存なし、純粋な変換ロジックのみ検証。
/// </summary>
public class FrontmatterConverterTests
{
    private readonly FrontmatterConverter _converter = new();

    [Fact]
    public void Convert_WithAllProperties_ProducesValidFrontmatter()
    {
        // Arrange
        var pageProperty = new PageProperty
        {
            Title = "テストタイトル",
            Description = "テスト説明",
            Tags = ["タグ1", "タグ2"],
            Type = "article",
            PublishedDateTime = new DateTime(2024, 1, 15, 12, 0, 0),
            PublicStatus = PublicStatus.Queued
        };

        // Act
        var result = _converter.Convert(pageProperty);

        // Assert
        Assert.Contains("---", result);
        Assert.Contains("title: \"テストタイトル\"", result);
        Assert.Contains("description: \"テスト説明\"", result);
        Assert.Contains("tags:", result);
        Assert.Contains("\"タグ1\"", result);
        Assert.Contains("type: \"article\"", result);
        Assert.Contains("date:", result);
    }

    [Fact]
    public void Convert_WithoutOptionalProperties_OmitsThem()
    {
        // Arrange（説明・タグ・タイプなし）
        var pageProperty = new PageProperty
        {
            Title = "タイトルのみ"
        };

        // Act
        var result = _converter.Convert(pageProperty);

        // Assert
        Assert.Contains("title: \"タイトルのみ\"", result);
        Assert.DoesNotContain("description:", result);
        Assert.DoesNotContain("tags:", result);
        Assert.DoesNotContain("type:", result);
    }
}
