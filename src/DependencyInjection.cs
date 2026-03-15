using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Extract;
using NotionMarkdownConverter.Infrastructure.FileSystem;
using NotionMarkdownConverter.Infrastructure.GitHub;
using NotionMarkdownConverter.Infrastructure.Http;
using NotionMarkdownConverter.Infrastructure.Notion;
using NotionMarkdownConverter.Load;
using NotionMarkdownConverter.Pipeline;
using NotionMarkdownConverter.Transform;
using NotionMarkdownConverter.Transform.Converters;
using NotionMarkdownConverter.Transform.Strategies;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;

namespace NotionMarkdownConverter;

/// <summary>
/// DIコンテナへのサービス登録
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// アプリケーションのサービスを登録します。
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <param name="options">
    /// コマンドライン引数からパース済みの設定値。
    /// Program.csで即時バリデーション済みのため、ここでは検証しません。
    /// </param>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        NotionExportOptions options)
    {
        // --- Configuration ---
        // Options.Createを使うことでIOptions<T>として注入可能にしつつ、Configureの遅延評価を排除します。
        services.AddSingleton(Options.Create(options));

        // --- Infrastructure ---
        services.AddHttpClient();
        services.Configure<HttpFileDownloaderOptions>(o =>
        {
            o.MaxRetryCount = 3;
            o.RetryDelayMilliseconds = 1000;
            o.TimeoutSeconds = 30;
            o.SkipExistingFiles = true;
        });
        services.AddSingleton<INotionClient>(provider =>
        {
            var config = provider.GetRequiredService<IOptions<NotionExportOptions>>().Value;
            return NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = config.NotionAuthToken
            });
        });
        services.AddSingleton<NotionClientWrapper>();
        services.AddSingleton<INotionPageReader>(p => p.GetRequiredService<NotionClientWrapper>());
        services.AddSingleton<INotionPageWriter>(p => p.GetRequiredService<NotionClientWrapper>());
        services.AddSingleton<IGitHubEnvironmentUpdater, GitHubEnvironmentUpdater>();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IFileDownloader, HttpFileDownloader>();

        // --- Extract ---
        services.AddSingleton<IPagePropertyMapper, PagePropertyMapper>();
        services.AddSingleton<PageExportEligibilityChecker>();
        services.AddSingleton<NotionPageExtractor>();

        // --- Transform ---
        services.AddSingleton<IMarkdownLinkProcessor, MarkdownLinkProcessor>();
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
        services.AddSingleton<ContentConverter>();
        services.AddSingleton<FrontmatterConverter>();
        services.AddSingleton<NotionPageTransformer>();
        services.AddSingleton<OutputPathBuilder>();

        // --- Load ---
        services.AddSingleton<NotionPageLoader>();

        // --- Pipeline ---
        services.AddSingleton<NotionExportPipeline>();

        return services;
    }
}
