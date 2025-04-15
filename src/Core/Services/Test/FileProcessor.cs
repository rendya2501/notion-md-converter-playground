using NotionMarkdownConverter.Infrastructure.Http.Model;
using NotionMarkdownConverter.Infrastructure.Http.Services;
using System.Text.RegularExpressions;

namespace NotionMarkdownConverter.Core.Services.Test;

/// <summary>
/// マークダウン内の画像を処理するサービス
/// </summary>
public interface IFileProcessor
{
    /// <summary>
    /// マークダウン内の画像を処理します。
    /// </summary>
    /// <param name="markdown">マークダウン</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>処理後のマークダウン</returns>
    Task<string> ProcessFileAsync(string markdown, string outputDirectory);
}

/// <summary>
/// マークダウン内の画像を処理するサービス
/// </summary>
public class FileProcessor(IImageDownloader _imageDownloader) : IFileProcessor
{
    /// <summary>
    /// マークダウン内の画像を処理する
    /// </summary>
    /// <param name="markdown"></param>
    /// <param name="outputDirectory"></param>
    /// <returns>マークダウン</returns>
    public async Task<string> ProcessFileAsync(string markdown, string outputDirectory)
    {
        var imageUrls = ExtractFileUrls(markdown);

        var downloadTasks = imageUrls.Select(async url =>
            await _imageDownloader.DownloadAsync(url, outputDirectory));
        var downloadedImages = await Task.WhenAll(downloadTasks);

        return ReplaceImageUrls(markdown, downloadedImages);
    }

    /// <summary>
    /// マークダウン内の.txtおよび.mdファイルのURLを抽出する
    /// </summary>
    /// <param name="markdown">マークダウン</param>
    /// <returns>抽出されたURLのリスト</returns>
    private static IEnumerable<string> ExtractFileUrls(string markdown)
    {
        // 通常リンク: [text](url.txt) または [text](url.md)
        var pattern = @"\[.*?\]\((.*?\.(?:txt|md)(?:\?[^\)]*)?)\)";
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
