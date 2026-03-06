using Microsoft.Extensions.DependencyInjection;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Application.Services;

namespace NotionMarkdownConverter.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        string[] args)
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

        return services;
    }
}
