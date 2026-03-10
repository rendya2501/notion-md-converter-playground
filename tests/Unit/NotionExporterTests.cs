using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Application.Services;
using NotionMarkdownConverter.Domain.Enums;
using NotionMarkdownConverter.Domain.Models;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// NotionExporterのユニットテスト。
/// 外部依存はすべて手書きFakeで差し替えます。
/// </summary>
public class NotionExporterTests : IDisposable
{
    // テスト用の一時ディレクトリ（File.WriteAllTextAsync の実行先）
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), $"notion-exporter-test-{Guid.NewGuid().ToString("N")[..8]}");

    public NotionExporterTests()
    {
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // ── Fake 実装 ──────────────────────────────────────────────────────

    /// <summary>
    /// 返却するページ一覧と UpdatePagePropertiesAsync の動作を制御できる Fake。
    /// </summary>
    private class FakeNotionClient : INotionClientWrapper
    {
        // GetPagesForPublishingAsync が返すページ一覧
        public List<Page> Pages { get; set; } = [];

        // GetPagesForPublishingAsync に投げさせる例外（null なら正常）
        public Exception? GetPagesException { get; set; }

        // UpdatePagePropertiesAsync が呼ばれた PageId の記録
        public List<string> UpdatedPageIds { get; } = [];

        // UpdatePagePropertiesAsync に投げさせる例外（null なら正常）
        public Exception? UpdateException { get; set; }

        public Task<List<Page>> GetPagesForPublishingAsync(string databaseId)
        {
            if (GetPagesException is not null)
                throw GetPagesException;
            return Task.FromResult(Pages);
        }

        public Task UpdatePagePropertiesAsync(string pageId, DateTime now)
        {
            if (UpdateException is not null)
                throw UpdateException;
            UpdatedPageIds.Add(pageId);
            return Task.CompletedTask;
        }

        // INotionClientWrapper の他のメンバー（NotionExporter では未使用）
        public Task<List<NotionBlock>> FetchBlockTreeAsync(string pageId) =>
            Task.FromResult<List<NotionBlock>>([]);
    }

    /// <summary>
    /// AssembleAsync の動作を制御できる Fake。
    /// </summary>
    private class FakeMarkdownAssembler : IMarkdownAssembler
    {
        // AssembleAsync に投げさせる例外（null なら正常）
        public Exception? Exception { get; set; }

        public Task<string> AssembleAsync(PageProperty pageProperty, string outputDirectory)
        {
            if (Exception is not null)
                throw Exception;
            return Task.FromResult("# markdown content");
        }
    }

    /// <summary>
    /// UpdateEnvironment の呼び出し引数を記録する Fake。
    /// </summary>
    private class FakeGitHubEnvironmentUpdater : IGitHubEnvironmentUpdater
    {
        public int? LastExportedCount { get; private set; }

        public void UpdateEnvironment(int exportedCount)
        {
            LastExportedCount = exportedCount;
        }
    }

    /// <summary>
    /// 常に一時ディレクトリを返す Fake（BuildAndCreate の副作用はテスト側で管理）。
    /// </summary>
    private class FakeOutputDirectoryProvider(string tempDir) : IOutputDirectoryProvider
    {
        public string BuildAndCreate(PageProperty pageProperty) => tempDir;
    }

    /// <summary>
    /// Map の動作を制御できる Fake。
    /// PublicStatus=Queued・PublishedDateTime=過去 の PageProperty を返すことで
    /// PageExportEligibilityChecker の判定をパスさせます。
    /// </summary>
    private class FakePagePropertyMapper : IPagePropertyMapper
    {
        // Map が投げさせる例外（null なら正常）
        public Exception? Exception { get; set; }

        public PageProperty Map(Page page)
        {
            if (Exception is not null)
                throw Exception;

            return new PageProperty
            {
                // エクスポート対象になる最低条件を満たす値を設定
                PublicStatus = PublicStatus.Queued,
                PublishedDateTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Slug = "test-slug",
                Title = "テストタイトル"
            };
        }
    }

    // ── ビルダー ──────────────────────────────────────────────────────

    private NotionExporter BuildSut(
        INotionClientWrapper? notionClient = null,
        IMarkdownAssembler? assembler = null,
        IGitHubEnvironmentUpdater? ghUpdater = null,
        IOutputDirectoryProvider? dirProvider = null,
        IPagePropertyMapper? mapper = null)
    {
        var config = Options.Create(new NotionExportOptions
        {
            NotionDatabaseId = "test-db-id",
            NotionAuthToken = "test-token"
        });

        return new NotionExporter(
            config,
            notionClient ?? new FakeNotionClient(),
            assembler ?? new FakeMarkdownAssembler(),
            ghUpdater ?? new FakeGitHubEnvironmentUpdater(),
            dirProvider ?? new FakeOutputDirectoryProvider(_tempDir),
            mapper ?? new FakePagePropertyMapper(),
            new PageExportEligibilityChecker(),
            NullLogger<NotionExporter>.Instance);
    }

    /// <summary>
    /// ID のみ持つ最小限のテスト用 Page を生成します。
    /// </summary>
    private static Page MakePage(string id) => new() { Id = id };

    // ── テスト ────────────────────────────────────────────────────────

    // ── ページ一覧の取得失敗 ─────────────────────────────────────────

    [Fact]
    public async Task ExportPagesAsync_WhenGetPagesFails_ThrowsException()
    {
        // ページ一覧の取得が失敗した場合は例外を再スローして処理を中断します。
        var notionClient = new FakeNotionClient
        {
            GetPagesException = new Exception("API error")
        };

        var sut = BuildSut(notionClient: notionClient);

        await Assert.ThrowsAsync<Exception>(() => sut.ExportPagesAsync());
    }

    [Fact]
    public async Task ExportPagesAsync_WhenGetPagesFails_DoesNotCallUpdateEnvironment()
    {
        // ページ一覧取得失敗時は UpdateEnvironment が呼ばれないことを確認します。
        var notionClient = new FakeNotionClient
        {
            GetPagesException = new Exception("API error")
        };
        var ghUpdater = new FakeGitHubEnvironmentUpdater();

        var sut = BuildSut(notionClient: notionClient, ghUpdater: ghUpdater);

        await Assert.ThrowsAsync<Exception>(() => sut.ExportPagesAsync());
        Assert.Null(ghUpdater.LastExportedCount);
    }

    // ── エクスポート件数のカウント ────────────────────────────────────

    [Fact]
    public async Task ExportPagesAsync_AllPagesExported_ReportsCorrectCount()
    {
        // 3ページすべてエクスポート成功した場合、UpdateEnvironment に 3 が渡ります。
        var notionClient = new FakeNotionClient
        {
            Pages = [MakePage("page-1"), MakePage("page-2"), MakePage("page-3")]
        };
        var ghUpdater = new FakeGitHubEnvironmentUpdater();

        var sut = BuildSut(notionClient: notionClient, ghUpdater: ghUpdater);
        await sut.ExportPagesAsync();

        Assert.Equal(3, ghUpdater.LastExportedCount);
    }

    [Fact]
    public async Task ExportPagesAsync_NoPagesExported_ReportsZero()
    {
        // ページが0件のとき、UpdateEnvironment に 0 が渡ります。
        var ghUpdater = new FakeGitHubEnvironmentUpdater();

        var sut = BuildSut(ghUpdater: ghUpdater);
        await sut.ExportPagesAsync();

        Assert.Equal(0, ghUpdater.LastExportedCount);
    }

    // ── スキップ（対象外ページ）──────────────────────────────────────

    [Fact]
    public async Task ExportPagesAsync_IneligiblePage_IsNotCountedAndNotUpdated()
    {
        // PublicStatus が Queued でないページはスキップされ、
        // UpdatePagePropertiesAsync も呼ばれません。
        var notionClient = new FakeNotionClient
        {
            Pages = [MakePage("skip-page")]
        };
        var ghUpdater = new FakeGitHubEnvironmentUpdater();

        // Published を返すことで PageExportEligibilityChecker の判定を落とします。
        var mapper = new FakePagePropertyMapper();
        // FakePagePropertyMapper の代わりにインライン Fake を使う場合は
        // 別クラスが必要なため、ここでは PublicStatus=Published を返す
        // サブクラスで上書きします。
        var ineligibleMapper = new IneligiblePagePropertyMapper();

        var sut = BuildSut(notionClient: notionClient, ghUpdater: ghUpdater, mapper: ineligibleMapper);
        await sut.ExportPagesAsync();

        Assert.Equal(0, ghUpdater.LastExportedCount);
        Assert.Empty(notionClient.UpdatedPageIds);
    }

    // エクスポート対象外（PublicStatus=Published）を返す Fake
    private class IneligiblePagePropertyMapper : IPagePropertyMapper
    {
        public PageProperty Map(Page page) => new()
        {
            PublicStatus = PublicStatus.Published, // Queued でないのでスキップ
            PublishedDateTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
    }

    // ── 1ページの失敗が他ページに波及しない ─────────────────────────

    [Fact]
    public async Task ExportPagesAsync_OnePageFails_OtherPagesStillExported()
    {
        // 2ページ中 1ページの AssembleAsync が失敗しても、残り1ページは処理されます。
        var notionClient = new FakeNotionClient
        {
            Pages = [MakePage("fail-page"), MakePage("success-page")]
        };
        var ghUpdater = new FakeGitHubEnvironmentUpdater();

        // 1回目の呼び出しだけ失敗させる Fake
        var assembler = new FirstCallFailsAssembler();

        var sut = BuildSut(notionClient: notionClient, ghUpdater: ghUpdater, assembler: assembler);
        await sut.ExportPagesAsync();

        // 失敗した1ページは除き、成功した1ページ分だけカウントされます。
        Assert.Equal(1, ghUpdater.LastExportedCount);
    }

    private class FirstCallFailsAssembler : IMarkdownAssembler
    {
        private int _callCount = 0;

        public Task<string> AssembleAsync(PageProperty pageProperty, string outputDirectory)
        {
            if (_callCount++ == 0)
                throw new Exception("1回目の呼び出しを失敗させる");
            return Task.FromResult("# markdown");
        }
    }

    // ── UpdatePagePropertiesAsync の失敗が他ページに波及しない ────────

    [Fact]
    public async Task ExportPagesAsync_UpdateFails_OtherPagesStillExported()
    {
        // UpdatePagePropertiesAsync が失敗しても残りページの処理は継続します。
        var notionClient = new FakeNotionClient
        {
            Pages = [MakePage("page-1"), MakePage("page-2")],
            UpdateException = new Exception("Notion API update error")
        };
        var ghUpdater = new FakeGitHubEnvironmentUpdater();

        var sut = BuildSut(notionClient: notionClient, ghUpdater: ghUpdater);
        await sut.ExportPagesAsync();

        // 更新失敗でも2ページ処理が完走し、件数に加算されます。
        Assert.Equal(2, ghUpdater.LastExportedCount);
    }

    [Fact]
    public async Task ExportPagesAsync_UpdateFails_ExportedCountIsStillIncremented()
    {
        // UpdatePagePropertiesAsync が失敗してもエクスポート件数には加算されます。
        // 「ファイル書き出し成功」と「Notion側の状態更新成功」は別の関心事のためです。
        var notionClient = new FakeNotionClient
        {
            Pages = [MakePage("page-1")],
            UpdateException = new Exception("Notion API update error")
        };
        var ghUpdater = new FakeGitHubEnvironmentUpdater();

        var sut = BuildSut(notionClient: notionClient, ghUpdater: ghUpdater);
        await sut.ExportPagesAsync();

        Assert.Equal(1, ghUpdater.LastExportedCount);
    }

    // ── UpdatePagePropertiesAsync の呼び出し確認 ────────────────────

    [Fact]
    public async Task ExportPagesAsync_SuccessfulExport_CallsUpdatePageProperties()
    {
        // エクスポート成功後、対象ページの UpdatePagePropertiesAsync が呼ばれます。
        var notionClient = new FakeNotionClient
        {
            Pages = [MakePage("page-abc")]
        };

        var sut = BuildSut(notionClient: notionClient);
        await sut.ExportPagesAsync();

        Assert.Contains("page-abc", notionClient.UpdatedPageIds);
    }

    [Fact]
    public async Task ExportPagesAsync_FailedExport_DoesNotCallUpdatePageProperties()
    {
        // エクスポートが失敗したページは UpdatePagePropertiesAsync が呼ばれません。
        var notionClient = new FakeNotionClient
        {
            Pages = [MakePage("fail-page")]
        };
        var assembler = new FakeMarkdownAssembler
        {
            Exception = new Exception("組み立て失敗")
        };

        var sut = BuildSut(notionClient: notionClient, assembler: assembler);
        await sut.ExportPagesAsync();

        Assert.Empty(notionClient.UpdatedPageIds);
    }
}
