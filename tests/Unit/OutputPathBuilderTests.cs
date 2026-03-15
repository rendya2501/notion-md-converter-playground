using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Shared.Models;
using NotionMarkdownConverter.Transform;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// OutputPathBuilder のユニットテスト。
/// </summary>
public class OutputPathBuilderTests
{
    private OutputPathBuilder MakeSut(string template) =>
        new(Options.Create(new NotionExportOptions
        {
            OutputDirectoryPathTemplate = template
        }));

    // ── 例外 ──────────────────────────────────────────────────────────

    [Fact]
    public void Build_NullPublishedDateTime_ThrowsInvalidOperationException()
    {
        var sut = MakeSut("{{slug}}");
        var page = new PageProperty { Slug = "test", PublishedDateTime = null };

        Assert.Throws<InvalidOperationException>(() => sut.Build(page));
    }

    [Fact]
    public void Build_NullPublishedDateTime_ExceptionMessageContainsPageId()
    {
        var sut = MakeSut("{{slug}}");
        var page = new PageProperty { PageId = "page-99", Slug = "test", PublishedDateTime = null };

        var ex = Assert.Throws<InvalidOperationException>(() => sut.Build(page));
        Assert.Contains("page-99", ex.Message);
    }

    // ── スラグ ────────────────────────────────────────────────────────

    [Fact]
    public void Build_SlugTemplate_PathEndsWithSlug()
    {
        var sut = MakeSut("output/{{slug}}");
        var page = new PageProperty { Slug = "my-post", PublishedDateTime = DateTime.Now };

        var result = sut.Build(page);

        Assert.EndsWith("my-post", result.TrimEnd(Path.DirectorySeparatorChar, '/'));
    }

    [Fact]
    public void Build_EmptySlug_FallsBackToTitle()
    {
        var sut = MakeSut("output/{{slug}}");
        var page = new PageProperty
        {
            Slug = string.Empty,
            Title = "タイトルフォールバック",
            PublishedDateTime = DateTime.Now
        };

        var result = sut.Build(page);

        Assert.Contains("タイトルフォールバック", result);
    }

    // ── 日付テンプレート ──────────────────────────────────────────────

    [Fact]
    public void Build_DateTemplate_ExpandsYear()
    {
        var sut = MakeSut("output/{{ publish | date.to_string '%Y' }}/{{slug}}");
        var page = new PageProperty
        {
            Slug = "post",
            PublishedDateTime = new DateTime(2024, 6, 15)
        };

        var result = sut.Build(page);

        Assert.Contains("2024", result);
    }

    [Fact]
    public void Build_DateTemplate_ExpandsMonth()
    {
        var sut = MakeSut("output/{{ publish | date.to_string '%Y/%m' }}/{{slug}}");
        var page = new PageProperty
        {
            Slug = "post",
            PublishedDateTime = new DateTime(2024, 3, 1)
        };

        var result = sut.Build(page);

        Assert.Matches(@".*2024[/\\]03.*", result);
    }

    // ── タイトルテンプレート ──────────────────────────────────────────

    [Fact]
    public void Build_TitleTemplate_ExpandsTitle()
    {
        var sut = MakeSut("output/{{title}}");
        var page = new PageProperty
        {
            Title = "My Article",
            PublishedDateTime = DateTime.Now
        };

        var result = sut.Build(page);

        Assert.Contains("My Article", result);
    }

    // ── 一貫性 ────────────────────────────────────────────────────────

    [Fact]
    public void Build_SameInput_ReturnsSamePath()
    {
        var sut = MakeSut("output/{{ publish | date.to_string '%Y/%m' }}/{{slug}}");
        var page = new PageProperty
        {
            Slug = "my-post",
            PublishedDateTime = new DateTime(2024, 3, 15)
        };

        var result1 = sut.Build(page);
        var result2 = sut.Build(page);

        Assert.Equal(result1, result2);
    }
}
