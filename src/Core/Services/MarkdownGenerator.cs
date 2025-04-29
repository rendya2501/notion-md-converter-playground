using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Core.Services;

/// <summary>
/// マークダウン生成サービス
/// </summary>
/// <param name="_frontmatterGenerator">フロントマター生成器</param>
/// <param name="_contentGenerator">コンテンツ生成器</param>
public class MarkdownGenerator(
    FrontmatterGenerator _frontmatterGenerator,
    ContentGenerator _contentGenerator)
{
    /// <summary>
    /// マークダウンを生成します。
    /// </summary>
    /// <param name="pageProperty">ページのプロパティ</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>生成されたマークダウン</returns>
    public string GenerateMarkdown(List<NotionBlock> pageContent, PageProperty pageProperty)
    {
        // フロントマターを生成
        var frontmatter = _frontmatterGenerator.GenerateFrontmatter(pageProperty);
        // コンテンツを生成
        var content = _contentGenerator.GenerateContent(pageContent);
        // マークダウンを組み立てる
        return $"{frontmatter}{content}";
    }
}
