using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

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
    public string Transform(NotionBlockTransformContext context)
    {
        var codeBlock = BlockConverter.GetOriginalBlock<CodeBlock>(context.CurrentBlock);
        var language = codeBlock.Code.Language;
        var text = MarkdownRichTextUtils.RichTextsToMarkdown(codeBlock.Code.RichText);

        return MarkdownBlockUtils.CodeBlock(text, language);
    }
} 