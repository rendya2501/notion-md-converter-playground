namespace hoge.Services;

/// <summary>
/// マークダウンの画像を処理するインターフェース
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// マークダウン内の画像を処理する
    /// </summary>
    /// <param name="markdown">マークダウン</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>マークダウン</returns>
    Task<string> ProcessMarkdownImagesAsync(string markdown, string outputDirectory);
} 