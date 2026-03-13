using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// ブックマーク変換ストラテジー
/// </summary>
public class BookmarkTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Bookmark;

    public string Transform(NotionBlockTransformContext context)
    {
        // ブロックをブックマークブロックに変換
        var originalBlock = BlockAccessor.GetOriginalBlock<BookmarkBlock>(context.CurrentBlock);
        // ブックマークのURLを取得
        var url = originalBlock.Bookmark.Url;
        // ブックマークのキャプションをMarkdown形式に変換
        var caption = MarkdownRichTextUtils.RichTextsToMarkdown(originalBlock.Bookmark.Caption);
        // ブックマークのキャプションが空の場合はURLを表示する
        var displayText = !string.IsNullOrEmpty(caption) ? caption : url;
    
        // リンクを生成し、最後に改行用スペースを追加
        return MarkdownInlineUtils.AppendTrailingSpaces(
            MarkdownInlineUtils.Link(displayText, url));
    }
}