using NotionMarkdownConverter.Infrastructure;
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
    IOutputDirectoryProvider _outputDirectoryProvider)
{
    public async Task<TransformedPage> TransformAsync(ExtractedPage extractedPage)
    {
        var pageProperty = extractedPage.PageProperty;

        // 出力ディレクトリを構築（リンク処理のダウンロード先として必要）
        var outputDirectory = _outputDirectoryProvider.BuildAndCreate(pageProperty);

        // フロントマターを生成
        var frontmatter = _frontmatterConverter.Convert(pageProperty);

        // ブロックをMarkdownに変換（ブロックはExtract済み）
        var content = _contentConverter.Convert(extractedPage.Blocks);

        // ダウンロードリンクをローカルパスに置換
        var processedContent = await _markdownLinkProcessor.ProcessLinksAsync(content, outputDirectory);

        return new TransformedPage(pageProperty, frontmatter + processedContent, outputDirectory);
    }
}
