// tests/Integration/IntegrationTestBase.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Application.Services;
using NotionMarkdownConverter.Domain.Markdown.Converters;
using NotionMarkdownConverter.Domain.Transformers;
using NotionMarkdownConverter.Domain.Transformers.Strategies;
using NotionMarkdownConverter.Infrastructure.FileSystem;
using NotionMarkdownConverter.Infrastructure.GitHub;
using NotionMarkdownConverter.Infrastructure.Http;
using NotionMarkdownConverter.Infrastructure.Notion;

namespace NotionMarkdownConverter.Tests.Integration;

/// <summary>
/// 統合テストの基底クラス。
/// DIコンテナの構築とUser Secretsの読み込みを担当します。
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    /// <summary>
    /// テスト用の設定値
    /// </summary>
    protected readonly TestSecrets Secrets;

    /// <summary>
    /// DIコンテナから解決したサービスプロバイダー
    /// </summary>
    protected readonly ServiceProvider ServiceProvider;

    protected IntegrationTestBase()
    {
        // User SecretsはUserSecretsIdを使って読み込む
        // テストプロジェクトのアセンブリを指定することで、そのプロジェクトのsecrets.jsonが読まれる
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<IntegrationTestBase>()
            .Build();

        // シークレット値を読み込む
        Secrets = new TestSecrets
        {
            NotionAuthToken = configuration["Notion:AuthToken"]
                ?? throw new InvalidOperationException(
                    "Notion:AuthToken が設定されていません。" +
                    "'dotnet user-secrets set \"Notion:AuthToken\" \"your-token\"' を実行してください。"),
            NotionDatabaseId = configuration["Notion:DatabaseId"]
                ?? throw new InvalidOperationException("Notion:DatabaseId が設定されていません。"),
            OutputDirectory = configuration["Test:OutputDirectory"]
                ?? Path.Combine(Path.GetTempPath(), "notion-test-output")
        };

        // DIコンテナを構築
        var services = new ServiceCollection();
        ConfigureServices(services, Secrets);
        ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// DIサービスを登録します。
    /// Program.cs の登録と同じ内容ですが、テスト用設定を使います。
    /// </summary>
    private static void ConfigureServices(IServiceCollection services, TestSecrets secrets)
    {
        // ロギング
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Application層
        services.Configure<NotionExportOptions>(config =>
        {
            config.NotionAuthToken = secrets.NotionAuthToken;
            config.NotionDatabaseId = secrets.NotionDatabaseId;
            // テスト用の出力テンプレート（シンプルなパス）
            config.OutputDirectoryPathTemplate = Path.Combine(secrets.OutputDirectory, "{{slug}}");
        });

        services.AddSingleton<INotionExporter, NotionExporter>();
        services.AddSingleton<MarkdownGenerator>();
        services.AddSingleton<DownloadLinkProcessor>();

        // Domain層
        services.AddSingleton<FrontmatterConverter>();
        services.AddSingleton<ContentConverter>();
        services.AddSingleton<IBlockTransformStrategy, BookmarkTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, BreadcrumbTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, BulletedListItemTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, CalloutTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, CodeTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, ColumnListTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, DividerTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, EmbedTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, EquationTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, FileTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, HeadingOneTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, HeadingTwoTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, HeadingThreeTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, ImageTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, LinkPreviewTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, NumberedListItemTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, ParagraphTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, PDFTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, QuoteTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, SyncedBlockTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, TableOfContentsTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, TableTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, TodoListItemTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, ToggleTransformStrategy>();
        services.AddSingleton<IBlockTransformStrategy, VideoTransformStrategy>();
        services.AddSingleton<IDefaultBlockTransformStrategy, DefaultTransformStrategy>();
        services.AddSingleton<BlockTransformDispatcher>();

        // Infrastructure層
        services.Configure<DownloaderOptions>(options =>
        {
            options.MaxRetryCount = 3;
            options.RetryDelayMilliseconds = 1000;
            options.TimeoutSeconds = 30;
            options.SkipExistingFiles = true;
        });

        services.AddHttpClient();

        services.AddSingleton<INotionClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<NotionExportOptions>>();
            return NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = options.Value.NotionAuthToken
            });
        });

        services.AddSingleton<INotionClientWrapper, NotionClientWrapper>();
        services.AddSingleton<IGitHubEnvironmentUpdater, GitHubEnvironmentUpdater>();
        services.AddSingleton<IOutputDirectoryBuilder, OutputDirectoryBuilder>();
        services.AddSingleton<IFileDownloader, FileDownloader>();
    }

    /// <summary>
    /// DIコンテナからサービスを取得するショートカット
    /// </summary>
    protected T GetService<T>() where T : notnull
        => ServiceProvider.GetRequiredService<T>();

    public void Dispose()
    {
        ServiceProvider.Dispose();
    }
}

/// <summary>
/// テスト用のシークレット設定値
/// </summary>
public class TestSecrets
{
    public string NotionAuthToken { get; set; } = string.Empty;
    public string NotionDatabaseId { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
}
