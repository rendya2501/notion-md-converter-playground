using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// リンクプレビュー変換ストラテジー
/// </summary>
public class LinkPreviewTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.LinkPreview;

    public string Transform(NotionBlockTransformContext context)
    {
        var linkPreview = BlockAccessor.GetOriginalBlock<LinkPreviewBlock>(context.CurrentBlock);
        // URLそのまま出力（Zennがリンクカードとして処理）
        return linkPreview.LinkPreview.Url;
    }
}
