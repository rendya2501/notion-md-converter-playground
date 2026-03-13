using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Enums;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// トグル変換ストラテジー
/// </summary>
public class ToggleTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Toggle;

    public string Transform(NotionBlockTransformContext context)
    {
        // 子ブロックを変換
        var children = context.ExecuteTransformBlocks(context.CurrentBlock.Children);
        // トグルブロックを取得
        var toggleBlock = BlockAccessor.GetOriginalBlock<ToggleBlock>(context.CurrentBlock);
        // タイトルを取得して改行を追加
        var title = MarkdownBlockUtils.ApplyLineBreaks(
            MarkdownRichTextUtils.RichTextsToMarkdown(toggleBlock.Toggle.RichText), LineBreakStyle.BR);

        // 詳細を生成
        return MarkdownBlockUtils.Details(title, children);
    }
}
