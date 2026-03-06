// tests/Integration/NotionExportIntegrationTests.cs
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Services;
using NotionMarkdownConverter.Domain.Mappers;

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
        // Arrange（準備）
        // DIコンテナからNotionClientWrapperを取得
        var client = GetService<INotionClientWrapper>();

        // Act（実行）
        var pages = await client.GetPagesForPublishingAsync(Secrets.NotionDatabaseId);

        // Assert（検証）
        // ページが取得できた（0件でも取得処理は成功）
        Assert.NotNull(pages);
        // ページの内容をコンソールに出力（テスト結果で確認できる）
        foreach (var page in pages)
        {
            Console.WriteLine($"Page: {page.Id}");
        }
    }

    /// <summary>
    /// MarkdownGeneratorが実際のNotionページからMarkdownを生成できることを確認する。
    /// ページが0件の場合はスキップ。
    /// </summary>
    [Fact]
    public async Task GenerateMarkdown_FromRealNotionPage_ProducesMarkdown()
    {
        // Arrange
        var client = GetService<INotionClientWrapper>();
        var markdownGenerator = GetService<MarkdownGenerator>();

        var pages = await client.GetPagesForPublishingAsync(Secrets.NotionDatabaseId);

        // ページが0件なら検証できないのでスキップ
        // Skip()はxUnitでテストを条件付きでスキップする方法
        if (pages.Count == 0)
        {
            return; // スキップ相当（本来はSkip属性を使うが、動的スキップはxUnitでは別途対応が必要）
        }

        var firstPage = pages[0];

        // PagePropertyMapperで変換
        var pagePropertyMapper = GetService<IPagePropertyMapper>();
        var pageProperty = pagePropertyMapper.CopyPageProperties(firstPage);

        // 出力ディレクトリを作成
        var outputDir = Path.Combine(Secrets.OutputDirectory, "test-output");
        Directory.CreateDirectory(outputDir);

        // Act
        var markdown = await markdownGenerator.GenerateMarkdownAsync(pageProperty, outputDir);

        // Assert
        Assert.NotNull(markdown);
        Assert.NotEmpty(markdown);

        // フロントマターが含まれているか
        Assert.Contains("---", markdown);

        // 結果をファイルに書き出して目視確認できるようにする
        var outputPath = Path.Combine(outputDir, "test-output.md");
        await File.WriteAllTextAsync(outputPath, markdown);
        Console.WriteLine($"Output written to: {outputPath}");
        Console.WriteLine("--- Markdown preview (first 500 chars) ---");
        Console.WriteLine(markdown[..Math.Min(500, markdown.Length)]);
    }

    /// <summary>
    /// エクスポート全体フローを実行する。
    /// NotionExporter.ExportPagesAsync()を直接呼ぶE2Eに近いテスト。
    /// 注意：このテストはNotionのページプロパティを実際に更新します。
    ///       テスト用DBを使うか、更新処理をモックにするか検討してください。
    /// </summary>
    [Fact(Skip = "NotionのDBを実際に更新するため、手動実行時のみ有効")]
    public async Task ExportPagesAsync_FullFlow_CreatesMarkdownFiles()
    {
        // Arrange
        var exporter = GetService<INotionExporter>();

        // Act
        await exporter.ExportPagesAsync();

        // Assert
        // 出力ディレクトリにファイルが生成されているか確認
        var files = Directory.GetFiles(Secrets.OutputDirectory, "*.md", SearchOption.AllDirectories);
        Assert.NotEmpty(files);

        foreach (var file in files)
        {
            Console.WriteLine($"Generated: {file}");
        }
    }
}
