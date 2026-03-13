using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// 箇条書きリスト変換ストラテジー
/// </summary>
public class BulletedListItemTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.BulletedListItem;

    public string Transform(NotionBlockTransformContext context)
    {
        var block = BlockAccessor.GetOriginalBlock<BulletedListItemBlock>(context.CurrentBlock);
        // テキストを改行で分割
        var text = MarkdownBlockUtils.ApplyLineBreaks(
            MarkdownRichTextUtils.RichTextsToMarkdown(block.BulletedListItem.RichText));

        // テキストに改行が含まれている場合、2行目以降にインデントを適用
        var lines = text.Split('\n');
        var formattedText = lines.Length > 1
            ? $"{lines[0]}\n{string.Join("\n", lines.Skip(1).Select(line => MarkdownBlockUtils.Indent(line)))}"
            : text;

        // 子ブロックが存在する場合、子ブロックを変換
        var children = context.CurrentBlock.HasChildren
            ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
            : string.Empty;

        // 子ブロックが存在しない場合、箇条書きリストを生成
        return string.IsNullOrEmpty(children)
            ? MarkdownListUtils.BulletList(formattedText)
            : $"{MarkdownListUtils.BulletList(formattedText)}\n{MarkdownBlockUtils.Indent(children)}";
    }
} 