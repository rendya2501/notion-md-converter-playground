using NotionMarkdownConverter.Application.Abstractions;

namespace NotionMarkdownConverter.Tests.Integration;

/// <summary>
/// Notion APIを実際に叩く統合テスト。
///
/// 実行前に以下のUser Secretsを設定してください：
///   dotnet user-secrets set "Notion:AuthToken" "secret_xxx"
///   dotnet user-secrets set "Notion:DatabaseId" "xxx"
///   dotnet user-secrets set "Test:OutputDirectory" "C:/tmp/notion-test"
/// </summary>
public class NotionExportIntegrationTests : IntegrationTestBase
{
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
        {
            Console.WriteLine($"Page: {page.Id}");
        }
    }

    /// <summary>
    /// MarkdownAssemblerが実際のNotionページからMarkdownを組み立てられることを確認する。
    /// ページが0件の場合はスキップ。
    /// </summary>
    [Fact]
    public async Task AssembleAsync_FromRealNotionPage_ProducesMarkdown()
    {
        // Arrange
        var client = GetService<INotionClientWrapper>();
        // DIコンテナには IMarkdownAssembler として登録されているため、インターフェース経由で取得します。
        var assembler = GetService<IMarkdownAssembler>();
        var pagePropertyMapper = GetService<IPagePropertyMapper>();

        var pages = await client.GetPagesForPublishingAsync(Secrets.NotionDatabaseId);

        if (pages.Count == 0)
        {
            Assert.Skip("テスト用DBにページが存在しないためスキップします。");
        }

        var firstPage = pages[0];
        var pageProperty = pagePropertyMapper.Map(firstPage);

        var outputDir = Path.Combine(Secrets.OutputDirectory, "test-output");
        Directory.CreateDirectory(outputDir);

        // Act
        var markdown = await assembler.AssembleAsync(pageProperty, outputDir);

        // Assert
        Assert.NotNull(markdown);
        Assert.NotEmpty(markdown);
        Assert.Contains("---", markdown);

        var outputPath = Path.Combine(outputDir, "test-output.md");
        await File.WriteAllTextAsync(outputPath, markdown);
        Console.WriteLine($"Output written to: {outputPath}");
        Console.WriteLine("--- Markdown preview (first 500 chars) ---");
        Console.WriteLine(markdown[..Math.Min(500, markdown.Length)]);
    }

    /// <summary>
    /// エクスポート全体フローを実行する。
    /// NotionExporter.ExportPagesAsync() を直接呼ぶE2Eに近いテスト。
    /// 注意：このテストはNotionのページプロパティを実際に更新します。
    ///       テスト用DBを使うか、更新処理をモックにするか検討してください。
    /// </summary>
    [Fact]
    public async Task ExportPagesAsync_FullFlow_CreatesMarkdownFiles()
    {
        // Arrange
        var exporter = GetService<INotionExporter>();

        // Act
        await exporter.ExportPagesAsync();

        // Assert
        var files = Directory.GetFiles(Secrets.OutputDirectory, "*.md", SearchOption.AllDirectories);
        Assert.NotEmpty(files);

        foreach (var file in files)
        {
            Console.WriteLine($"Generated: {file}");
        }
    }
}
