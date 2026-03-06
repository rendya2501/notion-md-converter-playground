using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Domain.Constants;
using NotionMarkdownConverter.Domain.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NotionMarkdownConverter.Application.Services;

/// <summary>
/// Markdown内のダウンロードリンクを処理するサービス
/// </summary>
public class MarkdownLinkProcessor(IFileDownloader _fileDownloader)
{
    /// <summary>
    /// Markdown内のダウンロードリンクをローカルファイルパスに置換し、ファイルをダウンロードします。
    /// </summary>
    /// <param name="markdown">処理対象のMarkdown文字列</param>
    /// <param name="outputDirectory">ファイルのダウンロード先ディレクトリ</param>
    /// <returns>ダウンロードリンクをローカルパスに置換したMarkdown文字列</returns>
    public async Task<string> ProcessLinksAsync(string markdown, string outputDirectory)
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
            return _fileDownloader.DownloadAsync(new UrlFilePair(sanitizedUrl, localFileName), outputDirectory);
        });

        // 5. タスクをawaitする(3番と4番を並列実行)
        await Task.WhenAll(replaceTask, Task.WhenAll(downloadTasks));

        // 6. 結果を返す
        return await replaceTask;
    }

    /// <summary>
    /// ダウンロードURLからローカル保存用のファイル名を生成します。
    /// </summary>
    /// <param name="downloadLink">サニタイズ済みのダウンロードURL</param>
    /// <returns>MD5ハッシュと元の拡張子を組み合わせたファイル名</returns>
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
    /// Markdown内からダウンロード対象のリンクURLを抽出します。
    /// </summary>
    /// <param name="markdown">抽出対象のMarkdown文字列</param>
    /// <returns>重複を除いたダウンロードリンクのURL一覧</returns>
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
    /// URLに含まれる <see cref="LinkConstants.DownloadMarker"/> を除去します。
    /// </summary>
    /// <param name="url">マーカーを含むURL</param>
    /// <returns>マーカーを除去したURL</returns>
    private static string SanitizeUrl(string url)
    {
        return url.Replace(LinkConstants.DownloadMarker, string.Empty);
    }

    /// <summary>
    /// Markdown内のダウンロードリンクをローカルファイル名に一括置換します。
    /// </summary>
    /// <param name="markdown">置換対象のMarkdown文字列</param>
    /// <param name="urlToLocalLinkPairs">ダウンロードURLとローカルファイル名のペア辞書</param>
    /// <returns>置換後のMarkdown文字列</returns>
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
