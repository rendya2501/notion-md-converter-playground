using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotionMarkdownConverter;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Pipeline;

// コマンドライン引数を即時パース・バリデーションします。
// Configure内で行うと遅延評価により、NotionExportOptionsの初回アクセス時まで
// エラーが発生せず原因特定が困難になるため、ここで即時チェックします。
if (args.Length != 3)
{
    Console.Error.WriteLine("Usage: NotionMarkdownConverter <auth_token> <database_id> <output_directory_path_template>");
    return;
}

var options = new NotionExportOptions
{
    NotionAuthToken = args[0],
    NotionDatabaseId = args[1],
    OutputDirectoryPathTemplate = args[2]
};

// DIコンテナを構築
var services = new ServiceCollection();

// ロギングの設定
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// アプリケーションのサービスを登録
services.AddApplicationServices(options);

// サービスプロバイダーの構築
await using var serviceProvider = services.BuildServiceProvider();

// パイプラインサービスの解決と実行
var pipeline = serviceProvider.GetRequiredService<NotionExportPipeline>();
await pipeline.RunAsync();
