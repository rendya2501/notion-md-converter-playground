// Program.cs - エントリポイント
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


// DIコンテナの設定
var services = new ServiceCollection();

// ロギングの設定
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// 各層のサービスを登録
RegisterApplicationServices(services);
RegisterDomainServices(services);
RegisterInfrastructureServices(services);

// サービスプロバイダーの構築
await using var serviceProvider = services.BuildServiceProvider();

// NotionExporterの取得
var exporter = serviceProvider.GetRequiredService<INotionExporter>();

// Notionのページのエクスポートを実行
await exporter.ExportPagesAsync();

/////////////////////////////////////////////////////////////////////////////////////

// アプリケーション層のサービス登録
void RegisterApplicationServices(IServiceCollection services)
{
    // コマンドライン引数から設定を取得
    services.Configure<NotionExportOptions>(config =>
    {
        if (args.Length != 3)
        {
            throw new ArgumentException("Required arguments: [NotionAuthToken] [DatabaseId] [OutputPathTemplate]");
        }

        config.NotionAuthToken = args[0];
        config.NotionDatabaseId = args[1];
        config.OutputDirectoryPathTemplate = args[2];
    });

    services.AddSingleton<INotionExporter, NotionExporter>();
    // マークダウン生成サービスを登録   
    services.AddSingleton<MarkdownGenerator>();
    // ダウンロードリンク処理サービスを登録
    services.AddSingleton<DownloadLinkProcessor>();
}

// ドメイン層のサービス登録
void RegisterDomainServices(IServiceCollection services)
{   
    // フロントマター変換サービスを登録
    services.AddSingleton<FrontmatterConverter>();
    // コンテンツ変換サービスを登録
    services.AddSingleton<ContentConverter>();

    // ストラテジーの登録
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

    // ブロック変換ストラテジーコンテキストを登録
    services.AddSingleton<BlockTransformDispatcher>();
}

// インフラストラクチャ層のサービス登録
void RegisterInfrastructureServices(IServiceCollection services)
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
}


//// コマンドライン引数から設定を取得
//var config = AppConfiguration.FromCommandLine(args);
//var exporter = new NotionExporter(config);
//await exporter.ExportPagesAsync();


// NotionClientの作成
// プロパティの取得
// ページの取得
// プロパティとページの情報からマークダウンの作成
// 後始末処理

// マークダウン変換処理をライブラリとして捉える
