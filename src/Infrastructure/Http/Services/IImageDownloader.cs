namespace NotionMarkdownConverter.Infrastructure.Http.Services;

/// <summary>
/// 画像ダウンローダー
/// </summary>
public interface IImageDownloader
{
    /// <summary>
    /// 画像をダウンロードします。
    /// </summary>
    /// <param name="url">画像URL</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>ダウンロードした画像の情報</returns>
    Task<DownloadedImage> DownloadAsync(string url, string outputDirectory);
} 