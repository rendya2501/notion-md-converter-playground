using NotionMarkdownConverter.Domain.Models;

namespace NotionMarkdownConverter.Pipeline.Models;

/// <summary>
/// Transformステージの出力。
/// 組み立て済みのMarkdown文字列と出力先ディレクトリを保持します。
/// </summary>
public record TransformedPage(
    PageProperty PageProperty,
    string Markdown,
    string OutputDirectory
);
