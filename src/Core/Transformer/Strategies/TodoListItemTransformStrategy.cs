using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Infrastructure.Notion.Services;
using NotionMarkdownConverter.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// タスクリスト変換ストラテジー
/// </summary>
public class TodoListItemTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.ToDo;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformState context)
    {
        // タスクリストのブロックを取得 
        var block = BlockConverter.GetOriginalBlock<ToDoBlock>(context.CurrentBlock);
        // タスクリストのテキストを取得して改行を追加
        var text = MarkdownUtils.LineBreak(
            MarkdownUtils.RichTextsToMarkdown(block.ToDo.RichText));

        // テキストに改行が含まれている場合、2行目以降にインデントを適用
        var lines = text.Split('\n');
        var formattedText = lines.Length > 1
            ? $"{lines[0]}\n{string.Join("\n", lines.Skip(1).Select(line => MarkdownUtils.Indent(line)))}"
            : text;

        // 子ブロックが存在する場合、子ブロックを変換
        var children = context.CurrentBlock.HasChildren
            ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
            : string.Empty;

        // チェックボックスを生成
        var checkbox = MarkdownUtils.CheckList(formattedText, block.ToDo.IsChecked);

        // 子ブロックが存在しない場合、タスクリストを返す
        return string.IsNullOrEmpty(children)
            ? $"{checkbox}"
            : $"{checkbox}\n{MarkdownUtils.Indent(children)}";
    }
} 