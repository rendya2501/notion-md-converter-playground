using NotionMarkdownConverter.Core.Models;
using NotionMarkdownConverter.Core.Services.Test;
using NotionMarkdownConverter.Infrastructure.Notion.Clients;

namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// マークダウン生成サービス
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="_notionClient">Notionクライアント</param>
/// <param name="_frontmatterGenerator">フロントマター生成器</param>
/// <param name="_contentGenerator">コンテンツ生成器</param>
/// <param name="_imageProcessor">画像プロセッサ</param>
public class MarkdownGenerator(
    INotionClientWrapper _notionClient,
    IFrontmatterGenerator _frontmatterGenerator,
    IContentGenerator _contentGenerator,
    IMarkdownImageProcessor _imageProcessor,
    IFileProcessor _fileProcessor) : IMarkdownGenerator
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
        var pageFullContent = _notionClient.GetPageFullContent(pageProperty.PageId);

        // フロントマターを作成
        var frontmatter = _frontmatterGenerator.GenerateFrontmatter(pageProperty);

        // ページの全内容をマークダウンに変換
        var content = _contentGenerator.GenerateContentAsync(await pageFullContent);

        // 画像処理 
        var processedContent = await _imageProcessor.ProcessMarkdownImagesAsync(content, outputDirectory);

        var sss = await _fileProcessor.ProcessFileAsync(processedContent, outputDirectory);

        // マークダウンを出力
        return $"{frontmatter}{sss}";
    }
}

/*
using NotionMarkdownConverter.Core.Models;
using NotionMarkdownConverter.Infrastructure.Http.Services;
using System.Text;

namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// マークダウン生成サービス
/// </summary>
public class MarkdownGenerator : IMarkdownGenerator
{
    private readonly IImageDownloader _imageDownloader;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="imageDownloader">画像ダウンローダー</param>
    public MarkdownGenerator(IImageDownloader imageDownloader)
    {
        _imageDownloader = imageDownloader;
    }

    /// <summary>
    /// マークダウンを生成します。
    /// </summary>
    /// <param name="pageProperty">ページのプロパティ</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>生成されたマークダウン</returns>
    public async Task<string> GenerateMarkdownAsync(PageProperty pageProperty, string outputDirectory)
    {
        // マークダウン生成ロジック
        var markdown = new StringBuilder();

        // タイトル
        markdown.AppendLine($"# {pageProperty.Title}");
        markdown.AppendLine();

        // 公開日時
        if (pageProperty.PublishedDateTime.HasValue)
        {
            markdown.AppendLine($"公開日: {pageProperty.PublishedDateTime.Value:yyyy-MM-dd}");
            markdown.AppendLine();
        }

        // タグ
        if (pageProperty.Tags.Any())
        {
            markdown.AppendLine("タグ: " + string.Join(", ", pageProperty.Tags));
            markdown.AppendLine();
        }

        // 本文
        foreach (var block in pageProperty.Blocks)
        {
            markdown.AppendLine(block.ToMarkdown());
            markdown.AppendLine();
        }

        // マークダウンドキュメントを作成
        var document = new MarkdownDocument(markdown.ToString());

        // 画像URLを抽出
        var imageUrls = document.ExtractImageUrls();

        // 画像をダウンロード
        foreach (var imageUrl in imageUrls)
        {
            var downloadedImage = await _imageDownloader.DownloadAsync(imageUrl, imagesDirectory);
            document.AddImageReference(new ImageReference(
                downloadedImage.OriginalUrl,
                Path.Combine("images", downloadedImage.FileName)));
        }

        // 画像参照を置換
        return document.ReplaceImageReferences();
    }
} 

using System.Text.RegularExpressions;

namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// マークダウンドキュメント
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="content">マークダウンの内容</param>
public class MarkdownDocument(string content)
{
    private readonly Dictionary<string, string> _imageReferences = [];

    /// <summary>
    /// 画像参照を追加します。
    /// </summary>
    /// <param name="originalUrl">元のURL</param>
    /// <param name="localPath">ローカルのパス</param>
    public void AddImageReference(string originalUrl, string localPath)
    {
        _imageReferences[originalUrl] = localPath;
    }
    /// <summary>
    /// 画像参照を置換します。
    /// </summary>
    /// <returns>置換後のマークダウン</returns>
    public string ReplaceImageReferences()
    {
        var result = content;
        foreach (var (originalUrl, localPath) in _imageReferences)
        {
            result = result.Replace(originalUrl, localPath);
        }
        return result;
    }

    /// <summary>
    /// マークダウン内の画像URLを抽出します。
    /// </summary>
    /// <returns>画像URLのリスト</returns>
    public IEnumerable<string> ExtractImageUrls()
    {
        var pattern = @"!\[.*?\]\((.*?)\)";
        return Regex.Matches(content, pattern)
            .Select(m => m.Groups[1].Value)
            .Distinct();
    }
} 

namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// マークダウン内の画像参照
/// </summary>
/// <remarks>
/// コンストラクタ
/// </remarks>
/// <param name="originalUrl">元のURL</param>
/// <param name="localPath">ローカルのパス</param>
public class ImageReference(string originalUrl, string localPath)
{
    /// <summary>
    /// 元のURL
    /// </summary>
    public string OriginalUrl { get; } = originalUrl;

    /// <summary>
    /// ローカルのパス
    /// </summary>
    public string LocalPath { get; } = localPath;

    /// <summary>
    /// マークダウン内の画像参照を置換します。
    /// </summary>
    /// <param name="markdown">マークダウン</param>
    /// <returns>置換後のマークダウン</returns>
    public string ReplaceInMarkdown(string markdown)
    {
        return markdown.Replace(OriginalUrl, LocalPath);
    }
} 
*/