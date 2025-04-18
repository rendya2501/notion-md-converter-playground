using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

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
    public string Transform(NotionBlockTransformState context)
    {
        // ブロックをブックマークブロックに変換
        var originalBlock = BlockConverter.GetOriginalBlock<BookmarkBlock>(context.CurrentBlock);
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