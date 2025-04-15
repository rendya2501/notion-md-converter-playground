using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// 見出し変換ストラテジー
/// </summary>
public class HeadingTwoTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Heading_2;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformState context)
    {
        // ブロックを取得
        var block = BlockConverter.GetOriginalBlock<HeadingTwoBlock>(context.CurrentBlock);
        // テキストを取得
        var text = MarkdownUtils.RichTextsToMarkdown(block.Heading_2.RichText);

        // 見出しを生成
        return MarkdownUtils.Heading(text, 2);
    }
} 