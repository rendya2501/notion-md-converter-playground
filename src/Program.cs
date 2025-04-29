// Program.cs - エントリポイント
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Application.Services;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Core.Services.Markdown;
using NotionMarkdownConverter.Core.Transformer.Strategies;
using NotionMarkdownConverter.Infrastructure.FileSystem.Services;
using NotionMarkdownConverter.Infrastructure.GitHub.Services;
using NotionMarkdownConverter.Infrastructure.Http.Services;
using NotionMarkdownConverter.Infrastructure.Notion.Clients;

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
var exporter = serviceProvider.GetRequiredService<INotionExporter>();
await exporter.ExportPagesAsync();

// リソースの解放
if (serviceProvider is IDisposable disposable)
{
    disposable.Dispose();
}




// アプリケーション層のサービス登録
void RegisterApplicationServices(IServiceCollection services)
{
    services.AddSingleton<INotionExporter, NotionExporter>();
}

// ドメイン層のサービス登録
void RegisterDomainServices(IServiceCollection services)
{
    services.AddSingleton<IMarkdownGenerator, MarkdownGenerator>();
    services.AddSingleton<IFrontmatterGenerator, FrontmatterGenerator>();
    services.AddSingleton<IContentGenerator, ContentGenerator>();
    services.AddSingleton<IDownloadLinkProcessor, DownloadLinkProcessor>();

    // ストラテジーの登録
    services.AddKeyedSingleton<IBlockTransformStrategy, BookmarkTransformStrategy>(BlockType.Bookmark);
    services.AddKeyedSingleton<IBlockTransformStrategy, BreadcrumbTransformStrategy>(BlockType.Breadcrumb);
    services.AddKeyedSingleton<IBlockTransformStrategy, BulletedListItemTransformStrategy>(BlockType.BulletedListItem);
    services.AddKeyedSingleton<IBlockTransformStrategy, CalloutTransformStrategy>(BlockType.Callout);
    services.AddKeyedSingleton<IBlockTransformStrategy, CodeTransformStrategy>(BlockType.Code);
    services.AddKeyedSingleton<IBlockTransformStrategy, ColumnListTransformStrategy>(BlockType.ColumnList);
    services.AddKeyedSingleton<IBlockTransformStrategy, DividerTransformStrategy>(BlockType.Divider);
    services.AddKeyedSingleton<IBlockTransformStrategy, DefaultTransformStrategy>(BlockType.Unsupported);
    services.AddKeyedSingleton<IBlockTransformStrategy, EmbedTransformStrategy>(BlockType.Embed);
    services.AddKeyedSingleton<IBlockTransformStrategy, EquationTransformStrategy>(BlockType.Equation);
    services.AddKeyedSingleton<IBlockTransformStrategy, FileTransformStrategy>(BlockType.File);
    services.AddKeyedSingleton<IBlockTransformStrategy, HeadingOneTransformStrategy>(BlockType.Heading_1);
    services.AddKeyedSingleton<IBlockTransformStrategy, HeadingTwoTransformStrategy>(BlockType.Heading_2);
    services.AddKeyedSingleton<IBlockTransformStrategy, HeadingThreeTransformStrategy>(BlockType.Heading_3);
    services.AddKeyedSingleton<IBlockTransformStrategy, ImageTransformStrategy>(BlockType.Image);
    services.AddKeyedSingleton<IBlockTransformStrategy, LinkPreviewTransformStrategy>(BlockType.LinkPreview);
    services.AddKeyedSingleton<IBlockTransformStrategy, NumberedListItemTransformStrategy>(BlockType.NumberedListItem);
    services.AddKeyedSingleton<IBlockTransformStrategy, ParagraphTransformStrategy>(BlockType.Paragraph);
    services.AddKeyedSingleton<IBlockTransformStrategy, PDFTransformStrategy>(BlockType.PDF);
    services.AddKeyedSingleton<IBlockTransformStrategy, QuoteTransformStrategy>(BlockType.Quote);
    services.AddKeyedSingleton<IBlockTransformStrategy, SyncedBlockTransformStrategy>(BlockType.SyncedBlock);
    services.AddKeyedSingleton<IBlockTransformStrategy, TableOfContentsTransformStrategy>(BlockType.TableOfContents);
    services.AddKeyedSingleton<IBlockTransformStrategy, TableTransformStrategy>(BlockType.Table);
    services.AddKeyedSingleton<IBlockTransformStrategy, TodoListItemTransformStrategy>(BlockType.ToDo);
    services.AddKeyedSingleton<IBlockTransformStrategy, ToggleTransformStrategy>(BlockType.Toggle);
    services.AddKeyedSingleton<IBlockTransformStrategy, VideoTransformStrategy>(BlockType.Video);

    // Strategyを解決するリゾルバを登録
    services.AddSingleton<IBlockTransformStrategyResolver, BlockTransformStrategyResolver>();
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
    services.AddSingleton<INotionClientWrapper, NotionClientWrapper>();
    services.AddSingleton<IGitHubEnvironmentUpdater, GitHubEnvironmentUpdater>();
    services.AddSingleton<IOutputDirectoryBuilder, OutputDirectoryBuilder>();
    services.AddScoped<IFileDownloader, FileDownloader>();
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


// キー付き戦略を解決するリゾルバインターフェース
public interface IBlockTransformStrategyResolver
{
    IBlockTransformStrategy Resolve(BlockType optionType);
}

// リゾルバ実装
public class BlockTransformStrategyResolver(IServiceProvider serviceProvider) : IBlockTransformStrategyResolver
{
    public IBlockTransformStrategy Resolve(BlockType optionType)
    {
        return serviceProvider.GetRequiredKeyedService<IBlockTransformStrategy>(optionType)
            ?? serviceProvider.GetRequiredKeyedService<IBlockTransformStrategy>(BlockType.Unsupported);
    }
}

