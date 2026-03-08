using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Enums;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

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
