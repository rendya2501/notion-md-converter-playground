using NotionMarkdownConverter.Extract;
using NotionMarkdownConverter.Infrastructure.Notion;
using NotionMarkdownConverter.Pipeline;
using NotionMarkdownConverter.Pipeline.Models;
using NotionMarkdownConverter.Transform;

namespace NotionMarkdownConverter.Tests.Integration;

/// <summary>
/// Notion APIを実際に叩く統合テスト。
///
/// 実行前に以下のUser Secretsを設定してください：
///   dotnet user-secrets set "Notion:AuthToken"  "secret_xxx"
///   dotnet user-secrets set "Notion:DatabaseId" "xxx"
///
/// テスト結果（Markdownとダウンロードファイル）は tests/Integration/articles/ 以下に出力されます。
/// このディレクトリは .gitignore で追跡対象から除外されています。
/// </summary>
public class NotionExportIntegrationTests : IntegrationTestBase
{
    // ── 出力先設定 ────────────────────────────────────────────────────────────
    //
    // ユーザーシークレットの代わりにここで articles ベースパスを直書きできます。
    // null のままなら tests/Integration/articles/ (テストプロジェクト相対) を使用します。
    //
    // 例（Windowsの場合）:
    //   private const string? OutputDirectoryOverride =
    //       @"C:\Users\rendya\projects\NotionMarkdownConverter\tests\Integration\articles";
    //
    private const string? OutputDirectoryOverride = null;

    /// <summary>
    /// テスト出力の articles ベースディレクトリを返します。
    /// <see cref="OutputDirectoryOverride"/> が設定されていればそちらを優先します。
    /// </summary>
    private static string ArticlesBaseDirectory =>
        OutputDirectoryOverride ?? GetArticlesBaseDirectory();

    // ── テスト ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Notion APIからページ一覧を取得できることを確認する。
    /// 最もシンプルな統合テスト。APIキーとDBIDが正しければ通る。
    /// </summary>
    [Fact]
    public async Task GetPagesForPublishing_ReturnsPages()
    {
        // Arrange
        var client = GetService<INotionClientWrapper>();

        // Act
        var pages = await client.GetPagesForPublishingAsync(Secrets.NotionDatabaseId);

        // Assert（0件でも取得処理は成功）
        Assert.NotNull(pages);
        foreach (var page in pages)
            Console.WriteLine($"Page: {page.Id}");
    }

    /// <summary>
    /// MarkdownAssemblerが実際のNotionページからMarkdownを組み立てられることを確認する。
    /// 出力は articles/YYYY/MM/スラッグ/ に書き出される。
    /// ページが0件の場合はスキップ。
    /// </summary>
    [Fact]
    public async Task TransformAsync_FromRealNotionPage_ProducesMarkdown()
    {
        // Arrange
        var client = GetService<INotionClientWrapper>();
        var transformer = GetService<NotionPageTransformer>();
        var pagePropertyMapper = GetService<IPagePropertyMapper>();

        var pages = await client.GetPagesForPublishingAsync(Secrets.NotionDatabaseId);

        if (pages.Count == 0)
            Assert.Skip("テスト用DBにページが存在しないためスキップします。");

        var firstPage = pages[0];
        var pageProperty = pagePropertyMapper.Map(firstPage);
        var blocks = await client.FetchBlockTreeAsync(pageProperty.PageId);

        var extractedPage = new ExtractedPage(pageProperty, blocks);

        // Act
        var result = await transformer.TransformAsync(extractedPage);

        // Assert
        Assert.NotNull(result.Markdown);
        Assert.NotEmpty(result.Markdown);
        Assert.Contains("---", result.Markdown);

        var outputPath = Path.Combine(result.OutputDirectory, "index.md");
        await File.WriteAllTextAsync(outputPath, result.Markdown,
            new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            TestContext.Current.CancellationToken);

        Console.WriteLine($"Output written to: {outputPath}");
        Console.WriteLine("--- Markdown preview (first 500 chars) ---");
        Console.WriteLine(result.Markdown[..Math.Min(500, result.Markdown.Length)]);
    }

    /// <summary>
    /// エクスポート全体フローを実行する。
    /// NotionExporter.ExportPagesAsync() を直接呼ぶE2Eに近いテスト。
    ///
    /// 注意：このテストはNotionのページプロパティを実際に更新します。
    ///       テスト用DBを使ってください。
    ///
    /// 出力は articles/YYYY/MM/スラッグ/ 以下に書き出される
    /// （IntegrationTestBase.BuildOutputDirectoryPathTemplate のテンプレート設定による）。
    /// </summary>
    [Fact]
    public async Task ExportPagesAsync_FullFlow_CreatesMarkdownFiles()
    {
        // Arrange
        var pipeline = GetService<NotionExportPipeline>();

        // Act
        await pipeline.RunAsync();

        // Assert: articles/ 以下に少なくとも1つの index.md が生成されていること
        var files = Directory.GetFiles(ArticlesBaseDirectory, "index.md", SearchOption.AllDirectories);
        Assert.NotEmpty(files);

        foreach (var file in files)
            Console.WriteLine($"Generated: {file}");
    }
}
