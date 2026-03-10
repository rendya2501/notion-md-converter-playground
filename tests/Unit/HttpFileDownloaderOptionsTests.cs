using NotionMarkdownConverter.Infrastructure.Http;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// HttpFileDownloaderOptionsのユニットテスト。
/// MaxRetryCount セッターのバリデーションロジックを検証します。
/// </summary>
public class HttpFileDownloaderOptionsTests
{
    // ── MaxRetryCount バリデーション ──────────────────────────────────

    [Fact]
    public void MaxRetryCount_DefaultValue_IsThree()
    {
        var options = new HttpFileDownloaderOptions();
        Assert.Equal(3, options.MaxRetryCount);
    }

    [Fact]
    public void MaxRetryCount_SetToOne_DoesNotThrow()
    {
        var options = new HttpFileDownloaderOptions();
        var ex = Record.Exception(() => options.MaxRetryCount = 1);
        Assert.Null(ex);
    }

    [Fact]
    public void MaxRetryCount_SetToZero_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpFileDownloaderOptions();
        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxRetryCount = 0);
    }

    [Fact]
    public void MaxRetryCount_SetToNegative_ThrowsArgumentOutOfRangeException()
    {
        var options = new HttpFileDownloaderOptions();
        Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxRetryCount = -1);
    }

    [Fact]
    public void MaxRetryCount_ExceptionMessage_ContainsParameterName()
    {
        var options = new HttpFileDownloaderOptions();
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => options.MaxRetryCount = 0);
        Assert.Contains("MaxRetryCount", ex.Message);
    }

    // ── その他のデフォルト値 ──────────────────────────────────────────

    [Fact]
    public void RetryDelayMilliseconds_DefaultValue_IsOneThousand()
    {
        Assert.Equal(1000, new HttpFileDownloaderOptions().RetryDelayMilliseconds);
    }

    [Fact]
    public void TimeoutSeconds_DefaultValue_IsThirty()
    {
        Assert.Equal(30, new HttpFileDownloaderOptions().TimeoutSeconds);
    }

    [Fact]
    public void SkipExistingFiles_DefaultValue_IsTrue()
    {
        Assert.True(new HttpFileDownloaderOptions().SkipExistingFiles);
    }
}
