using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    /// アプリケーション層のサービスを登録します。
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
        // Options.Createを使うことでIOptions<T>として注入可能にしつつ、
        // Configureの遅延評価を排除します。
        services.AddSingleton(Options.Create(options));

        // Notionのページをエクスポートするサービスを登録
        services.AddSingleton<INotionExporter, NotionExporter>();
        // Markdownの組み立てサービスを登録
        services.AddSingleton<MarkdownAssembler>();
        // Markdown内のダウンロードリンク処理サービスを登録
        services.AddSingleton<MarkdownLinkProcessor>();

        return services;
    }
}
