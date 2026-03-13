using Microsoft.Extensions.Logging.Abstractions;
using NotionMarkdownConverter.Infrastructure;
using NotionMarkdownConverter.Load;
using NotionMarkdownConverter.Pipeline.Models;
using NotionMarkdownConverter.Shared.Enums;
using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Tests.Unit;

public class NotionPageLoaderTests : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), $"loader-test-{Guid.NewGuid().ToString("N")[..8]}");

    public NotionPageLoaderTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }

    // ── Fake ──────────────────────────────────────────────────────────

    private class FakeNotionClient : INotionClientWrapper
    {
        public List<string> UpdatedPageIds { get; } = [];
        public Exception? UpdateException { get; set; }

        public Task<List<Notion.Client.Page>> GetPagesForPublishingAsync(string databaseId)
            => Task.FromResult(new List<Notion.Client.Page>());

        public Task UpdatePagePropertiesAsync(string pageId, DateTime now)
        {
            if (UpdateException is not null) throw UpdateException;
            UpdatedPageIds.Add(pageId);
            return Task.CompletedTask;
        }

        public Task<List<NotionBlock>> FetchBlockTreeAsync(string blockId)
            => Task.FromResult(new List<NotionBlock>());
    }

    private class FakeGitHubEnvironmentUpdater : IGitHubEnvironmentUpdater
    {
        public int? LastExportedCount { get; private set; }
        public void UpdateEnvironment(int exportedCount) => LastExportedCount = exportedCount;
    }

    // ── ヘルパー ──────────────────────────────────────────────────────

    private TransformedPage MakePage(string pageId, string markdown = "# content") =>
        new(
            new PageProperty
            {
                PageId = pageId,
                PublicStatus = PublicStatus.Queued,
                PublishedDateTime = new DateTime(2024, 1, 1)
            },
            markdown,
            _tempDir);

    private static NotionPageLoader CreateSut(
        FakeNotionClient? client = null,
        FakeGitHubEnvironmentUpdater? ghUpdater = null) =>
        new(
            client ?? new FakeNotionClient(),
            ghUpdater ?? new FakeGitHubEnvironmentUpdater(),
            NullLogger<NotionPageLoader>.Instance);

    // ── テスト ────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_EmptyList_ReturnsZero()
    {
        var ghUpdater = new FakeGitHubEnvironmentUpdater();
        var sut = CreateSut(ghUpdater: ghUpdater);

        var result = await sut.LoadAsync([]);

        Assert.Equal(0, result);
        Assert.Equal(0, ghUpdater.LastExportedCount);
    }

    [Fact]
    public async Task LoadAsync_SinglePage_WritesIndexMd()
    {
        var sut = CreateSut();
        await sut.LoadAsync([MakePage("page-1", "# hello")]);

        var filePath = Path.Combine(_tempDir, "index.md");
        Assert.True(File.Exists(filePath));
        Assert.Contains("# hello", await File.ReadAllTextAsync(filePath, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LoadAsync_SinglePage_ReturnsOne()
    {
        var sut = CreateSut();
        var result = await sut.LoadAsync([MakePage("page-1")]);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task LoadAsync_SuccessfulLoad_CallsUpdatePageProperties()
    {
        var client = new FakeNotionClient();
        var sut = CreateSut(client: client);

        await sut.LoadAsync([MakePage("page-1")]);

        Assert.Contains("page-1", client.UpdatedPageIds);
    }

    [Fact]
    public async Task LoadAsync_UpdateFails_ExportedCountStillIncremented()
    {
        var client = new FakeNotionClient
        {
            UpdateException = new Exception("Notion API error")
        };
        var ghUpdater = new FakeGitHubEnvironmentUpdater();
        var sut = CreateSut(client: client, ghUpdater: ghUpdater);

        var result = await sut.LoadAsync([MakePage("page-1")]);

        Assert.Equal(1, result);
        Assert.Equal(1, ghUpdater.LastExportedCount);
    }

    [Fact]
    public async Task LoadAsync_MultiplePages_ReportsCorrectCount()
    {
        var ghUpdater = new FakeGitHubEnvironmentUpdater();
        var sut = CreateSut(ghUpdater: ghUpdater);

        var result = await sut.LoadAsync([MakePage("p1"), MakePage("p2"), MakePage("p3")]);

        Assert.Equal(3, result);
        Assert.Equal(3, ghUpdater.LastExportedCount);
    }

    [Fact]
    public async Task LoadAsync_WrittenFile_IsUtf8WithoutBom()
    {
        var sut = CreateSut();
        await sut.LoadAsync([MakePage("page-1", "テスト")]);

        var bytes = await File.ReadAllBytesAsync(Path.Combine(_tempDir, "index.md"), TestContext.Current.CancellationToken);
        // BOMあり UTF-8 は EF BB BF で始まる
        Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
    }
}
