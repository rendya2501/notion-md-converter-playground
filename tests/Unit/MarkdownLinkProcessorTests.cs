using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Services;
using NotionMarkdownConverter.Domain.Constants;
using NotionMarkdownConverter.Domain.Models;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// MarkdownLinkProcessorのユニットテスト。
/// IFileDownloader はテスト用スタブで差し替えます。
/// </summary>
public class MarkdownLinkProcessorTests
{
    // ── スタブ ────────────────────────────────────────────────────────

    /// <summary>
    /// ダウンロード呼び出しを記録するだけのスタブ。
    /// </summary>
    private sealed class StubFileDownloader : IFileDownloader
    {
        public List<(string Url, string FileName, string OutputDir)> Calls { get; } = [];

        public Task DownloadAsync(UrlFilePair urlFilePair, string outputDirectory)
        {
            Calls.Add((urlFilePair.OriginalUrl, urlFilePair.ConversionFileName, outputDirectory));
            return Task.CompletedTask;
        }
    }

    private readonly StubFileDownloader _stub = new();
    private MarkdownLinkProcessor CreateSut() => new(_stub);

    // ── ダウンロードリンクなし ────────────────────────────────────────

    [Fact]
    public async Task ProcessLinksAsync_NoDownloadMarker_ReturnsUnchanged()
    {
        var sut = CreateSut();
        var markdown = "# 見出し\n\nテキスト [リンク](https://example.com)\n\n![画像](https://example.com/img.png)";

        var result = await sut.ProcessLinksAsync(markdown, "/output");

        Assert.Equal(markdown, result);
        Assert.Empty(_stub.Calls);
    }

    // ── 画像リンクのダウンロードマーカー ─────────────────────────────

    [Fact]
    public async Task ProcessLinksAsync_ImageWithMarker_TriggersDownload()
    {
        var sut = CreateSut();
        var originalUrl = "https://example.com/image.png";
        var markedUrl = LinkConstants.DownloadMarker + originalUrl;
        var markdown = $"![代替テキスト]({markedUrl})";

        var result = await sut.ProcessLinksAsync(markdown, "/output");

        // ダウンロードが1回呼ばれた
        Assert.Single(_stub.Calls);
        // サニタイズ後のURLが使用された
        Assert.Equal(originalUrl, _stub.Calls[0].Url);
        // 出力ディレクトリが正しく渡された
        Assert.Equal("/output", _stub.Calls[0].OutputDir);
    }

    [Fact]
    public async Task ProcessLinksAsync_ImageWithMarker_ReplacesUrlWithLocalFileName()
    {
        var sut = CreateSut();
        var originalUrl = "https://example.com/image.png";
        var markedUrl = LinkConstants.DownloadMarker + originalUrl;
        var markdown = $"![代替テキスト]({markedUrl})";

        var result = await sut.ProcessLinksAsync(markdown, "/output");

        // マーカー付きURLが結果に含まれない
        Assert.DoesNotContain(LinkConstants.DownloadMarker, result);
        // ローカルファイル名（MD5ハッシュ+拡張子）に置換されている
        Assert.Matches(@"!\[代替テキスト\]\([0-9A-F]+\.png\)", result);
    }

    // ── ファイルリンクのダウンロードマーカー ──────────────────────────

    [Fact]
    public async Task ProcessLinksAsync_FileLink_TriggersDownload()
    {
        var sut = CreateSut();
        var originalUrl = "https://example.com/document.pdf";
        var markedUrl = LinkConstants.DownloadMarker + originalUrl;
        var markdown = $"[ドキュメント]({markedUrl})";

        var result = await sut.ProcessLinksAsync(markdown, "/output");

        Assert.Single(_stub.Calls);
        Assert.Equal(originalUrl, _stub.Calls[0].Url);
        Assert.DoesNotContain(LinkConstants.DownloadMarker, result);
    }

    // ── 複数リンク ────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessLinksAsync_MultipleMarkedLinks_TriggersAllDownloads()
    {
        var sut = CreateSut();
        var url1 = "https://example.com/image1.png";
        var url2 = "https://example.com/image2.jpg";
        var markdown = $"![img1]({LinkConstants.DownloadMarker + url1})\n![img2]({LinkConstants.DownloadMarker + url2})";

        var result = await sut.ProcessLinksAsync(markdown, "/output");

        Assert.Equal(2, _stub.Calls.Count);
        Assert.Contains(_stub.Calls, c => c.Url == url1);
        Assert.Contains(_stub.Calls, c => c.Url == url2);
        Assert.DoesNotContain(LinkConstants.DownloadMarker, result);
    }

    // ── 重複リンク ────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessLinksAsync_DuplicateLinks_DownloadsOnlyOnce()
    {
        var sut = CreateSut();
        var url = "https://example.com/image.png";
        var markedUrl = LinkConstants.DownloadMarker + url;
        // 同じURLが2回登場
        var markdown = $"![img1]({markedUrl})\n![img2]({markedUrl})";

        var result = await sut.ProcessLinksAsync(markdown, "/output");

        // ダウンロードは1回だけ
        Assert.Single(_stub.Calls);
        // 両方の画像が同じローカルファイル名に置換される
        var localFileName = _stub.Calls[0].FileName;
        Assert.Equal(2, CountOccurrences(result, localFileName));
    }

    // ── ローカルファイル名の一貫性 ────────────────────────────────────

    [Fact]
    public async Task ProcessLinksAsync_SameUrl_GeneratesSameLocalFileName()
    {
        // 2回処理しても同じファイル名になることを確認
        var url = "https://example.com/image.png";
        var markedUrl = LinkConstants.DownloadMarker + url;

        var result1 = await CreateSut().ProcessLinksAsync($"![img]({markedUrl})", "/out");
        var result2 = await CreateSut().ProcessLinksAsync($"![img]({markedUrl})", "/out");

        Assert.Equal(result1, result2);
    }

    [Fact]
    public async Task ProcessLinksAsync_DifferentUrls_GenerateDifferentLocalFileNames()
    {
        var url1 = "https://example.com/image1.png";
        var url2 = "https://example.com/image2.png";

        var stub1 = new StubFileDownloader();
        var stub2 = new StubFileDownloader();
        var sut1 = new MarkdownLinkProcessor(stub1);
        var sut2 = new MarkdownLinkProcessor(stub2);

        await sut1.ProcessLinksAsync($"![img]({LinkConstants.DownloadMarker + url1})", "/out");
        await sut2.ProcessLinksAsync($"![img]({LinkConstants.DownloadMarker + url2})", "/out");

        var fileName1 = stub1.Calls[0].FileName;
        var fileName2 = stub2.Calls[0].FileName;

        Assert.NotEqual(fileName1, fileName2);
    }

    // ── 拡張子の保持 ──────────────────────────────────────────────────

    [Theory]
    [InlineData("image.png", ".png")]
    [InlineData("image.jpg", ".jpg")]
    [InlineData("document.pdf", ".pdf")]
    [InlineData("video.mp4", ".mp4")]
    public async Task ProcessLinksAsync_PreservesFileExtension(string filename, string expectedExt)
    {
        var sut = CreateSut();
        var url = $"https://example.com/{filename}";
        var markdown = $"![img]({LinkConstants.DownloadMarker + url})";

        await sut.ProcessLinksAsync(markdown, "/output");

        var localFileName = _stub.Calls[0].FileName;
        Assert.EndsWith(expectedExt, localFileName);
    }

    // ── ヘルパー ──────────────────────────────────────────────────────

    private static int CountOccurrences(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }
        return count;
    }
}
