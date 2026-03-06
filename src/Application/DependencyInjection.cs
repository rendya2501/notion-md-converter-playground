using Microsoft.Extensions.DependencyInjection;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Application.Services;

namespace NotionMarkdownConverter.Application;

/// <summary>
/// アプリケーション層のDIクラス
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// アプリケーション層のサービスをDIコンテナに登録します。
    /// </summary>
    /// <param name="services">DIコンテナ</param>
    /// <param name="args">コマンドライン引数</param>
    /// <returns>DIコンテナ</returns>
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

        // Notionのページをエクスポートするサービスを登録
        services.AddSingleton<INotionExporter, NotionExporter>();
        // Markdownの組み立てサービスを登録
        services.AddSingleton<MarkdownAssembler>();
        // Markdown内のダウンロードリンク処理サービスを登録
        services.AddSingleton<MarkdownLinkProcessor>();

        return services;
    }
}
