using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotionMarkdownConverter.Application;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Domain;
using NotionMarkdownConverter.Infrastructure;

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
    .AddApplicationServices(args)
    .AddDomainServices()
    .AddInfrastructureServices();

// サービスプロバイダーの構築
await using var serviceProvider = services.BuildServiceProvider();

// NotionExporterの取得
var exporter = serviceProvider.GetRequiredService<INotionExporter>();

// Notionのページのエクスポートを実行
await exporter.ExportPagesAsync();
