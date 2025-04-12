using Notion.Client;
using NotionMarkdownConverter.Models;
using NotionMarkdownConverter.Utils;

namespace NotionMarkdownConverter.Transformer.Strategies;

/// <summary>
/// ブックマーク変換ストラテジー
/// </summary>
public class BookmarkTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Bookmark;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        // ブロックをブックマークブロックに変換
        var originalBlock = context.CurrentBlock.GetOriginalBlock<BookmarkBlock>();
        // // ブックマークブロックが存在しない場合は空文字を返す
        // if (originalBlock is not BookmarkBlock bookmarkBlock || string.IsNullOrEmpty(bookmarkBlock.Bookmark.Url))
        // {
        //     return string.Empty;
        // }
        // ブックマークのURLを取得
        var url = originalBlock.Bookmark.Url;
        // ブックマークのキャプションをMarkdown形式に変換
        var caption = MarkdownUtils.RichTextsToMarkdown(originalBlock.Bookmark.Caption);
        // ブックマークのキャプションが空の場合はURLを表示する
        var displayText = !string.IsNullOrEmpty(caption) ? caption : url;
    
        // リンクを生成し、最後に改行用スペースを追加
        return MarkdownUtils.WithLineBreak(
            MarkdownUtils.Link(displayText, url));
    }
}