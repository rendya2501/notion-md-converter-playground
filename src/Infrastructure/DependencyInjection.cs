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

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // ダウンローダーのオプション設定
        services.Configure<DownloaderOptions>(options =>
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

        services.AddSingleton<INotionClientWrapper, NotionClientWrapper>();
        services.AddSingleton<IGitHubEnvironmentUpdater, GitHubEnvironmentUpdater>();
        services.AddSingleton<IOutputDirectoryBuilder, OutputDirectoryBuilder>();
        services.AddSingleton<IFileDownloader, FileDownloader>();

        return services;
    }
}