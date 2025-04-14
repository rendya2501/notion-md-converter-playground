using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Infrastructure.Notion.Services;
using NotionMarkdownConverter.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// コード変換ストラテジー
/// </summary>
public class CodeTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Code;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformState context)
    {
        var codeBlock = BlockConverter.GetOriginalBlock<CodeBlock>(context.CurrentBlock);
        var language = codeBlock.Code.Language;
        var text = MarkdownUtils.RichTextsToMarkdown(codeBlock.Code.RichText);

        return MarkdownUtils.CodeBlock(text, language);
    }
} 