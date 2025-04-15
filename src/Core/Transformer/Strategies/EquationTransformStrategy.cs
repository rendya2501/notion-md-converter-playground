using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

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
    public string Transform(NotionBlockTransformState context)
    {
        // 方程式ブロックを取得
        var block = BlockConverter.GetOriginalBlock<EquationBlock>(context.CurrentBlock);
        // 方程式の式を取得
        var text = block.Equation.Expression;
        // 方程式の式をMarkdown形式に変換
        var result = block.Type == BlockType.Code
            ? MarkdownUtils.CodeBlock(text, "txt")
            : MarkdownUtils.BlockEquation(text);

        return result;
    }
}