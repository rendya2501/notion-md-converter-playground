using NotionMarkdownConverter.Infrastructure.FileSystem;
using NotionMarkdownConverter.Pipeline.Models;
using NotionMarkdownConverter.Transform.Converters;

namespace NotionMarkdownConverter.Transform;

/// <summary>
/// Transformステージ。
/// ExtractedPageを受け取り、Markdown文字列と出力ディレクトリを組み立てます。
/// ブロック取得はExtractステージ済みのため、このクラスはNotionAPIに依存しません。
/// </summary>
public class NotionPageTransformer(
    FrontmatterConverter _frontmatterConverter,
    ContentConverter _contentConverter,
    IMarkdownLinkProcessor _markdownLinkProcessor,
    OutputPathBuilder _outputPathBuilder,
    IFileSystem _fileSystem)
{
    /// <summary>
    /// ExtractedPageをMarkdown文字列と出力ディレクトリに変換します。
    /// </summary>
    /// <param name="extractedPage">Extractステージの出力</param>
    /// <returns>Markdown文字列と出力先ディレクトリを含むTransformedPage</returns>
    public async Task<TransformedPage> TransformAsync(ExtractedPage extractedPage)
    {
        var pageProperty = extractedPage.PageProperty;

        // 出力ディレクトリを構築（リンク処理のダウンロード先として必要）
        var outputDirectory = _outputPathBuilder.Build(pageProperty);

        // ディレクトリを作成（既存の場合は何もしない）
        _fileSystem.CreateDirectory(outputDirectory);

        // フロントマターを生成
        var frontmatter = _frontmatterConverter.Convert(pageProperty);

        // ブロックをMarkdownに変換（ブロックはExtract済み）
        var content = _contentConverter.Convert(extractedPage.Blocks);

        // ダウンロードリンクをローカルパスに置換
        var processedContent = await _markdownLinkProcessor.ProcessLinksAsync(content, outputDirectory);

        return new TransformedPage(pageProperty, frontmatter + processedContent, outputDirectory);
    }
}
