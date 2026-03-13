using NotionMarkdownConverter.Infrastructure;
using NotionMarkdownConverter.Shared.Constants;
using NotionMarkdownConverter.Shared.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NotionMarkdownConverter.Transform;

/// <summary>
/// Markdown内のダウンロードリンクを処理するサービス
/// </summary>
public class MarkdownLinkProcessor(IFileDownloader _fileDownloader) : IMarkdownLinkProcessor
{
    /// <summary>
    /// Markdown内のダウンロードリンクをローカルファイルパスに置換し、ファイルをダウンロードします。
    /// </summary>
    /// <param name="markdown">処理対象のMarkdown文字列</param>
    /// <param name="outputDirectory">ファイルのダウンロード先ディレクトリ</param>
    /// <returns>ダウンロードリンクをローカルパスに置換したMarkdown文字列</returns>
    public async Task<string> ProcessLinksAsync(string markdown, string outputDirectory)
    {
        // 1. マークダウン内のダウンロード対象URLを抽出
        var urls = ExtractDownloadLinks(markdown);

        // 2. 抽出したURLごとに、マーカーを除去したURLとローカルファイル名のペアを生成
        var urlToLocalFilePairs = urls.ToDictionary(
            url => url,
            url => GenerateFileName(SanitizeUrl(url))
        );

        // 3. マークダウン内のダウンロードURL（マーカー付き）をローカルファイル名に置換するタスクを開始
        Task<string> replaceTask = Task.Run(() => ReplaceDownloadLinks(markdown, urlToLocalFilePairs));

        // 4. 各URLのダウンロードタスクを生成（マーカー除去後のURLを使用）
        IEnumerable<Task> downloadTasks = urlToLocalFilePairs.Select(pair =>
        {
            var sanitizedUrl = SanitizeUrl(pair.Key);
            var localFileName = pair.Value;
            return _fileDownloader.DownloadAsync(new UrlFilePair(sanitizedUrl, localFileName), outputDirectory);
        });

        // 5. 置換タスクとダウンロードタスクを並列実行
        await Task.WhenAll(replaceTask, Task.WhenAll(downloadTasks));

        // 6. 置換済みのMarkdownを返す
        return await replaceTask;
    }

    /// <summary>
    /// ダウンロードURLからローカル保存用のファイル名を生成します。
    /// </summary>
    /// <param name="downloadLink">マーカー除去済みのダウンロードURL</param>
    /// <returns>MD5ハッシュと元の拡張子を組み合わせたファイル名</returns>
    private static string GenerateFileName(string downloadLink)
    {
        // URIのパス部分（クエリ文字列除外）をもとにMD5ハッシュを生成してファイル名とする
        var uri = new Uri(downloadLink);
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
        // ダウンロードマーカーを含むリンクを正規表現で検出
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
    /// <param name="urlToLocalFilePairs">マーカー付きURLとローカルファイル名のペア辞書</param>
    /// <returns>置換後のMarkdown文字列</returns>
    private static string ReplaceDownloadLinks(string markdown, Dictionary<string, string> urlToLocalFilePairs)
    {
        var result = markdown;
        foreach (var pair in urlToLocalFilePairs)
        {
            result = result.Replace(pair.Key, pair.Value);
        }
        return result;
    }
}
