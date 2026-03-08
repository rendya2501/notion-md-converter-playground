using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotionMarkdownConverter.Application;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Infrastructure;

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

// 各層のサービスを登録
services
    .AddApplicationServices(options)
    .AddInfrastructureServices();

// サービスプロバイダーの構築
await using var serviceProvider = services.BuildServiceProvider();

// NotionExporterの取得
var exporter = serviceProvider.GetRequiredService<INotionExporter>();

// Notionのページのエクスポートを実行
await exporter.ExportPagesAsync();
