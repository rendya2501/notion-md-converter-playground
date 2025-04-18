using NotionMarkdownConverter.Core.Constants;
using NotionMarkdownConverter.Infrastructure.Http.Services;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NotionMarkdownConverter.Core.Services.Test;

/// <summary>
/// マークダウン内のダウンロードリンクを処理するサービス
/// </summary>
public interface IDownloadLinkProcessor
{
    /// <summary>
    /// マークダウン内のダウンロードリンクを処理します
    /// </summary>
    /// <param name="markdown">マークダウン</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>処理後のマークダウン</returns>
    Task<string> ProcessLinkAsync(string markdown, string outputDirectory);
}


/// <summary>
/// マークダウン内のダウンロードリンクを処理するサービス
/// </summary>
public class DownloadLinkProcessor(IFileDownloader _imageDownloader) : IDownloadLinkProcessor
{
    /// <summary>
    /// マークダウン内のリンクを処理します
    /// </summary>
    /// <param name="markdown"></param>
    /// <param name="outputDirectory"></param>
    /// <returns>マークダウン</returns>
    public async Task<string> ProcessLinkAsync(string markdown, string outputDirectory)
    {
        //// マークダウン内のダウンロードリンクを抽出
        //var urls = ExtractDownloadLinks(markdown);

        //// URLとファイル名のペアを生成
        //var urlFilePairs = urls.Select(url =>
        //{
        //    var sanitizedUrl = SanitizeUrl(url);
        //    return new UrlFilePair(sanitizedUrl, GenerateFileName(sanitizedUrl));
        //});

        //// ダウンロードタスクを生成
        //var downloadTasks = urlFilePairs.Select(pair =>
        //    _imageDownloader.DownloadAsync(pair, outputDirectory));

        //// ダウンロードが完了するまで待機
        //await Task.WhenAll(downloadTasks);

        //// マークダウンのダウンロードリンクを置換
        //return ReplaceUrls(markdown, urlFilePairs);




        //// 1. マークダウン文字列から固定文字列をregexする
        //var urls = ExtractDownloadLinks(markdown);

        //// 2. regexした結果からローカルリンクを生成しておく
        //var urlToLocalLinkPairs = urls.ToDictionary(
        //    url => url,
        //    url => GenerateFileName(SanitizeUrl(url))
        //);

        //// 3. マークダウン文字列からregexした結果とローカルリンクのペアを置換する
        //var updatedMarkdown = ReplaceUrls(markdown, urlToLocalLinkPairs);

        //// 4. サニタイジング文字列とローカルリンクのペアを元にダウンロードタスクを生成する
        //var downloadTasks = urlToLocalLinkPairs.Select(pair =>
        //{
        //    var sanitizedUrl = SanitizeUrl(pair.Key);
        //    var localFileName = pair.Value;
        //    return _imageDownloader.DownloadAsync(new UrlFilePair(sanitizedUrl, localFileName), outputDirectory);
        //});

        //// 5. タスクをawaitする
        //await Task.WhenAll(downloadTasks);

        //// 6. 結果を返す
        //return updatedMarkdown;




        // 1. マークダウン内のダウンロードリンクを抽出
        var urls = ExtractDownloadLinks(markdown);

        // 2. regexした結果からローカルリンクを生成しておく
        var urlToLocalLinkPairs = urls.ToDictionary(
            url => url,
            url => GenerateFileName(SanitizeUrl(url))
        );

        // 3. マークダウン文字列からregexした結果とローカルリンクのペアを置換する
        Task<string> replaceTask = Task.Run(() => ReplaceUrls(markdown, urlToLocalLinkPairs));

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
    private static string ReplaceUrls(string markdown, Dictionary<string, string> urlToLocalLinkPairs)
    {
        var result = markdown;
        foreach (var pair in urlToLocalLinkPairs)
        {
            result = result.Replace(pair.Key, pair.Value);
        }
        return result;
    }
    //private static string ReplaceUrls(string markdown, IEnumerable<UrlFilePair> urlFilePairs)
    //{
    //    var result = markdown;
    //    foreach (var file in urlFilePairs)
    //    {
    //        result = result.Replace(LinkConstants.DownloadMarker + file.OriginalUrl, file.ConversionFileName);
    //    }
    //    return result;
    //}
}
