using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotionMarkdownConverter.Configuration;

namespace NotionMarkdownConverter.Tests.Integration;

/// <summary>
/// Notion APIを使う統合テストの基底クラス。
/// DIコンテナの構築とUser Secretsの読み込みを担います。
/// </summary>
public abstract class IntegrationTestBase
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>テスト実行に必要なシークレット情報。</summary>
    protected IntegrationSecrets Secrets { get; }

    protected IntegrationTestBase()
    {
        // User Secrets からNotion認証情報を読み込む
        var config = new ConfigurationBuilder()
            .AddUserSecrets<IntegrationTestBase>()
            .Build();

        Secrets = new IntegrationSecrets(
            NotionAuthToken: config["Notion:AuthToken"] ?? throw new InvalidOperationException("User Secretsに Notion:AuthToken が設定されていません。"),
            NotionDatabaseId: config["Notion:DatabaseId"] ?? throw new InvalidOperationException("User Secretsに Notion:DatabaseId が設定されていません。")
        );

        // OutputDirectoryPathTemplate はテスト側で決定した articles ベースパスを使う
        // （派生クラスで NotionExportOptions.OutputDirectoryPathTemplate を上書き可能にするため、
        //   仮値として空文字を設定し、後述の AddInfrastructureServices に委ねる）
        var options = new NotionExportOptions
        {
            NotionAuthToken = Secrets.NotionAuthToken,
            NotionDatabaseId = Secrets.NotionDatabaseId,
            OutputDirectoryPathTemplate = BuildOutputDirectoryPathTemplate(),
        };

        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddApplicationServices(options);
        services.AddInfrastructureServices();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// テスト出力先のScribanテンプレートを構築します。
    /// articles ベースディレクトリの下に YYYY/MM/スラッグ 形式で出力します。
    /// </summary>
    private static string BuildOutputDirectoryPathTemplate()
    {
        var articlesBase = GetArticlesBaseDirectory();
        // パス区切りを / に統一してScribanテンプレートに埋め込む
        var normalizedBase = articlesBase.Replace('\\', '/');
        return $"{normalizedBase}/{{{{ publish | date.to_string '%Y' }}}}/{{{{ publish | date.to_string '%m' }}}}/{{{{ slug }}}}";
    }

    /// <summary>
    /// テスト出力先のベースディレクトリ（tests/Integration/articles/）を返します。
    /// </summary>
    /// <remarks>
    /// AppContext.BaseDirectory は bin/Debug/net8.0/ なので、
    /// 3階層上がると tests/Integration/、その下の articles/ を使用します。
    /// </remarks>
    internal static string GetArticlesBaseDirectory()
    {
        // bin/Debug/net8.0/ → ../../.. → tests/Integration/
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Integration", "articles"));
    }

    /// <summary>
    /// DIコンテナから指定した型のサービスを取得します。
    /// </summary>
    /// <typeparam name="T">取得するサービスの型。</typeparam>
    protected T GetService<T>() where T : notnull
        => _serviceProvider.GetRequiredService<T>();
}

/// <summary>統合テスト実行に必要なシークレット情報。</summary>
/// <param name="NotionAuthToken">Notion APIの認証トークン。</param>
/// <param name="NotionDatabaseId">エクスポート対象のNotionデータベースID。</param>
public record IntegrationSecrets(string NotionAuthToken, string NotionDatabaseId);
