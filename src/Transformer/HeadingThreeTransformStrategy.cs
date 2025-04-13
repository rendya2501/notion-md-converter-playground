using Notion.Client;
using NotionMarkdownConverter.Infrastructure.Notion.Services;
using NotionMarkdownConverter.Models;
using NotionMarkdownConverter.Utils;

namespace NotionMarkdownConverter.Transformer;

/// <summary>
/// 見出し変換ストラテジー
/// </summary>
public class HeadingThreeTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Heading_3;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        // ブロックを取得
        var block = BlockConverter.GetOriginalBlock<HeadingThreeBlock>(context.CurrentBlock);
        // テキストを取得
        var text = MarkdownUtils.RichTextsToMarkdown(block.Heading_3.RichText);

        // 見出しを生成
        return MarkdownUtils.Heading(text, 3);
    }
}
