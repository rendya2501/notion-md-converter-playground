namespace NotionMarkdownConverter.Application.Abstractions;

/// <summary>
/// Markdown内のダウンロードリンクを処理するサービスのインターフェース
/// </summary>
public interface IMarkdownLinkProcessor
{
    /// <summary>
    /// Markdown内のダウンロードリンクをローカルファイルパスに置換し、ファイルをダウンロードします。
    /// </summary>
    /// <param name="markdown">処理対象のMarkdown文字列</param>
    /// <param name="outputDirectory">ファイルのダウンロード先ディレクトリ</param>
    /// <returns>ダウンロードリンクをローカルパスに置換したMarkdown文字列</returns>
    Task<string> ProcessLinksAsync(string markdown, string outputDirectory);
}
