using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Shared.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// リンクプレビュー変換ストラテジー
/// </summary>
public class LinkPreviewTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.LinkPreview;

    public string Transform(NotionBlockTransformContext context)
    {
        var linkPreview = BlockAccessor.GetOriginalBlock<LinkPreviewBlock>(context.CurrentBlock);
        var url = linkPreview.LinkPreview.Url;
        
        return MarkdownInlineUtils.Link(url,url);
    }
}