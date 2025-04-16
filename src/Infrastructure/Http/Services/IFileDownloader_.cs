using NotionMarkdownConverter.Core.Services.Test;

namespace NotionMarkdownConverter.Infrastructure.Http.Services;

/// <summary>
/// ファイルダウンローダー
/// </summary>
public interface IFileDownloader_
{
    /// <summary>
    /// 出力ディレクトリへファイルをダウンロードします。
    /// </summary>
    /// <param name="downloadLink">ダウンロードリンク</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>ダウンロードした情報</returns>
    Task<UrlFilePair> DownloadAsync(string downloadLink, string outputDirectory);
}
