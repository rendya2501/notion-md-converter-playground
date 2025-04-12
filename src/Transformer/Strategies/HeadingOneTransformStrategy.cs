using Notion.Client;
using NotionMarkdownConverter.Models;
using NotionMarkdownConverter.Utils;

namespace NotionMarkdownConverter.Transformer.Strategies;

/// <summary>
/// 見出し変換ストラテジー
/// </summary>
public class HeadingOneTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Heading_1;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        // ブロックを取得
        var block = context.CurrentBlock.GetOriginalBlock<HeadingOneBlock>();
        // テキストを取得
        var text = MarkdownUtils.RichTextsToMarkdown(block.Heading_1.RichText);

        // 見出しを生成
        return MarkdownUtils.Heading(text, 1);
    }
} 