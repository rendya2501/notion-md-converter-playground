namespace NotionMarkdownConverter.Core.Models;

/// <summary>
/// URLとファイル名のペア
/// </summary>
/// <param name="OriginalUrl">元のURL</param>
/// <param name="ConversionFileName">変換したファイル名</param>
public record UrlFilePair(string OriginalUrl, string ConversionFileName);
