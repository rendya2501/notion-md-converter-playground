namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// マークダウン内の画像を処理するサービス
/// </summary>
public interface IMarkdownImageProcessor
{
    /// <summary>
    /// マークダウン内の画像を処理します。
    /// </summary>
    /// <param name="markdown">マークダウン</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>処理後のマークダウン</returns>
    Task<string> ProcessMarkdownImagesAsync(string markdown, string outputDirectory);
} 