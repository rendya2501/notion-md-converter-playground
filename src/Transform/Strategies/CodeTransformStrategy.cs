using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// コード変換ストラテジー
/// </summary>
public class CodeTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Code;

    public string Transform(NotionBlockTransformContext context)
    {
        var codeBlock = BlockAccessor.GetOriginalBlock<CodeBlock>(context.CurrentBlock);
        var language = codeBlock.Code.Language;
        var text = MarkdownRichTextUtils.RichTextsToMarkdown(codeBlock.Code.RichText);

        return MarkdownBlockUtils.CodeBlock(text, language);
    }
} 
