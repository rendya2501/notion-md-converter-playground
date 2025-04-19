namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// マークダウン内のダウンロードリンクを処理するサービス
/// </summary>
public interface IDownloadLinkProcessor
{
    /// <summary>
    /// マークダウン内のダウンロードリンクを処理します
    /// </summary>
    /// <param name="markdown">マークダウン</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>処理後のマークダウン</returns>
    Task<string> ProcessLinkAsync(string markdown, string outputDirectory);
}
