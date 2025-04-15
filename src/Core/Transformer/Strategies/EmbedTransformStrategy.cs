using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// 埋め込み変換ストラテジー
/// </summary>
public class EmbedTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Embed;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformState context)
    {
        // ブロックを埋め込みブロックに変換
        var originalBlock = BlockConverter.GetOriginalBlock<EmbedBlock>(context.CurrentBlock);
        // 埋め込みのURLを取得
        var url = originalBlock.Embed.Url;
        // 埋め込みのキャプションをMarkdown形式に変換
        var caption = MarkdownUtils.RichTextsToMarkdown(originalBlock.Embed.Caption);
        // 表示テキストの決定（キャプションが空の場合はURLを使用）
        var displayText = !string.IsNullOrEmpty(caption) ? caption : url;

        // リンクを生成し、最後に改行用スペースを追加
        return MarkdownUtils.WithLineBreak(
            MarkdownUtils.Link(displayText, url));
    }
}
