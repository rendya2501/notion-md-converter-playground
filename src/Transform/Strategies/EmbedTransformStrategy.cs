using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Context;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
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
        // 表示テキストの決定（キャプションが空の場合はURLを使用）
        var displayText = !string.IsNullOrEmpty(caption) ? caption : url;

        // リンクを生成し、最後に改行用スペースを追加
        return MarkdownInlineUtils.AppendTrailingSpaces(
            MarkdownInlineUtils.Link(displayText, url));
    }
}
