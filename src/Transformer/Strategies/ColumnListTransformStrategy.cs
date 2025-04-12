using Notion.Client;
using NotionMarkdownConverter.Models;

namespace NotionMarkdownConverter.Transformer.Strategies;

/// <summary>
/// カラムリスト変換ストラテジー
/// </summary>
public class ColumnListTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.ColumnList;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
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
