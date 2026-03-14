using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// 方程式変換ストラテジー
/// </summary>
public class EquationTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Equation;

    public string Transform(NotionBlockTransformContext context)
    {
        // 方程式ブロックを取得
        var block = BlockAccessor.GetOriginalBlock<EquationBlock>(context.CurrentBlock);
        // 方程式をMarkdown形式で変換して返す
        return MarkdownBlockUtils.BlockEquation(block.Equation.Expression);
    }
}
