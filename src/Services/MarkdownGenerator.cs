using hoge.Models;

namespace hoge.Services;

/// <summary>
/// マークダウンを生成するクラスです
/// </summary>
/// <param name="_notionClient"></param>
/// <param name="_frontmatterGenerator"></param>
/// <param name="_contentGenerator"></param>
public class MarkdownGenerator(
    INotionClientWrapper _notionClient,
    IFrontmatterGenerator _frontmatterGenerator,
    IContentGenerator _contentGenerator) : IMarkdownGenerator
{
    /// <summary>
    /// マークダウンを生成します。
    /// </summary>
    /// <param name="pageProperty"></param>
    /// <returns></returns>
    public async Task<string> GenerateMarkdownAsync(PageProperty pageProperty)
    {
        // ページの全内容を取得(非同期で実行)
        var pageFullContent = _notionClient.GetPageFullContent(pageProperty.PageId);

        // フロントマターを作成
        var frontmatter = _frontmatterGenerator.GenerateFrontmatter(pageProperty);

        // ページの全内容をマークダウンに変換
        var content = _contentGenerator.GenerateContentAsync(await pageFullContent);

        // マークダウンを出力
        return $"{frontmatter}{content}";
    }
}
