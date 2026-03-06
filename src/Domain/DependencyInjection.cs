using Microsoft.Extensions.DependencyInjection;
using NotionMarkdownConverter.Domain.Mappers;
using NotionMarkdownConverter.Domain.Markdown.Converters;
using NotionMarkdownConverter.Domain.Transformers;
using NotionMarkdownConverter.Domain.Transformers.Strategies;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;

namespace NotionMarkdownConverter.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        //// ブロック変換ストラテジーを登録
        //services.Scan(scan => scan
        //    .FromAssemblyOf<Transformers.Strategies.ParagraphTransformStrategy>()
        //    .AddClasses(classes => classes.AssignableTo<Transformers.Strategies.IBlockTransformStrategy>())
        //    .AsImplementedInterfaces()
        //    .WithSingletonLifetime());

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
