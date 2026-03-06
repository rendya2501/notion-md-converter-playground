using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// タスクリスト変換ストラテジー
/// </summary>
public class TodoListItemTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.ToDo;

    public string Transform(NotionBlockTransformContext context)
    {
        // タスクリストのブロックを取得 
        var block = BlockConverter.GetOriginalBlock<ToDoBlock>(context.CurrentBlock);
        // タスクリストのテキストを取得して改行を追加
        var text = MarkdownBlockUtils.LineBreak(
            MarkdownRichTextUtils.RichTextsToMarkdown(block.ToDo.RichText));

        // テキストに改行が含まれている場合、2行目以降にインデントを適用
        var lines = text.Split('\n');
        var formattedText = lines.Length > 1
            ? $"{lines[0]}\n{string.Join("\n", lines.Skip(1).Select(line => MarkdownBlockUtils.Indent(line)))}"
            : text;

        // 子ブロックが存在する場合、子ブロックを変換
        var children = context.CurrentBlock.HasChildren
            ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
            : string.Empty;

        // チェックボックスを生成
        var checkbox = MarkdownListUtils.CheckList(formattedText, block.ToDo.IsChecked);

        // 子ブロックが存在しない場合、タスクリストを返す
        return string.IsNullOrEmpty(children)
            ? checkbox
            : $"{checkbox}\n{MarkdownBlockUtils.Indent(children)}";
    }
} 