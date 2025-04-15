namespace NotionMarkdownConverter.Infrastructure.Http.Services;

/// <summary>
/// ファイルダウンローダー
/// </summary>
public interface IFileDownloader
{
    /// <summary>
    /// 出力ディレクトリへファイルをダウンロードします。
    /// </summary>
    /// <param name="downloadLink">ダウンロードリンク</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>ダウンロードしたファイル名</returns>
    Task<string> DownloadAsync(string downloadLink, string outputDirectory);
}
