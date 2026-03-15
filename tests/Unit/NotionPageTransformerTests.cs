using Microsoft.Extensions.DependencyInjection;
using Notion.Client;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Infrastructure.FileSystem;
using NotionMarkdownConverter.Infrastructure.Http;
using NotionMarkdownConverter.Pipeline.Models;
using NotionMarkdownConverter.Shared.Enums;
using NotionMarkdownConverter.Shared.Models;
using NotionMarkdownConverter.Transform;
using NotionMarkdownConverter.Transform.Converters;
using System.Text;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// NotionPageTransformerのユニットテスト。
/// MarkdownAssemblerTestsのゴールデンファイルテストと同一の入力・期待値を使い、
/// ETL移行後も振る舞いが変わっていないことを保証します。
/// </summary>
public class NotionPageTransformerTests : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), $"transformer-test-{Guid.NewGuid().ToString("N")[..8]}");

    public NotionPageTransformerTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    // ── Fake ──────────────────────────────────────────────────────────

    private sealed class FakeFileDownloader : IFileDownloader
    {
        public Task DownloadAsync(UrlFilePair urlFilePair, string outputDirectory)
            => Task.CompletedTask;
    }

    private sealed class FakeFileSystem : IFileSystem
    {
        public void CreateDirectory(string path) { }
        public Task WriteAllTextAsync(string path, string content, Encoding encoding)
            => Task.CompletedTask;
    }

    // ── ビルダー ──────────────────────────────────────────────────────

    private NotionPageTransformer CreateSut()
    {
        var services = new ServiceCollection();
        services.AddApplicationServices(new NotionExportOptions());
        services.AddSingleton<IFileDownloader>(new FakeFileDownloader());
        services.AddSingleton<IFileSystem>(new FakeFileSystem());

        var provider = services.BuildServiceProvider();
        return new NotionPageTransformer(
            provider.GetRequiredService<FrontmatterConverter>(),
            provider.GetRequiredService<ContentConverter>(),
            provider.GetRequiredService<MarkdownLinkReplacer>(),
            new FakeFileDownloader(),
            provider.GetRequiredService<OutputPathBuilder>(),
            new FakeFileSystem());
    }

    // ── テストデータ ──────────────────────────────────────────────────

    private static ExtractedPage MakeExtractedPage(
        string title = "テストタイトル",
        DateTime? publishedAt = null,
        List<NotionBlock>? blocks = null) =>
        new(
            new PageProperty
            {
                PageId = "test-page-id",
                Title = title,
                PublicStatus = PublicStatus.Queued,
                PublishedDateTime = publishedAt ?? new DateTime(2024, 1, 15, 0, 0, 0)
            },
            blocks ?? []);

    private static NotionBlock MakeParagraph(string text)
    {
        var block = new ParagraphBlock
        {
            Paragraph = new ParagraphBlock.Info
            {
                RichText =
                [
                    new RichTextText
                    {
                        PlainText = text,
                        Text = new Text { Content = text },
                        Annotations = new Annotations()
                    }
                ]
            }
        };
        return new NotionBlock(block);
    }

    private static NotionBlock MakeHeadingOne(string text)
    {
        var block = new HeadingOneBlock
        {
            Heading_1 = new HeadingOneBlock.Info
            {
                RichText =
                [
                    new RichTextText
                    {
                        PlainText = text,
                        Text = new Text { Content = text },
                        Annotations = new Annotations()
                    }
                ]
            }
        };
        return new NotionBlock(block);
    }

    // ── フロントマター ────────────────────────────────────────────────

    [Fact]
    public async Task TransformAsync_Always_ContainsFrontmatterDelimiters()
    {
        var sut = CreateSut();
        var result = await sut.TransformAsync(MakeExtractedPage());

        var dashes = result.Markdown.Split('\n').Count(l => l.TrimEnd() == "---");
        Assert.Equal(2, dashes);
    }

    [Fact]
    public async Task TransformAsync_WithTitle_FrontmatterContainsTitle()
    {
        var sut = CreateSut();
        var result = await sut.TransformAsync(MakeExtractedPage("マイタイトル"));

        Assert.Contains("title: \"マイタイトル\"", result.Markdown);
    }

    // ── ブロック変換 ──────────────────────────────────────────────────

    [Fact]
    public async Task TransformAsync_WithParagraph_BodyContainsParagraphText()
    {
        var sut = CreateSut();
        var result = await sut.TransformAsync(MakeExtractedPage(blocks: [MakeParagraph("段落テキスト")]));

        Assert.Contains("段落テキスト", result.Markdown);
    }

    [Fact]
    public async Task TransformAsync_WithHeadingOne_BodyContainsMarkdownH1()
    {
        var sut = CreateSut();
        var result = await sut.TransformAsync(MakeExtractedPage(blocks: [MakeHeadingOne("見出し1")]));

        Assert.Contains("# 見出し1", result.Markdown);
    }

    // ── ゴールデンファイルテスト（MarkdownAssemblerTestsと同一入力） ──

    [Fact]
    public async Task TransformAsync_KnownInput_ProducesExpectedMarkdownStructure()
    {
        var extractedPage = new ExtractedPage(
            new PageProperty
            {
                PageId = "golden-test-id",
                Title = "ゴールデンテスト記事",
                Type = "article",
                Tags = ["C#", "テスト"],
                Description = "リグレッションテスト用の記事",
                PublishedDateTime = new DateTime(2024, 3, 15, 12, 0, 0),
                PublicStatus = PublicStatus.Queued
            },
            [MakeHeadingOne("はじめに"), MakeParagraph("これはテスト段落です。")]
        );

        var sut = CreateSut();
        var result = await sut.TransformAsync(extractedPage);

        Assert.Contains("type: \"article\"", result.Markdown);
        Assert.Contains("title: \"ゴールデンテスト記事\"", result.Markdown);
        Assert.Contains("description: \"リグレッションテスト用の記事\"", result.Markdown);
        Assert.Contains("tags: [\"C#\",\"テスト\"]", result.Markdown);
        Assert.Contains("date: \"2024-03-15T12:00:00\"", result.Markdown);
        Assert.Contains("# はじめに", result.Markdown);
        Assert.Contains("これはテスト段落です。", result.Markdown);

        var frontmatterEnd = result.Markdown.LastIndexOf("---", StringComparison.Ordinal);
        var bodyStart = result.Markdown.IndexOf("# はじめに", StringComparison.Ordinal);
        Assert.True(frontmatterEnd < bodyStart);
    }
}
