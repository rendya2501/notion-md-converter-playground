using NotionMarkdownConverter.Shared.Enums;
using NotionMarkdownConverter.Shared.Models;
using NotionMarkdownConverter.Transform.Converters;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// FrontmatterConverterのユニットテスト
/// </summary>
public class FrontmatterConverterTests
{
    private readonly FrontmatterConverter _converter = new();

    [Fact]
    public void Convert_WithAllProperties_ProducesValidFrontmatter()
    {
        var pageProperty = new PageProperty
        {
            Title = "テストタイトル",
            Description = "テスト説明",
            Tags = ["タグ1", "タグ2"],
            Type = "article",
            PublishedDateTime = new DateTime(2024, 1, 15, 12, 0, 0),
            PublicStatus = PublicStatus.Queued
        };

        var result = _converter.Convert(pageProperty);

        Assert.Contains("---", result);
        Assert.Contains("title: \"テストタイトル\"", result);
        Assert.Contains("description: \"テスト説明\"", result);
        Assert.Contains("tags:", result);
        Assert.Contains("\"タグ1\"", result);
        Assert.Contains("\"タグ2\"", result);
        Assert.Contains("type: \"article\"", result);
        Assert.Contains("date:", result);
    }

    [Fact]
    public void Convert_WithoutOptionalProperties_OmitsThem()
    {
        var pageProperty = new PageProperty
        {
            Title = "タイトルのみ"
        };

        var result = _converter.Convert(pageProperty);

        Assert.Contains("title: \"タイトルのみ\"", result);
        Assert.DoesNotContain("description:", result);
        Assert.DoesNotContain("tags:", result);
        Assert.DoesNotContain("type:", result);
    }

    [Fact]
    public void Convert_StartsAndEndsWithTripleDash()
    {
        var pageProperty = new PageProperty { Title = "テスト" };
        var result = _converter.Convert(pageProperty);
        var lines = result.Split('\n').Where(l => l.TrimEnd() == "---").ToList();

        // フロントマターの開始・終了の---が2つ存在する
        Assert.Equal(2, lines.Count);
    }

    [Fact]
    public void Convert_PublishedDateTime_UsesIso8601Format()
    {
        var pageProperty = new PageProperty
        {
            Title = "テスト",
            PublishedDateTime = new DateTime(2024, 6, 1, 9, 30, 0)
        };

        var result = _converter.Convert(pageProperty);

        // sortable format (s) = yyyy-MM-ddTHH:mm:ss
        Assert.Contains("date: \"2024-06-01T09:30:00\"", result);
    }

    [Fact]
    public void Convert_TypeAppearsBeforeTitle()
    {
        var pageProperty = new PageProperty
        {
            Title = "タイトル",
            Type = "article"
        };

        var result = _converter.Convert(pageProperty);
        var typeIndex = result.IndexOf("type:", StringComparison.Ordinal);
        var titleIndex = result.IndexOf("title:", StringComparison.Ordinal);

        Assert.True(typeIndex < titleIndex, "typeはtitleより前に出力されるべき");
    }

    [Fact]
    public void Convert_EmptyTags_OmitsTagsField()
    {
        var pageProperty = new PageProperty
        {
            Title = "テスト",
            Tags = []
        };

        var result = _converter.Convert(pageProperty);

        Assert.DoesNotContain("tags:", result);
    }

    [Fact]
    public void Convert_WithTags_FormatsAsJsonArray()
    {
        var pageProperty = new PageProperty
        {
            Title = "テスト",
            Tags = ["C#", ".NET"]
        };

        var result = _converter.Convert(pageProperty);

        Assert.Contains("tags: [\"C#\",\".NET\"]", result);
    }
}
