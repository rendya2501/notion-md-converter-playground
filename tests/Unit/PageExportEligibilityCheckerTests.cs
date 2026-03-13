using NotionMarkdownConverter.Extract;
using NotionMarkdownConverter.Shared.Enums;
using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// PageExportEligibilityCheckerのユニットテスト
/// </summary>
public class PageExportEligibilityCheckerTests
{
    private readonly PageExportEligibilityChecker _sut = new();
    private readonly DateTime _now = new(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void ShouldExport_QueuedAndPastDate_ReturnsTrue()
    {
        var page = BuildPage(PublicStatus.Queued, publishedAt: new DateTime(2024, 1, 1));
        Assert.True(_sut.ShouldExport(page, _now));
    }

    [Fact]
    public void ShouldExport_ExactlyNow_ReturnsTrue()
    {
        // 境界値：公開日時と現在時刻が一致する場合はエクスポート対象
        var page = BuildPage(PublicStatus.Queued, publishedAt: _now);
        Assert.True(_sut.ShouldExport(page, _now));
    }

    [Fact]
    public void ShouldExport_AlreadyPublished_ReturnsFalse()
    {
        var page = BuildPage(PublicStatus.Published, publishedAt: new DateTime(2024, 1, 1));
        Assert.False(_sut.ShouldExport(page, _now));
    }

    [Fact]
    public void ShouldExport_Unpublished_ReturnsFalse()
    {
        // Queued 以外のステータスはエクスポートしない
        var page = BuildPage(PublicStatus.Unpublished, publishedAt: new DateTime(2024, 1, 1));
        Assert.False(_sut.ShouldExport(page, _now));
    }

    [Fact]
    public void ShouldExport_FutureDate_ReturnsFalse()
    {
        var page = BuildPage(PublicStatus.Queued, publishedAt: new DateTime(2024, 2, 1));
        Assert.False(_sut.ShouldExport(page, _now));
    }

    [Fact]
    public void ShouldExport_NullPublishedDateTime_ReturnsFalse()
    {
        var page = BuildPage(PublicStatus.Queued, publishedAt: null);
        Assert.False(_sut.ShouldExport(page, _now));
    }

    private static PageProperty BuildPage(PublicStatus status, DateTime? publishedAt) =>
        new() { PublicStatus = status, PublishedDateTime = publishedAt };
}
