using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Domain.Markdown.Converters;
using NotionMarkdownConverter.Domain.Models;
using System.Text;

namespace NotionMarkdownConverter.Application.Services;

/// <summary>
/// NotionページのコンテンツからMarkdown文字列を組み立てるサービス
/// </summary>
/// <param name="_notionClient">Notionページの全内容を取得するクライアント</param>
/// <param name="_frontmatterConverter">ページプロパティをフロントマターに変換するコンバーター</param>
/// <param name="_contentConverter">ブロック列をMarkdown本文に変換するコンバーター</param>
/// <param name="_markdownLinkProcessor">Markdown内のダウンロードリンクを処理するプロセッサー</param>
public class MarkdownAssembler(
    INotionClientWrapper _notionClient,
    FrontmatterConverter _frontmatterConverter,
    ContentConverter _contentConverter,
    MarkdownLinkProcessor _markdownLinkProcessor)
{
    /// <summary>
    /// 指定されたページプロパティを元に、フロントマターと本文を組み立てたMarkdown文字列を生成します。
    /// </summary>
    /// <param name="pageProperty">組み立て対象のページプロパティ</param>
    /// <param name="outputDirectory">ダウンロードファイルの出力先ディレクトリ</param>
    /// <returns>フロントマターと本文を含むMarkdown文字列</returns>
    public async Task<string> AssembleAsync(PageProperty pageProperty, string outputDirectory)
    {
        // ページの全内容を取得(非同期で実行)
        var pageFullContent = _notionClient.FetchBlockTreeAsync(pageProperty.PageId);

        // フロントマターを作成
        var frontmatter = _frontmatterConverter.Convert(pageProperty);

        // ページの全内容をマークダウンに変換
        var content = _contentConverter.Convert(await pageFullContent);

        // ファイルのダウンロードとリンクの変換処理 
        var processedContent = await _markdownLinkProcessor.ProcessLinksAsync(content, outputDirectory);

        // マークダウンを組み立てる
        var markdownBuilder = new StringBuilder();
        markdownBuilder.Append(frontmatter);
        markdownBuilder.Append(processedContent);
        return markdownBuilder.ToString();
    }
}
