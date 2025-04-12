using Notion.Client;
using NotionMarkdownConverter.Models;
using NotionMarkdownConverter.Utils;

namespace NotionMarkdownConverter.Transformer.Strategies;

/// <summary>
/// 方程式変換ストラテジー
/// </summary>
public class EquationTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Equation;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        // 方程式ブロックを取得
        var block = context.CurrentBlock.GetOriginalBlock<EquationBlock>();
        // 方程式の式を取得
        var text = block.Equation.Expression;
        // 方程式の式をMarkdown形式に変換
        var result = block.Type == BlockType.Code
            ? MarkdownUtils.CodeBlock(text, "txt")
            : MarkdownUtils.BlockEquation(text);

        return result;
    }
}