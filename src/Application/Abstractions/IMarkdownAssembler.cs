using NotionMarkdownConverter.Domain.Models;

namespace NotionMarkdownConverter.Application.Abstractions;

/// <summary>
/// ページプロパティとブロックコンテンツからMarkdown文字列を組み立てる抽象。
/// </summary>
public interface IMarkdownAssembler
{
    /// <summary>
    /// 指定したページのMarkdown文字列を非同期で組み立てます。
    /// </summary>
    /// <param name="pageProperty">フロントマターの生成に使用するページプロパティ</param>
    /// <param name="outputDirectory">画像などの添付ファイルを保存する出力ディレクトリパス</param>
    /// <returns>組み立て済みのMarkdown文字列</returns>
    Task<string> AssembleAsync(PageProperty pageProperty, string outputDirectory);
}
