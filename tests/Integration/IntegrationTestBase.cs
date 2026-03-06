using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Application.Services;
using NotionMarkdownConverter.Domain;
using NotionMarkdownConverter.Infrastructure;

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

        // ドメイン層とインフラ層のサービスを登録
        services
            .AddDomainServices()
            .AddInfrastructureServices();

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
