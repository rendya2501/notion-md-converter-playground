using NotionMarkdownConverter.Application.Interface;
using NotionMarkdownConverter.Core.Constants;
using NotionMarkdownConverter.Core.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// マークダウン内のダウンロードリンクを処理するサービス
/// </summary>
public class DownloadLinkProcessor(IHttpDownloader _imageDownloader) : IDownloadLinkProcessor
{
    /// <summary>
    /// マークダウン内のリンクを処理します
    /// </summary>
    /// <param name="markdown"></param>
    /// <param name="outputDirectory"></param>
    /// <returns>マークダウン</returns>
    public async Task<string> ProcessLinkAsync(string markdown, string outputDirectory)
    {
        // 1. マークダウン内のダウンロードリンクを抽出
        var urls = ExtractDownloadLinks(markdown);

        // 2. regexした結果からローカルリンクを生成しておく
        var urlToLocalLinkPairs = urls.ToDictionary(
            url => url,
            url => GenerateFileName(SanitizeUrl(url))
        );

        // 3. マークダウン文字列からregexした結果とローカルリンクのペアを置換する
        Task<string> replaceTask = Task.Run(() => ReplaceDownloadLinks(markdown, urlToLocalLinkPairs));

        // 4. サニタイジング文字列とローカルリンクのペアを元にダウンロードタスクを生成する
        IEnumerable<Task> downloadTasks = urlToLocalLinkPairs.Select(pair =>
        {
            var sanitizedUrl = SanitizeUrl(pair.Key);
            var localFileName = pair.Value;
            return _imageDownloader.DownloadAsync(new UrlFilePair(sanitizedUrl, localFileName), outputDirectory);
        });

        // 5. タスクをawaitする(3番と4番を並列実行)
        await Task.WhenAll(replaceTask, Task.WhenAll(downloadTasks));

        // 6. 結果を返す
        return await replaceTask;
    }


    /// <summary>
    /// ファイル名を生成します。
    /// </summary>
    /// <param name="uri">URI</param>
    /// <returns>ファイル名</returns>
    private static string GenerateFileName(string downloadLink)
    {
        // URIを生成
        var uri = new Uri(downloadLink);
        // URIのパス部分をUTF-8でエンコード
        var fileNameBytes = Encoding.UTF8.GetBytes(uri.LocalPath);

        // MD5ハッシュを生成し、16進数文字列に変換
        return $"{Convert.ToHexString(MD5.HashData(fileNameBytes))}{Path.GetExtension(uri.LocalPath)}";
    }

    /// <summary>
    /// マークダウン内のダウンロードリンクを抽出する
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    private static IEnumerable<string> ExtractDownloadLinks(string markdown)
    {
        // 特殊文字列を含むリンクを検出
        var pattern = @"!?\[.*?\]\(\s*(" + Regex.Escape(LinkConstants.DownloadMarker) + @".*?)\s*\)";
        // 正規表現でマッチする部分を取得
        var matches = Regex.Matches(markdown, pattern);

        // マッチしたURLを取得
        return matches.Select(m => m.Groups[1].Value).Distinct();
    }

    /// <summary>
    /// URLに含まれるDownloadMarkerをサニタイズします
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static string SanitizeUrl(string url)
    {
        return url.Replace(LinkConstants.DownloadMarker, string.Empty);
    }

    /// <summary>
    /// マークダウン内の画像URLを置換する
    /// </summary>
    /// <param name="markdown"></param>
    /// <param name="urlFilePairs"></param>
    /// <returns></returns>
    private static string ReplaceDownloadLinks(string markdown, Dictionary<string, string> urlToLocalLinkPairs)
    {
        var result = markdown;
        foreach (var pair in urlToLocalLinkPairs)
        {
            result = result.Replace(pair.Key, pair.Value);
        }
        return result;
    }
}
