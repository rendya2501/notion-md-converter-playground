using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Infrastructure.FileSystem;
using NotionMarkdownConverter.Infrastructure.GitHub;
using NotionMarkdownConverter.Infrastructure.Http;
using NotionMarkdownConverter.Infrastructure.Notion;

namespace NotionMarkdownConverter.Infrastructure;

/// <summary>
/// インフラストラクチャ層のDIクラス
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// インフラストラクチャ層のサービスをDIコンテナに登録します。
    /// </summary>
    /// <param name="services">DIコンテナ</param>
    /// <returns>DIコンテナ</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // ダウンローダーのオプション設定
        services.Configure<HttpFileDownloaderOptions>(options =>
        {
            options.MaxRetryCount = 3;
            options.RetryDelayMilliseconds = 1000;
            options.TimeoutSeconds = 30;
            options.SkipExistingFiles = true;
        });

        // HttpClientの登録
        services.AddHttpClient();

        // NotionClientの登録
        services.AddSingleton<INotionClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<NotionExportOptions>>();
            var config = options.Value;
            return NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = config.NotionAuthToken
            });
        });

        // NotionClientWrapperの登録
        services.AddSingleton<INotionClientWrapper, NotionClientWrapper>();
        // GitHubEnvironmentUpdaterの登録
        services.AddSingleton<IGitHubEnvironmentUpdater, GitHubEnvironmentUpdater>();
        // OutputDirectoryBuilderの登録
        services.AddSingleton<IOutputDirectoryBuilder, OutputDirectoryBuilder>();
        // FileDownloaderの登録
        services.AddSingleton<IFileDownloader, HttpFileDownloader>();

        return services;
    }
}