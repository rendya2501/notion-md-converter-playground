using Microsoft.Extensions.Logging.Abstractions;
using Notion.Client;
using NotionMarkdownConverter.Extract;
using NotionMarkdownConverter.Infrastructure;
using NotionMarkdownConverter.Shared.Enums;
using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Tests.Unit;

public class NotionPageExtractorTests
{
    // ── Fake ──────────────────────────────────────────────────────────

    private class FakeNotionClient : INotionClientWrapper
    {
        public List<Page> Pages { get; set; } = [];
        public Exception? GetPagesException { get; set; }
        public List<string> FetchedBlockPageIds { get; } = [];

        public Task<List<Page>> GetPagesForPublishingAsync(string databaseId)
        {
            if (GetPagesException is not null) throw GetPagesException;
            return Task.FromResult(Pages);
        }

        public Task<List<NotionBlock>> FetchBlockTreeAsync(string blockId)
        {
            FetchedBlockPageIds.Add(blockId);
            return Task.FromResult(new List<NotionBlock>());
        }

        public Task UpdatePagePropertiesAsync(string pageId, DateTime now)
            => Task.CompletedTask;
    }

    private class FakePagePropertyMapper : IPagePropertyMapper
    {
        public PageProperty Map(Page page) => new()
        {
            PageId = page.Id,
            PublicStatus = PublicStatus.Queued,
            PublishedDateTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
    }

    private class IneligiblePagePropertyMapper : IPagePropertyMapper
    {
        public PageProperty Map(Page page) => new()
        {
            PageId = page.Id,
            PublicStatus = PublicStatus.Published
        };
    }

    // ── ヘルパー ──────────────────────────────────────────────────────

    private static Page MakePage(string id) => new() { Id = id };

    private static NotionPageExtractor CreateSut(
        FakeNotionClient? client = null,
        IPagePropertyMapper? mapper = null) =>
        new(
            client ?? new FakeNotionClient(),
            mapper ?? new FakePagePropertyMapper(),
            new PageExportEligibilityChecker(),
            NullLogger<NotionPageExtractor>.Instance);

    // ── テスト ────────────────────────────────────────────────────────

    [Fact]
    public async Task ExtractAsync_GetPagesFails_ThrowsException()
    {
        var client = new FakeNotionClient
        {
            GetPagesException = new Exception("API error")
        };
        var sut = CreateSut(client: client);

        await Assert.ThrowsAsync<Exception>(() => sut.ExtractAsync("db-id"));
    }

    [Fact]
    public async Task ExtractAsync_NoPages_ReturnsEmptyList()
    {
        var sut = CreateSut();
        var result = await sut.ExtractAsync("db-id");
        Assert.Empty(result);
    }

    [Fact]
    public async Task ExtractAsync_EligiblePage_ReturnsExtractedPage()
    {
        var client = new FakeNotionClient { Pages = [MakePage("page-1")] };
        var sut = CreateSut(client: client);

        var result = await sut.ExtractAsync("db-id");

        Assert.Single(result);
        Assert.Equal("page-1", result[0].PageProperty.PageId);
    }

    [Fact]
    public async Task ExtractAsync_EligiblePage_FetchesBlockTree()
    {
        var client = new FakeNotionClient { Pages = [MakePage("page-1")] };
        var sut = CreateSut(client: client);

        await sut.ExtractAsync("db-id");

        Assert.Contains("page-1", client.FetchedBlockPageIds);
    }

    [Fact]
    public async Task ExtractAsync_IneligiblePage_IsExcluded()
    {
        var client = new FakeNotionClient { Pages = [MakePage("skip-page")] };
        var sut = CreateSut(client: client, mapper: new IneligiblePagePropertyMapper());

        var result = await sut.ExtractAsync("db-id");

        Assert.Empty(result);
        Assert.Empty(client.FetchedBlockPageIds);
    }

    [Fact]
    public async Task ExtractAsync_OnePageFails_OtherPagesStillExtracted()
    {
        var client = new FakeNotionClient { Pages = [MakePage("page-1"), MakePage("page-2")] };
        var mapper = new ThrowOnFirstCallMapper();
        var sut = CreateSut(client: client, mapper: mapper);

        var result = await sut.ExtractAsync("db-id");

        Assert.Single(result);
    }

    private class ThrowOnFirstCallMapper : IPagePropertyMapper
    {
        private int _callCount = 0;
        public PageProperty Map(Page page)
        {
            if (_callCount++ == 0) throw new Exception("マッパー失敗");
            return new PageProperty
            {
                PageId = page.Id,
                PublicStatus = PublicStatus.Queued,
                PublishedDateTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
        }
    }
}
