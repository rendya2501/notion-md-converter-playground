// Program.cs - エントリポイント
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notion.Client;
using NotionMarkdownConverter.Application.Services;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Core.Services.Markdown;
using NotionMarkdownConverter.Core.Services.Test;
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

// NotionClientの登録
services.AddSingleton<INotionClient>(provider =>
    NotionClientFactory.Create(new ClientOptions
    {
    var options = provider.GetRequiredService<IOptions<AppConfiguration>>();
    var config = options.Value;
    return NotionClientFactory.Create(new ClientOptions
    {
        AuthToken = config.NotionAuthToken
    });
});

// サービスの登録
services.AddSingleton<INotionClientWrapper, NotionClientWrapper>();
services.AddSingleton<IGitHubEnvironmentUpdater, GitHubEnvironmentUpdater>();
services.AddSingleton<IOutputDirectoryBuilder, OutputDirectoryBuilder>();
services.AddSingleton<IFrontmatterGenerator, FrontmatterGenerator>();
services.AddSingleton<IContentGenerator, ContentGenerator>();
services.AddSingleton<IMarkdownGenerator, MarkdownGenerator>();
services.Configure<DownloaderOptions>(options =>
{
    options.MaxRetryCount = 3;
    options.RetryDelayMilliseconds = 1000;
    options.TimeoutSeconds = 30;
    options.SkipExistingFiles = true;
});
services.AddSingleton<IImageDownloader, ImageDownloader>();
services.AddSingleton<IFileDownloader, FileDownloader>();
services.AddSingleton<IMarkdownImageProcessor, MarkdownImageProcessor>();
services.AddSingleton<IFileProcessor, FileProcessor>();
services.AddSingleton<INotionExporter, NotionExporter>();

services.AddSingleton<EventBus>();
services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<EventBus>());


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
// services.AddSingleton<IBlockTransformStrategy, ImageTransformStrategy>();
services.AddSingleton<IBlockTransformStrategy>(sp => {
    var downloader = sp.GetRequiredService<IFileDownloader>();
    var bus = sp.GetRequiredService<EventBus>();
    var strategy = new ImageTransformStrategy(downloader);
    bus.Subscribe(strategy);
    return strategy;
});
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



// --- OutputDirectoryChangedEvent.cs ---
public record OutputDirectoryChangedEvent(string OutputDirectory);

// --- IEventPublisher.cs ---
public interface IEventPublisher
{
    void Publish<T>(T eventData);
}

// --- IEventSubscriber.cs ---
public interface IEventSubscriber<T>
{
    void OnEvent(T eventData);
}

// --- EventBus.cs ---
public class EventBus : IEventPublisher
{
    private readonly List<object> _subscribers = new();

    public void Subscribe<T>(IEventSubscriber<T> subscriber)
    {
        _subscribers.Add(subscriber);
    }

    public void Publish<T>(T eventData)
    {
        foreach (var subscriber in _subscribers.OfType<IEventSubscriber<T>>())
        {
            subscriber.OnEvent(eventData);
        }
    }
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

