using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// 方程式変換ストラテジー
/// </summary>
public class EquationTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Equation;

    public string Transform(NotionBlockTransformContext context)
    {
        // 方程式ブロックを取得
        var block = BlockCaster.GetOriginalBlock<EquationBlock>(context.CurrentBlock);
        // 方程式の式を取得
        var text = block.Equation.Expression;
        // 方程式の式をMarkdown形式に変換
        var result = block.Type == BlockType.Code
            ? MarkdownBlockUtils.CodeBlock(text, "txt")
            : MarkdownBlockUtils.BlockEquation(text);

        return result;
    }
}