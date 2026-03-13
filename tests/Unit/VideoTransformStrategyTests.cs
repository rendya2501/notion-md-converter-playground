using Notion.Client;
using NotionMarkdownConverter.Shared.Models;
using NotionMarkdownConverter.Transform.Strategies;
using NotionMarkdownConverter.Transform.Strategies.Context;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// VideoTransformStrategyのユニットテスト
/// </summary>
public class VideoTransformStrategyTests
{
    private readonly VideoTransformStrategy _sut = new();

    // ── ヘルパー ──────────────────────────────────────────────────────

    private static NotionBlock MakeVideoBlock(ExternalFile video)
    {
        var block = new VideoBlock { Video = video };
        return new NotionBlock(block);
    }

    private static NotionBlock MakeVideoBlock(UploadedFile video)
    {
        var block = new VideoBlock { Video = video };
        return new NotionBlock(block);
    }

    private static NotionBlockTransformContext MakeContext(NotionBlock block) => new()
    {
        ExecuteTransformBlocks = _ => string.Empty,
        Blocks = [block],
        CurrentBlock = block,
        CurrentBlockIndex = 0
    };

    private string Transform(ExternalFile video) =>
        _sut.Transform(MakeContext(MakeVideoBlock(video)));

    private string Transform(UploadedFile video) =>
        _sut.Transform(MakeContext(MakeVideoBlock(video)));

    // ── BlockType ─────────────────────────────────────────────────────

    [Fact]
    public void BlockType_IsVideo()
    {
        Assert.Equal(BlockType.Video, _sut.BlockType);
    }

    // ── アップロードファイル ───────────────────────────────────────────

    [Fact]
    public void Transform_UploadedFile_ReturnsVideoTag()
    {
        var uploadedFile = new UploadedFile();
        uploadedFile.File = new() { Url = "https://cdn.notion.so/video.mp4" };
        var result = Transform(uploadedFile);

        Assert.Equal("<video controls src=\"https://cdn.notion.so/video.mp4\"></video>", result);
    }

    // ── YouTube ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=abc123")]
    [InlineData("https://youtube.com/watch?v=abc123")]
    [InlineData("https://youtu.be/abc123")]
    [InlineData("https://www.youtube.com/shorts/abc123")]
    public void Transform_YouTubeUrl_ReturnsIFrameTag(string url)
    {
        var result = Transform(new ExternalFile { External = new ExternalFile.Info { Url = url } });

        Assert.Contains("<iframe", result);
        Assert.Contains("youtube.com/embed/abc123", result);
        Assert.DoesNotContain("<video", result);
    }

    [Fact]
    public void Transform_YouTubeWatchUrl_UsesVideoIdFromQueryString()
    {
        var result = Transform(new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ" }
        });

        Assert.Contains("embed/dQw4w9WgXcQ", result);
    }

    [Fact]
    public void Transform_YouTubeShortUrl_UsesVideoIdFromPath()
    {
        var result = Transform(new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://youtu.be/dQw4w9WgXcQ" }
        });

        Assert.Contains("embed/dQw4w9WgXcQ", result);
    }

    [Fact]
    public void Transform_YouTubeShortsUrl_UsesVideoIdFromPath()
    {
        var result = Transform(new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://www.youtube.com/shorts/dQw4w9WgXcQ" }
        });

        Assert.Contains("embed/dQw4w9WgXcQ", result);
    }

    [Fact]
    public void Transform_YouTubeIFrame_HasAllowFullscreenAttribute()
    {
        var result = Transform(new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://youtu.be/abc123" }
        });

        Assert.Contains("allowfullscreen", result);
    }

    // ── その他の外部URL ───────────────────────────────────────────────

    [Fact]
    public void Transform_ExternalVideoFile_ReturnsVideoTag()
    {
        var result = Transform(new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://example.com/video.mp4" }
        });

        Assert.Equal("<video controls src=\"https://example.com/video.mp4\"></video>", result);
    }

    [Fact]
    public void Transform_ExternalVideoFile_DoesNotReturnIFrame()
    {
        var result = Transform(new ExternalFile
        {
            External = new ExternalFile.Info { Url = "https://vimeo.com/123456789" }
        });

        // Vimeo は YouTube ではないので <video> タグで出力
        Assert.Contains("<video", result);
        Assert.DoesNotContain("<iframe", result);
    }

    // ── フォールバック ────────────────────────────────────────────────

    [Fact]
    public void Transform_InvalidUrl_ReturnsMarkdownLink()
    {
        var result = Transform(new ExternalFile
        {
            External = new ExternalFile.Info { Url = "not-a-valid-url" }
        });

        // 不正なURLはMarkdownリンクにフォールバック
        Assert.Contains("[not-a-valid-url]", result);
        Assert.DoesNotContain("<video", result);
        Assert.DoesNotContain("<iframe", result);
    }

    [Theory]
    [InlineData("https://youtu.be/")]                        // youtu.be のパスが空
    [InlineData("https://www.youtube.com/watch")]            // ?v= クエリなし
    [InlineData("https://www.youtube.com/watch?list=PLxxx")] // v= パラメータなし
    [InlineData("https://www.youtube.com/shorts/")]          // /shorts/ の後が空
    public void Transform_YouTubeUrlWithNoExtractableVideoId_FallsBackToMarkdownLink(string url)
    {
        // ToYouTubeEmbedUrl が null を返す（動画ID抽出失敗）→ Markdownリンクにフォールバック
        var result = Transform(new ExternalFile
        {
            External = new ExternalFile.Info { Url = url }
        });

        Assert.DoesNotContain("<iframe", result);
        Assert.DoesNotContain("<video", result);
        Assert.Contains($"[{url}]({url})", result);
    }
}
