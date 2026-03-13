using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Shared.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// 番号付きリスト変換ストラテジー
/// </summary>
public class NumberedListItemTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.NumberedListItem;

    public string Transform(NotionBlockTransformContext context)
    {
        // 現在のインデックスより前にある連続した NumberedListItemBlock の数を数えて連番を決定します。
        // 計算量はO(n)ですが、呼び出しはブロック数分繰り返されるため、全体ではO(n²)になります。
        // ブロック数が通常のページ規模（数百件以下）では実用上問題ありません。
        // 将来的にパフォーマンスが問題になる場合は、ContentConverter側でカウンターを管理する設計変更を検討してください。
        var listCount = context.Blocks
            .Take(context.CurrentBlockIndex)
            .Reverse()
            .TakeWhile(b => b.OriginalBlock is NumberedListItemBlock)
            .Count() + 1;

        // 番号付きリストのブロックを取得
        var block = BlockAccessor.GetOriginalBlock<NumberedListItemBlock>(context.CurrentBlock);
        // 番号付きリストのテキストを取得して改行を追加
        var text = MarkdownBlockUtils.ApplyLineBreaks(
            MarkdownRichTextUtils.RichTextsToMarkdown(
                block.NumberedListItem.RichText));

        // テキストに改行が含まれている場合、2行目以降にインデントを適用
        var lines = text.Split('\n');
        var formattedText = lines.Length > 1
            ? $"{lines[0]}\n{string.Join("\n", lines.Skip(1).Select(line => MarkdownBlockUtils.Indent(line, 3)))}"
            : text;

        // 子ブロックが存在する場合、子ブロックを変換
        var children = context.CurrentBlock.HasChildren
            ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
            : string.Empty;

        // 子ブロックが存在しない場合、番号付きリストを返す
        return string.IsNullOrEmpty(children)
            ? MarkdownListUtils.NumberedList(formattedText, listCount)
            : $"{MarkdownListUtils.NumberedList(formattedText, listCount)}\n{MarkdownBlockUtils.Indent(children, 3)}";
    }
}
