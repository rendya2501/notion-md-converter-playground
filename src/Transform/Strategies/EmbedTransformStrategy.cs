using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// 埋め込み変換ストラテジー
/// </summary>
public class EmbedTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Embed;

    public string Transform(NotionBlockTransformContext context)
    {
        // ブロックを埋め込みブロックに変換
        var originalBlock = BlockAccessor.GetOriginalBlock<EmbedBlock>(context.CurrentBlock);
        // 埋め込みのURLを取得
        var url = originalBlock.Embed.Url;
        // 埋め込みのキャプションをMarkdown形式に変換
        var caption = MarkdownRichTextUtils.RichTextsToMarkdown(originalBlock.Embed.Caption);

        // キャプションがある場合はリンク形式、ない場合はURLそのまま（Zennがリンクカードとして処理）
        if (!string.IsNullOrEmpty(caption))
            return MarkdownInlineUtils.AppendTrailingSpaces(
                MarkdownInlineUtils.Link(caption, url));

        return url;
    }
}
