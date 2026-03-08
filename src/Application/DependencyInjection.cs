using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Application.Services;
using NotionMarkdownConverter.Domain.Mappers;
using NotionMarkdownConverter.Domain.Markdown.Converters;
using NotionMarkdownConverter.Domain.Transformers;
using NotionMarkdownConverter.Domain.Transformers.Strategies;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;

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
        // ページの公開対象判定サービスを登録
        services.AddSingleton<PageExportEligibilityChecker>();

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

        // マークダウンコンバーターを登録
        services.AddSingleton<ContentConverter>();
        // フロントマター変換サービスを登録
        services.AddSingleton<FrontmatterConverter>();
        // ページプロパティマッパーを登録
        services.AddSingleton<IPagePropertyMapper, PagePropertyMapper>();


        return services;
    }
}
