using Microsoft.Extensions.DependencyInjection;
using Notion.Client;
using NotionMarkdownConverter.Application;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Domain.Enums;
using NotionMarkdownConverter.Domain.Models;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// MarkdownAssemblerのユニットテスト。
/// Notionクライアントとリンクプロセッサはスタブで差し替え、
/// フロントマター生成・ブロック変換・組み立てロジックを実際の実装で検証します。
/// </summary>
/// <remarks>
/// ETLパイプライン移行前後のリグレッションベースラインです。
/// 移行後もこのテストがパスし続けることが、振る舞い保持の証明になります。
/// </remarks>
public class MarkdownAssemblerTests : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), $"assembler-test-{Guid.NewGuid().ToString("N")[..8]}");

    public MarkdownAssemblerTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // ── スタブ ────────────────────────────────────────────────────────

    /// <summary>
    /// 指定されたブロックリストを返すスタブ。
    /// </summary>
    private sealed class StubNotionClient(List<NotionBlock> blocks) : INotionClientWrapper
    {
        public Task<List<Page>> GetPagesForPublishingAsync(string databaseId)
            => Task.FromResult(new List<Page>());

        public Task UpdatePagePropertiesAsync(string pageId, DateTime now)
            => Task.CompletedTask;

        public Task<List<NotionBlock>> FetchBlockTreeAsync(string blockId)
            => Task.FromResult(blocks);
    }

    /// <summary>
    /// Markdownを変更せずそのまま返しつつ、受け取った引数を記録するスタブ。
    /// </summary>
    private sealed class CapturingLinkProcessor : IMarkdownLinkProcessor
    {
        public string? LastMarkdown { get; private set; }
        public string? LastOutputDirectory { get; private set; }

        public Task<string> ProcessLinksAsync(string markdown, string outputDirectory)
        {
            LastMarkdown = markdown;
            LastOutputDirectory = outputDirectory;
            return Task.FromResult(markdown);
        }
    }

    // ── ビルダー ──────────────────────────────────────────────────────

    /// <summary>
    /// アプリケーション層のDIを使ってMarkdownAssemblerを構築します。
    /// 外部依存（INotionClientWrapper・IMarkdownLinkProcessor）はスタブで上書きします。
    /// </summary>
    /// <remarks>
    /// AddApplicationServicesが登録するMarkdownLinkProcessor（IFileDownloaderに依存）は、
    /// 後続のAddSingletonで上書きされるため、IFileDownloader未登録でも解決できます。
    /// .NET DIはサービスを遅延評価するため、上書きされたサービスは生成されません。
    /// </remarks>
    private (IMarkdownAssembler assembler, CapturingLinkProcessor linkProcessor) CreateSut(
        List<NotionBlock>? blocks = null)
    {
        var linkProcessor = new CapturingLinkProcessor();
        var services = new ServiceCollection();

        // アプリケーション層のサービスを登録
        // （IMarkdownLinkProcessorとしてMarkdownLinkProcessorも登録されるが、後で上書きする）
        services.AddApplicationServices(new NotionExportOptions());

        // 外部依存をスタブで上書き（後勝ちルールにより、後に登録した実装が使われる）
        services.AddSingleton<INotionClientWrapper>(new StubNotionClient(blocks ?? []));
        services.AddSingleton<IMarkdownLinkProcessor>(linkProcessor);

        using var provider = services.BuildServiceProvider();
        return (provider.GetRequiredService<IMarkdownAssembler>(), linkProcessor);
    }

    // ── テストデータ ──────────────────────────────────────────────────

    private static PageProperty MakePageProperty(
        string title = "テストタイトル",
        DateTime? publishedAt = null) => new()
        {
            PageId = "test-page-id",
            Title = title,
            PublicStatus = PublicStatus.Queued,
            PublishedDateTime = publishedAt ?? new DateTime(2024, 1, 15, 0, 0, 0)
        };

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
    public async Task AssembleAsync_Always_ContainsFrontmatterDelimiters()
    {
        // フロントマターは必ず --- で囲まれている
        var (sut, _) = CreateSut();
        var result = await sut.AssembleAsync(MakePageProperty(), _tempDir);

        var dashes = result.Split('\n').Count(l => l.TrimEnd() == "---");
        Assert.Equal(2, dashes);
    }

    [Fact]
    public async Task AssembleAsync_WithTitle_FrontmatterContainsTitle()
    {
        var (sut, _) = CreateSut();
        var result = await sut.AssembleAsync(MakePageProperty("マイタイトル"), _tempDir);

        Assert.Contains("title: \"マイタイトル\"", result);
    }

    [Fact]
    public async Task AssembleAsync_WithPublishedDate_FrontmatterContainsIso8601Date()
    {
        var (sut, _) = CreateSut();
        var pageProperty = MakePageProperty(publishedAt: new DateTime(2024, 6, 15, 9, 30, 0));

        var result = await sut.AssembleAsync(pageProperty, _tempDir);

        Assert.Contains("date: \"2024-06-15T09:30:00\"", result);
    }

    [Fact]
    public async Task AssembleAsync_FrontmatterAppearsBeforeBody()
    {
        // フロントマターの終端 --- が本文テキストより前に現れる
        var (sut, _) = CreateSut([MakeParagraph("本文テキスト")]);
        var result = await sut.AssembleAsync(MakePageProperty(), _tempDir);

        var lastDash = result.LastIndexOf("---", StringComparison.Ordinal);
        var bodyStart = result.IndexOf("本文テキスト", StringComparison.Ordinal);
        Assert.True(lastDash < bodyStart);
    }

    // ── ブロック変換 ──────────────────────────────────────────────────

    [Fact]
    public async Task AssembleAsync_EmptyBlocks_ReturnsFrontmatterOnly()
    {
        var (sut, _) = CreateSut([]);
        var result = await sut.AssembleAsync(MakePageProperty(), _tempDir);

        Assert.Contains("---", result);
        // 本文が空なので見出しも段落テキストも存在しない
        Assert.DoesNotContain("# ", result);
    }

    [Fact]
    public async Task AssembleAsync_WithParagraph_BodyContainsParagraphText()
    {
        var (sut, _) = CreateSut([MakeParagraph("段落テキスト")]);
        var result = await sut.AssembleAsync(MakePageProperty(), _tempDir);

        Assert.Contains("段落テキスト", result);
    }

    [Fact]
    public async Task AssembleAsync_WithHeadingOne_BodyContainsMarkdownH1()
    {
        var (sut, _) = CreateSut([MakeHeadingOne("見出し1テキスト")]);
        var result = await sut.AssembleAsync(MakePageProperty(), _tempDir);

        Assert.Contains("# 見出し1テキスト", result);
    }

    [Fact]
    public async Task AssembleAsync_WithMultipleBlocks_AllBlocksAppearInOrder()
    {
        var (sut, _) = CreateSut(
        [
            MakeHeadingOne("セクション1"),
            MakeParagraph("段落A"),
            MakeParagraph("段落B"),
        ]);
        var result = await sut.AssembleAsync(MakePageProperty(), _tempDir);

        // ブロックの出現順序が入力と一致する
        var h1Index = result.IndexOf("# セクション1", StringComparison.Ordinal);
        var paraAIndex = result.IndexOf("段落A", StringComparison.Ordinal);
        var paraBIndex = result.IndexOf("段落B", StringComparison.Ordinal);

        Assert.True(h1Index < paraAIndex, "見出しが段落Aより前");
        Assert.True(paraAIndex < paraBIndex, "段落Aが段落Bより前");
    }

    // ── リンク処理連携 ────────────────────────────────────────────────

    [Fact]
    public async Task AssembleAsync_Always_PassesOutputDirectoryToLinkProcessor()
    {
        // 出力ディレクトリがリンクプロセッサに正しく渡される
        var (sut, linkProcessor) = CreateSut([MakeParagraph("テキスト")]);
        await sut.AssembleAsync(MakePageProperty(), _tempDir);

        Assert.Equal(_tempDir, linkProcessor.LastOutputDirectory);
    }

    [Fact]
    public async Task AssembleAsync_Always_PassesConvertedContentToLinkProcessor()
    {
        // リンクプロセッサに渡されるMarkdownが変換済み本文を含む
        var (sut, linkProcessor) = CreateSut([MakeParagraph("テキスト")]);
        await sut.AssembleAsync(MakePageProperty(), _tempDir);

        Assert.NotNull(linkProcessor.LastMarkdown);
        Assert.Contains("テキスト", linkProcessor.LastMarkdown);
    }

    // ── ゴールデンファイルテスト（リグレッションベースライン） ────────

    /// <summary>
    /// 既知の入力から生成されるMarkdownの完全な構造を検証します。
    /// ETLパイプライン移行後も同一の出力が得られることを確認するベースラインです。
    /// 移行後にこのテストが失敗した場合、変換ロジックに意図しない差異が生じています。
    /// </summary>
    [Fact]
    public async Task AssembleAsync_KnownInput_ProducesExpectedMarkdownStructure()
    {
        // Arrange
        var pageProperty = new PageProperty
        {
            PageId = "golden-test-id",
            Title = "ゴールデンテスト記事",
            Type = "article",
            Tags = ["C#", "テスト"],
            Description = "リグレッションテスト用の記事",
            PublishedDateTime = new DateTime(2024, 3, 15, 12, 0, 0),
            PublicStatus = PublicStatus.Queued
        };

        var blocks = new List<NotionBlock>
        {
            MakeHeadingOne("はじめに"),
            MakeParagraph("これはテスト段落です。"),
        };

        var (sut, _) = CreateSut(blocks);

        // Act
        var result = await sut.AssembleAsync(pageProperty, _tempDir);

        // Assert: フロントマター
        Assert.Contains("type: \"article\"", result);
        Assert.Contains("title: \"ゴールデンテスト記事\"", result);
        Assert.Contains("description: \"リグレッションテスト用の記事\"", result);
        Assert.Contains("tags: [\"C#\",\"テスト\"]", result);
        Assert.Contains("date: \"2024-03-15T12:00:00\"", result);

        // Assert: 本文
        Assert.Contains("# はじめに", result);
        Assert.Contains("これはテスト段落です。", result);

        // Assert: フロントマターが本文より前
        var frontmatterEnd = result.LastIndexOf("---", StringComparison.Ordinal);
        var bodyStart = result.IndexOf("# はじめに", StringComparison.Ordinal);
        Assert.True(frontmatterEnd < bodyStart);
    }
}
