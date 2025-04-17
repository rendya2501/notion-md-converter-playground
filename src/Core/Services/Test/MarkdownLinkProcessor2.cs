using NotionMarkdownConverter.Infrastructure.Http.Services;
using System.Security.Cryptography;
using System.Text;

namespace NotionMarkdownConverter.Core.Services.Test;

/// <summary>
/// マークダウン内のリンクを処理するサービス
/// </summary>
public interface IMarkdownLinkProcessor2
{
    /// <summary>
    /// マークダウン内のリンクを処理します
    /// </summary>
    /// <param name="downloadLink">ダウンロードリンク</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>ダウンロードリンクから変換したファイル名</returns>
    Task<string> ProcessLinksAsync(string downloadLink, string outputDirectory);
}

/// <summary>
/// マークダウン内のリンクを処理するサービス
/// </summary>
public class MarkdownLinkProcessor2(IFileDownloader _imageDownloader) : IMarkdownLinkProcessor2
{
    /// <summary>
    /// マークダウン内のリンクを処理します
    /// </summary>
    /// <param name="markdown"></param>
    /// <param name="outputDirectory"></param>
    /// <returns></returns>
    public async Task<string> ProcessLinksAsync(string downloadLink, string outputDirectory)
    {
        // URLとファイル名のペアを生成
        var urlFilePair = new UrlFilePair(downloadLink, GenerateFileName(downloadLink));

        // 収集したURLをダウンロード
        await _imageDownloader.DownloadAsync(urlFilePair, outputDirectory);

        // ダウンロードリンクから変換したファイル名を返す
        return urlFilePair.ConversionFileName;
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
