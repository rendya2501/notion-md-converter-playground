using NotionMarkdownConverter.Shared.Constants;
using NotionMarkdownConverter.Transform;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// MarkdownLinkReplacerのユニットテスト。
/// ファイルのダウンロードは行わず、文字列置換とUrlFilePair生成のみを検証します。
/// </summary>
public class MarkdownLinkReplacerTests
{
    private readonly MarkdownLinkReplacer _sut = new();

    // ── ダウンロードリンクなし ────────────────────────────────────────

    [Fact]
    public void Replace_NoDownloadMarker_ReturnsUnchangedMarkdown()
    {
        var markdown = "# 見出し\n\nテキスト [リンク](https://example.com)\n\n![画像](https://example.com/img.png)";

        var (replacedMarkdown, urlFilePairs) = _sut.Replace(markdown);

        Assert.Equal(markdown, replacedMarkdown);
        Assert.Empty(urlFilePairs);
    }

    // ── リンク置換 ────────────────────────────────────────────────────

    [Fact]
    public void Replace_ImageWithMarker_ReplacesUrlWithLocalFileName()
    {
        var originalUrl = "https://example.com/image.png";
        var markedUrl = MarkdownConstants.DownloadMarker + originalUrl;
        var markdown = $"![代替テキスト]({markedUrl})";

        var (replacedMarkdown, _) = _sut.Replace(markdown);

        Assert.DoesNotContain(MarkdownConstants.DownloadMarker, replacedMarkdown);
        Assert.Matches(@"!\[代替テキスト\]\([0-9A-F]+\.png\)", replacedMarkdown);
    }

    [Fact]
    public void Replace_FileLink_ReplacesUrlWithLocalFileName()
    {
        var originalUrl = "https://example.com/document.pdf";
        var markedUrl = MarkdownConstants.DownloadMarker + originalUrl;
        var markdown = $"[ドキュメント]({markedUrl})";

        var (replacedMarkdown, _) = _sut.Replace(markdown);

        Assert.DoesNotContain(MarkdownConstants.DownloadMarker, replacedMarkdown);
    }

    // ── UrlFilePair 生成 ──────────────────────────────────────────────

    [Fact]
    public void Replace_ImageWithMarker_ReturnsCorrectUrlFilePair()
    {
        var originalUrl = "https://example.com/image.png";
        var markedUrl = MarkdownConstants.DownloadMarker + originalUrl;
        var markdown = $"![img]({markedUrl})";

        var (_, urlFilePairs) = _sut.Replace(markdown);

        var pair = Assert.Single(urlFilePairs);
        // マーカーが除去されたURLが使用される
        Assert.Equal(originalUrl, pair.OriginalUrl);
        // ローカルファイル名が生成されている
        Assert.NotEmpty(pair.LocalFileName);
    }

    // ── 複数リンク ────────────────────────────────────────────────────

    [Fact]
    public void Replace_MultipleMarkedLinks_ReturnsAllUrlFilePairs()
    {
        var url1 = "https://example.com/image1.png";
        var url2 = "https://example.com/image2.jpg";
        var markdown = $"![img1]({MarkdownConstants.DownloadMarker + url1})\n![img2]({MarkdownConstants.DownloadMarker + url2})";

        var (replacedMarkdown, urlFilePairs) = _sut.Replace(markdown);

        Assert.Equal(2, urlFilePairs.Count());
        Assert.Contains(urlFilePairs, p => p.OriginalUrl == url1);
        Assert.Contains(urlFilePairs, p => p.OriginalUrl == url2);
        Assert.DoesNotContain(MarkdownConstants.DownloadMarker, replacedMarkdown);
    }

    // ── 重複リンク ────────────────────────────────────────────────────

    [Fact]
    public void Replace_DuplicateLinks_ReturnsOneUrlFilePair()
    {
        var url = "https://example.com/image.png";
        var markedUrl = MarkdownConstants.DownloadMarker + url;
        var markdown = $"![img1]({markedUrl})\n![img2]({markedUrl})";

        var (replacedMarkdown, urlFilePairs) = _sut.Replace(markdown);

        // URLFilePairは1つだけ
        Assert.Single(urlFilePairs);
        // 両方の画像が同じローカルファイル名に置換される
        var localFileName = urlFilePairs.First().LocalFileName;
        Assert.Equal(2, CountOccurrences(replacedMarkdown, localFileName));
    }

    // ── ローカルファイル名の一貫性 ────────────────────────────────────

    [Fact]
    public void Replace_SameUrl_GeneratesSameLocalFileName()
    {
        var url = "https://example.com/image.png";
        var markedUrl = MarkdownConstants.DownloadMarker + url;

        var (result1, _) = _sut.Replace($"![img]({markedUrl})");
        var (result2, _) = _sut.Replace($"![img]({markedUrl})");

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Replace_DifferentUrls_GenerateDifferentLocalFileNames()
    {
        var url1 = "https://example.com/image1.png";
        var url2 = "https://example.com/image2.png";

        var (_, pairs1) = _sut.Replace($"![img]({MarkdownConstants.DownloadMarker + url1})");
        var (_, pairs2) = _sut.Replace($"![img]({MarkdownConstants.DownloadMarker + url2})");

        Assert.NotEqual(pairs1.First().LocalFileName, pairs2.First().LocalFileName);
    }

    // ── 拡張子の保持 ──────────────────────────────────────────────────

    [Theory]
    [InlineData("image.png", ".png")]
    [InlineData("image.jpg", ".jpg")]
    [InlineData("document.pdf", ".pdf")]
    [InlineData("video.mp4", ".mp4")]
    public void Replace_PreservesFileExtension(string filename, string expectedExt)
    {
        var url = $"https://example.com/{filename}";
        var markdown = $"![img]({MarkdownConstants.DownloadMarker + url})";

        var (_, urlFilePairs) = _sut.Replace(markdown);

        Assert.EndsWith(expectedExt, urlFilePairs.First().LocalFileName);
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
