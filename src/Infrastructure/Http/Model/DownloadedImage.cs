namespace NotionMarkdownConverter.Infrastructure.Http.Model;

/// <summary>
/// ダウンロードした画像の情報
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="OriginalUrl">元のURL</param>
/// <param name="FileName">ファイル名</param>
public record DownloadedImage(string OriginalUrl, string FileName);
