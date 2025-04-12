using Notion.Client;
using NotionMarkdownConverter.Transformer.Strategies;

namespace NotionMarkdownConverter.Transformer;

/// <summary>
/// ブロック変換ストラテジーのファクトリークラス
/// </summary>
public class StrategySelector(IEnumerable<IBlockTransformStrategy> _strategies)
{
    public IBlockTransformStrategy GetStrategy(BlockType blockType)
    {
        return _strategies.FirstOrDefault(s => s.BlockType == blockType)
            ?? new DefaultTransformStrategy();
            // ?? throw new InvalidOperationException($"No strategy found for block type: {blockType}");
    }

    // private readonly Dictionary<Type, IBlockTransformStrategy> _strategies = new()
    // {
    //     { typeof(BookmarkBlock), new BookmarkTransformStrategy() },
    //     { typeof(BreadcrumbBlock), new BreadcrumbTransformStrategy() },
    //     { typeof(CalloutBlock), new CalloutTransformStrategy() },
    //     { typeof(CodeBlock), new CodeTransformStrategy() },
    //     { typeof(ColumnListBlock), new ColumnListTransformStrategy() },
    //     { typeof(DividerBlock), new DividerTransformStrategy() },
    //     { typeof(EquationBlock), new EquationTransformStrategy() },
    //     { typeof(HeadingOneBlock), new HeadingTransformStrategy() },
    //     { typeof(HeadingTwoBlock), new HeadingTransformStrategy() },
    //     { typeof(HeadingThreeBlock), new HeadingTransformStrategy() },
    //     { typeof(LinkPreviewBlock), new LinkPreviewTransformStrategy() },
    //     { typeof(BulletedListItemBlock), new BulletedListItemTransformStrategy() },
    //     { typeof(NumberedListItemBlock), new NumberedListItemTransformStrategy() },
    //     { typeof(ToDoBlock), new TodoListItemTransformStrategy() },
    //     { typeof(ToggleBlock), new ToggleTransformStrategy() },
    //     { typeof(ParagraphBlock), new ParagraphTransformStrategy() },
    //     { typeof(QuoteBlock), new QuoteTransformStrategy() },
    //     { typeof(SyncedBlockBlock), new SyncedBlockTransformStrategy() },
    //     { typeof(TableOfContentsBlock), new TableOfContentsTransformStrategy() },
    //     { typeof(TableBlock), new TableTransformStrategy() },
    //     { typeof(FileBlock), new FileTransformStrategy() },
    //     { typeof(ImageBlock), new ImageTransformStrategy() },
    //     { typeof(PDFBlock), new PDFTransformStrategy() },
    //     { typeof(VideoBlock), new VideoTransformStrategy() },
    //     { typeof(EmbedBlock), new EmbedTransformStrategy() }
    // };

    // /// <summary>
    // /// ブロックタイプに応じたストラテジーを取得します
    // /// </summary>
    // /// <param name="blockType">ブロックタイプ</param>
    // /// <returns>変換ストラテジー</returns>
    // public IBlockTransformStrategy GetStrategy(Type blockType)
    // {
    //     if (_strategies.TryGetValue(blockType, out var strategy))
    //     {
    //         return strategy;
    //     }
    //     return new DefaultTransformStrategy();
    // }

}