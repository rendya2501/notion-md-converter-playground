using Notion.Client;
using NotionMarkdownConverter.Shared.Constants;
using NotionMarkdownConverter.Shared.Models;
using NotionMarkdownConverter.Transform.Strategies;
using NotionMarkdownConverter.Transform.Strategies.Context;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// VideoTransformStrategyのユニットテスト。
/// </summary>
public class VideoTransformStrategyTests
{
    private readonly VideoTransformStrategy _sut = new();

    // ── ヘルパー ──────────────────────────────────────────────────────

    private static NotionBlockTransformContext MakeContext(NotionBlock block) => new()
    {
        ExecuteTransformBlocks = _ => string.Empty,
        Blocks = [block],
        CurrentBlock = block,
        CurrentBlockIndex = 0
    };

    // ── BlockType ─────────────────────────────────────────────────────

    [Fact]
    public void BlockType_IsVideo()
    {
        Assert.Equal(BlockType.Video, _sut.BlockType);
    }

    // ── UploadedFile ──────────────────────────────────────────────────
   
    [Fact]
    public void Transform_UploadedFile_ReturnsLinkWithDownloadMarker()
    {
        var uploadedFile = new UploadedFile();
        uploadedFile.File = new() { Url = "https://cdn.notion.so/video.mp4" };
        var block = new NotionBlock(new VideoBlock { Video = uploadedFile });

        var result = _sut.Transform(MakeContext(block));

        Assert.Contains(MarkdownConstants.DownloadMarker, result);
        Assert.Contains("https://cdn.notion.so/video.mp4", result);
    }

    [Fact]
    public void Transform_UploadedFile_ReturnsUrlAsIs()
    {
        var uploadedFile = new UploadedFile
        {
            File = new() { Url = "https://cdn.notion.so/video.mp4" }
        };
        var block = new NotionBlock(new VideoBlock { Video = uploadedFile });

        var result = _sut.Transform(MakeContext(block));

        Assert.Contains(MarkdownConstants.DownloadMarker, result);
        Assert.Contains("https://cdn.notion.so/video.mp4", result);
    }

    // ── ExternalFile ──────────────────────────────────────────────────

    [Fact]
    public void Transform_ExternalFile_ReturnsUrlAsIs()
    {
        var block = new NotionBlock(new VideoBlock
        {
            Video = new ExternalFile
            {
                External = new ExternalFile.Info { Url = "https://example.com/video.mp4" }
            }
        });

        var result = _sut.Transform(MakeContext(block));

        Assert.Equal("https://example.com/video.mp4  ", result);
    }

    [Fact]
    public void Transform_YouTubeUrl_ReturnsUrlAsIs()
    {
        var url = "https://www.youtube.com/watch?v=abc123";
        var block = new NotionBlock(new VideoBlock
        {
            Video = new ExternalFile { External = new ExternalFile.Info { Url = url } }
        });

        var result = _sut.Transform(MakeContext(block));

        // YouTubeもURLそのまま（Zennが自動で埋め込みに変換）
        Assert.Equal(url, result);
    }
}
