using NotionMarkdownConverter.Infrastructure.Http.Services;
using System.Security.Cryptography;
using System.Text;

namespace NotionMarkdownConverter.Core.Services.Test;

/// <summary>
/// マークダウン内のリンクを処理するサービス
/// </summary>
public interface IMarkdownLinkProcessor_
{
    /// <summary>
    /// マークダウン内のリンクを処理します
    /// </summary>
    /// <param name="markdown">マークダウン</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>処理後のマークダウン</returns>
    Task<string> ProcessLinksAsync(string markdown, string outputDirectory);
}

/// <summary>
/// マークダウン内のリンクを処理するサービス
/// </summary>
public class MarkdownLinkProcessor_(
    IFileDownloader _imageDownloader,
    DownloadLinkCollector _collector) : IMarkdownLinkProcessor_
{

    /// <summary>
    /// マークダウン内のリンクを処理します
    /// </summary>
    /// <param name="markdown"></param>
    /// <param name="outputDirectory"></param>
    /// <returns></returns>
    public async Task<string> ProcessLinksAsync(string markdown, string outputDirectory)
    {
        // マークダウン内の画像URLを収集
        var urls = _collector.GetCollectedUrls();

        // 収集したURLがない場合はそのまま返す
        if (urls.Count == 0)
        {
            return markdown;
        }

        // URLとファイル名のペアを生成
        var urlFilePairs = urls.Select(url => new UrlFilePair(url, GenerateFileName(url)));

        // 収集したURLをダウンロード
        var downloadTasks = urlFilePairs.Select(async urlFilePair =>
            await _imageDownloader.DownloadAsync(urlFilePair, outputDirectory));
        // ダウンロードが完了するまで待機
        await Task.WhenAll(downloadTasks);

        // ダウンロードしたファイルのURLを置換
        var replacedMarkdown = ReplaceUrls(markdown, urlFilePairs);

        // コレクターがstaticなので、全ての処理が完了した後にクリアする
        _collector.ClearCollectedUrls();

        return replacedMarkdown;
    }

    /// <summary>
    /// マークダウン内の画像URLを置換する
    /// </summary>
    /// <param name="markdown"></param>
    /// <param name="downloadedFiles"></param>
    /// <returns></returns>
    private static string ReplaceUrls(
        string markdown,
        IEnumerable<UrlFilePair> downloadedFiles)
    {
        var result = markdown;
        foreach (var file in downloadedFiles)
        {
            result = result.Replace(file.OriginalUrl, file.ConversionFileName);
        }
        return result;
    }

    /// <summary>
    /// ファイル名を生成します。
    /// </summary>
    /// <param name="uri">URI</param>
    /// <returns>ファイル名</returns>
    private static string GenerateFileName(string downloadLink)
    {
        var uri = new Uri(downloadLink);
        var fileNameBytes = Encoding.UTF8.GetBytes(uri.LocalPath);
        return $"{Convert.ToHexString(MD5.HashData(fileNameBytes))}{Path.GetExtension(uri.LocalPath)}";
    }
}
