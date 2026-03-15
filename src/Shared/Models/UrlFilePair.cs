namespace NotionMarkdownConverter.Shared.Models;

/// <summary>
/// ダウンロード対象のURLとローカル保存ファイル名のペア
/// </summary>
/// <param name="OriginalUrl">ダウンロード元のURL</param>
/// <param name="LocalFileName">ローカルに保存するファイル名</param>
public record UrlFilePair(string OriginalUrl, string LocalFileName);
