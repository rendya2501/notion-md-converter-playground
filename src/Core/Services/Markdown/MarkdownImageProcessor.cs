using NotionMarkdownConverter.Infrastructure.Http.Model;
using NotionMarkdownConverter.Infrastructure.Http.Services;
using System.Text.RegularExpressions;

namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// マークダウン内の画像を処理するサービス
/// </summary>
public class MarkdownImageProcessor(IImageDownloader _imageDownloader) : IMarkdownImageProcessor
{
    /// <summary>
    /// マークダウン内の画像を処理する
    /// </summary>
    /// <param name="markdown"></param>
    /// <param name="outputDirectory"></param>
    /// <returns>マークダウン</returns>
    public async Task<string> ProcessMarkdownImagesAsync(string markdown, string outputDirectory)
    {
        var imageUrls = ExtractImageUrls(markdown);
        
        var downloadTasks = imageUrls.Select(async url =>
            await _imageDownloader.DownloadAsync(url, outputDirectory));
        var downloadedImages = await Task.WhenAll(downloadTasks);

        return ReplaceImageUrls(markdown, downloadedImages);
    }

    /// <summary>
    /// マークダウン内の画像URLを抽出する
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    private static IEnumerable<string> ExtractImageUrls(string markdown)
    {
        var pattern = @"!\[.*?\]\((.*?)\)";
        return Regex.Matches(markdown, pattern)
            .Select(m => m.Groups[1].Value)
            .Distinct();
    }

    /// <summary>
    /// マークダウン内の画像URLを置換する
    /// </summary>
    /// <param name="markdown"></param>
    /// <param name="downloadedImages"></param>
    /// <returns></returns>
    private static string ReplaceImageUrls(
        string markdown,
        IEnumerable<DownloadedImage> downloadedImages)
    {
        var result = markdown;
        foreach (var downloadedImage in downloadedImages)
        {
            result = result.Replace(downloadedImage.OriginalUrl, downloadedImage.FileName);
        }
        return result;
    }
}