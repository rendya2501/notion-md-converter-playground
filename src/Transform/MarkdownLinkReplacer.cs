using NotionMarkdownConverter.Shared.Constants;
using NotionMarkdownConverter.Shared.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace NotionMarkdownConverter.Transform;

/// <summary>
/// Markdown内のダウンロードリンクをローカルファイル名に置換します。
/// ファイルのダウンロードは行いません。
/// </summary>
public class MarkdownLinkReplacer
{
    /// <summary>
    /// Markdown内のダウンロードリンクをローカルファイル名に置換し、
    /// 置換後のMarkdownとダウンロード対象URLペアの一覧を返します。
    /// </summary>
    /// <param name="markdown">処理対象のMarkdown文字列</param>
    /// <returns>置換後のMarkdownとダウンロード対象URLペアの一覧</returns>
    public (string ReplacedMarkdown, IEnumerable<UrlFilePair> UrlFilePairs) Replace(string markdown)
    {
        var urls = ExtractDownloadLinks(markdown);

        var urlToLocalFilePairs = urls.ToDictionary(
            url => url,
            url => GenerateFileName(SanitizeUrl(url))
        );

        var replaced = ReplaceDownloadLinks(markdown, urlToLocalFilePairs);

        var pairs = urlToLocalFilePairs.Select(p =>
            new UrlFilePair(SanitizeUrl(p.Key), p.Value));

        return (replaced, pairs);
    }

    /// <summary>
    /// ダウンロードURLからローカル保存用のファイル名を生成します。
    /// </summary>
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
    private static IEnumerable<string> ExtractDownloadLinks(string markdown)
    {
        var pattern = @"!?\[.*?\]\(\s*(" + Regex.Escape(MarkdownConstants.DownloadMarker) + @".*?)\s*\)";
        return Regex.Matches(markdown, pattern)
            .Select(m => m.Groups[1].Value)
            .Distinct();
    }

    /// <summary>
    /// URLに含まれる <see cref="MarkdownConstants.DownloadMarker"/> を除去します。
    /// </summary>
    private static string SanitizeUrl(string url)
        => url.Replace(MarkdownConstants.DownloadMarker, string.Empty);

    /// <summary>
    /// Markdown内のダウンロードリンクをローカルファイル名に一括置換します。
    /// </summary>
    private static string ReplaceDownloadLinks(string markdown, Dictionary<string, string> pairs)
    {
        var result = markdown;
        foreach (var pair in pairs)
            result = result.Replace(pair.Key, pair.Value);
        return result;
    }
}
