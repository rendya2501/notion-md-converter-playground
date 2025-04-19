using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Infrastructure.Http.Services;

/// <summary>
/// ファイルダウンローダー
/// </summary>
public interface IFileDownloader
{
    /// <summary>
    /// 出力ディレクトリへファイルをダウンロードします。
    /// </summary>
    /// <param name="urlFilePair">URLとファイル名のペア</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>ダウンロードした情報</returns>
    Task DownloadAsync(UrlFilePair urlFilePair, string outputDirectory);
}
