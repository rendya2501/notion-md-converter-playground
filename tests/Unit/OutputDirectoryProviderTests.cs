using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Infrastructure.FileSystem;
using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// OutputDirectoryProviderのユニットテスト。
/// テスト実行ごとにユニークなtempディレクトリを使用し、テスト後に削除します。
/// </summary>
public class OutputDirectoryProviderTests : IDisposable
{
    private readonly string _tempBase =
        Path.Combine(Path.GetTempPath(), $"notion-test-{Guid.NewGuid().ToString("N")[..8]}");

    private OutputDirectoryProvider MakeSut(string template) =>
        new(Options.Create(new NotionExportOptions
        {
            OutputDirectoryPathTemplate = template
        }));

    private string Template(string suffix) =>
        Path.Combine(_tempBase, suffix);

    // ── 例外 ──────────────────────────────────────────────────────────

    [Fact]
    public void BuildAndCreate_NullPublishedDateTime_ThrowsInvalidOperationException()
    {
        var sut = MakeSut(Template("{{slug}}"));
        var page = new PageProperty { Slug = "test", PublishedDateTime = null };

        Assert.Throws<InvalidOperationException>(() => sut.BuildAndCreate(page));
    }

    [Fact]
    public void BuildAndCreate_NullPublishedDateTime_ExceptionMessageContainsPageId()
    {
        var sut = MakeSut(Template("{{slug}}"));
        var page = new PageProperty { PageId = "page-99", Slug = "test", PublishedDateTime = null };

        var ex = Assert.Throws<InvalidOperationException>(() => sut.BuildAndCreate(page));
        Assert.Contains("page-99", ex.Message);
    }

    // ── パス構築 ──────────────────────────────────────────────────────

    [Fact]
    public void BuildAndCreate_SlugTemplate_PathEndsWithSlug()
    {
        var sut = MakeSut(Template("{{slug}}"));
        var page = new PageProperty { Slug = "my-post", PublishedDateTime = DateTime.Now };

        var result = sut.BuildAndCreate(page);

        Assert.EndsWith("my-post", result.TrimEnd(Path.DirectorySeparatorChar, '/'));
    }

    [Fact]
    public void BuildAndCreate_EmptySlug_FallsBackToTitle()
    {
        // スラグが空の場合はタイトルをフォールバックとして使用する
        var sut = MakeSut(Template("{{slug}}"));
        var page = new PageProperty
        {
            Slug = string.Empty,
            Title = "タイトルフォールバック",
            PublishedDateTime = DateTime.Now
        };

        var result = sut.BuildAndCreate(page);

        Assert.Contains("タイトルフォールバック", result);
    }

    [Fact]
    public void BuildAndCreate_DateTemplate_ExpandsYear()
    {
        var sut = MakeSut(Template("{{ publish | date.to_string '%Y' }}/{{slug}}"));
        var page = new PageProperty
        {
            Slug = "post",
            PublishedDateTime = new DateTime(2024, 6, 15)
        };

        var result = sut.BuildAndCreate(page);

        Assert.Contains("2024", result);
    }

    [Fact]
    public void BuildAndCreate_DateTemplate_ExpandsMonth()
    {
        var sut = MakeSut(Template("{{ publish | date.to_string '%Y/%m' }}/{{slug}}"));
        var page = new PageProperty
        {
            Slug = "post",
            PublishedDateTime = new DateTime(2024, 3, 1)
        };

        var result = sut.BuildAndCreate(page);

        // 月は2桁でゼロ埋めされる（03）
        Assert.Matches(@".*2024[/\\]03.*", result);
    }

    [Fact]
    public void BuildAndCreate_TitleTemplate_ExpandsTitle()
    {
        var sut = MakeSut(Template("{{title}}"));
        var page = new PageProperty
        {
            Title = "My Article",
            PublishedDateTime = DateTime.Now
        };

        var result = sut.BuildAndCreate(page);

        Assert.Contains("My Article", result);
    }

    [Fact]
    public void BuildAndCreate_ReturnsAbsolutePath()
    {
        var sut = MakeSut(Template("{{slug}}"));
        var page = new PageProperty { Slug = "test", PublishedDateTime = DateTime.Now };

        var result = sut.BuildAndCreate(page);

        Assert.True(Path.IsPathRooted(result), $"Expected absolute path, got: {result}");
    }

    // ── ディレクトリ作成 ──────────────────────────────────────────────

    [Fact]
    public void BuildAndCreate_CreatesDirectory()
    {
        var sut = MakeSut(Template("{{slug}}"));
        var page = new PageProperty { Slug = "new-post", PublishedDateTime = DateTime.Now };

        var result = sut.BuildAndCreate(page);

        Assert.True(Directory.Exists(result));
    }

    [Fact]
    public void BuildAndCreate_ExistingDirectory_DoesNotThrow()
    {
        // Directory.CreateDirectory は冪等なので、既存ディレクトリでも例外を投げない
        var sut = MakeSut(Template("{{slug}}"));
        var page = new PageProperty { Slug = "existing", PublishedDateTime = DateTime.Now };

        sut.BuildAndCreate(page); // 1回目で作成
        var ex = Record.Exception(() => sut.BuildAndCreate(page)); // 2回目

        Assert.Null(ex);
    }

    [Fact]
    public void BuildAndCreate_NestedTemplate_CreatesIntermediateDirectories()
    {
        var sut = MakeSut(Template("{{ publish | date.to_string '%Y' }}/{{slug}}"));
        var page = new PageProperty
        {
            Slug = "deep-post",
            PublishedDateTime = new DateTime(2025, 1, 1)
        };

        var result = sut.BuildAndCreate(page);

        Assert.True(Directory.Exists(result));
    }

    // ── IDisposable ───────────────────────────────────────────────────

    public void Dispose()
    {
        if (Directory.Exists(_tempBase))
            Directory.Delete(_tempBase, recursive: true);
    }
}
