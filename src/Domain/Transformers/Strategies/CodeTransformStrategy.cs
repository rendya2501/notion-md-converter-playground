using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// コード変換ストラテジー
/// </summary>
public class CodeTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Code;

    public string Transform(NotionBlockTransformContext context)
    {
        var codeBlock = BlockConverter.GetOriginalBlock<CodeBlock>(context.CurrentBlock);
        var language = codeBlock.Code.Language;
        var text = MarkdownRichTextUtils.RichTextsToMarkdown(codeBlock.Code.RichText);

        return MarkdownBlockUtils.CodeBlock(text, language);
    }
} 