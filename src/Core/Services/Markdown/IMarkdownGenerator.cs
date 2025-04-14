using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// マークダウン生成サービス
/// </summary>
public interface IMarkdownGenerator
{
    /// <summary>
    /// マークダウンを生成します。
    /// </summary>
    /// <param name="pageProperty">ページのプロパティ</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>生成されたマークダウン</returns>
    Task<string> GenerateMarkdownAsync(PageProperty pageProperty, string outputDirectory);
} 