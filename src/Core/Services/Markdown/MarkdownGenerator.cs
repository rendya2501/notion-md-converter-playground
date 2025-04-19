using NotionMarkdownConverter.Core.Models;
using NotionMarkdownConverter.Core.Services.Test;
using NotionMarkdownConverter.Infrastructure.Notion.Clients;
using System.Text;

namespace NotionMarkdownConverter.Core.Services.Markdown;


/// <summary>
/// マークダウン生成サービス
/// </summary>
/// <param name="_notionClient">Notionクライアント</param>
/// <param name="_frontmatterGenerator">フロントマター生成器</param>
/// <param name="_contentGenerator">コンテンツ生成器</param>
/// <param name="_markdownLinkProcessor">リンク処理サービス</param>
public class MarkdownGenerator(
    INotionClientWrapper _notionClient,
    IFrontmatterGenerator _frontmatterGenerator,
    IContentGenerator _contentGenerator,
    IDownloadLinkProcessor _markdownLinkProcessor) : IMarkdownGenerator
{
    /// <summary>
    /// マークダウンを生成します。
    /// </summary>
    /// <param name="pageProperty">ページのプロパティ</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>生成されたマークダウン</returns>
    public async Task<string> GenerateMarkdownAsync(PageProperty pageProperty, string outputDirectory)
    {
        // ページの全内容を取得(非同期で実行)
        var pageFullContent = _notionClient.GetPageFullContentAsync(pageProperty.PageId);

        // フロントマターを作成
        var frontmatter = _frontmatterGenerator.GenerateFrontmatter(pageProperty);

        // ページの全内容をマークダウンに変換
        var content = _contentGenerator.GenerateContent(await pageFullContent);

        // ファイルのダウンロードとリンクの変換処理 
        var processedContent = await _markdownLinkProcessor.ProcessLinkAsync(content, outputDirectory);

        // マークダウンを組み立てる
        var markdownBuilder = new StringBuilder();
        markdownBuilder.Append(frontmatter);
        markdownBuilder.Append(processedContent);
        return markdownBuilder.ToString();
    }
}
