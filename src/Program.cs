// Program.cs - エントリポイント
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Application.Interfaces;
using NotionMarkdownConverter.Application.UseCases;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Core.Services;
using NotionMarkdownConverter.Core.Transformer.Strategies;
using NotionMarkdownConverter.Infrastructure.Service;
using NotionMarkdownConverter.Infrastructure.Services;

// DIコンテナの設定
var services = new ServiceCollection();

// ロギングの設定
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// コマンドライン引数から設定を取得
services.Configure<AppConfiguration>(config =>
{
    if (args.Length != 3)
    {
        throw new ArgumentException("Required arguments: [NotionAuthToken] [DatabaseId] [OutputPathTemplate]");
    }

    config.NotionAuthToken = args[0];
    config.NotionDatabaseId = args[1];
    config.OutputDirectoryPathTemplate = args[2];
});
// ダウンローダーのオプション設定
services.Configure<DownloaderOptions>(options =>
{
    options.MaxRetryCount = 3;
    options.RetryDelayMilliseconds = 1000;
    options.TimeoutSeconds = 30;
    options.SkipExistingFiles = true;
});

// 各層のサービスを登録
RegisterApplicationServices(services);
RegisterDomainServices(services);
RegisterInfrastructureServices(services);

// サービスプロバイダーの構築
var serviceProvider = services.BuildServiceProvider();

// サービスの取得と実行
var exporter = serviceProvider.GetRequiredService<ConvertNotionToMarkdownUseCase>();
await exporter.ExportPagesAsync();

// リソースの解放
if (serviceProvider is IDisposable disposable)
{
    disposable.Dispose();
}




// アプリケーション層のサービス登録
void RegisterApplicationServices(IServiceCollection services)
{
    services.AddSingleton<ConvertNotionToMarkdownUseCase>();
}

// ドメイン層のサービス登録
void RegisterDomainServices(IServiceCollection services)
{
    services.AddSingleton<MarkdownGenerator>();
    services.AddSingleton<FrontmatterGenerator>();
    services.AddSingleton<ContentGenerator>();
    services.AddSingleton<DownloadLinkProcessor>();

    // ストラテジーの登録
    services.AddSingleton<IBlockTransformStrategy, BookmarkTransformStrategy>();
    services.AddSingleton<IBlockTransformStrategy, BreadcrumbTransformStrategy>();
    services.AddSingleton<IBlockTransformStrategy, BulletedListItemTransformStrategy>();
    services.AddSingleton<IBlockTransformStrategy, CalloutTransformStrategy>();
    services.AddSingleton<IBlockTransformStrategy, CodeTransformStrategy>();
    services.AddSingleton<IBlockTransformStrategy, ColumnListTransformStrategy>();
    services.AddSingleton<IBlockTransformStrategy, DividerTransformStrategy>();
    services.AddSingleton<IBlockTransformStrategy, DefaultTransformStrategy>();
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

    // ディクショナリを生成して DI コンテナに登録
    services.AddSingleton<IDictionary<BlockType, IBlockTransformStrategy>>(provider =>
    {
        var strategies = provider.GetServices<IBlockTransformStrategy>();
        return strategies.ToDictionary(strategy => strategy.BlockType);
    });
}

// インフラストラクチャ層のサービス登録
void RegisterInfrastructureServices(IServiceCollection services)
{
    // NotionClientの登録
    services.AddSingleton<INotionClient>(provider =>
    {
        var options = provider.GetRequiredService<IOptions<AppConfiguration>>();
        var config = options.Value;
        return NotionClientFactory.Create(new ClientOptions
        {
            AuthToken = config.NotionAuthToken
        });
    });
    services.AddSingleton<INotionWrapperClient, NotionClientWrapper>();
    services.AddSingleton<IGitHubClient, GitHubEnvironmentUpdater>();
    services.AddSingleton<IDirectoryBuilder, OutputDirectoryBuilder>();
    services.AddScoped<IHttpDownloader, FileDownloader>();
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
