using Notion.Client;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// カラムリスト変換ストラテジー
/// </summary>
public class ColumnListTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.ColumnList;

    public string Transform(NotionBlockTransformContext context)
    {
        // カラムリストの子ブロックを取得
        var columns = context.CurrentBlock.Children;
        // カラムリストの子ブロックを変換
        var columnsText = columns.Select(column => 
            context.ExecuteTransformBlocks(column.Children));

        // カラムリストの子ブロックを変換したマークダウン文字列を返す
        return string.Join("\n", columnsText);
    }
}
