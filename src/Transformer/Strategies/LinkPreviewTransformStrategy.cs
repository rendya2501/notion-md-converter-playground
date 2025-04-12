using Notion.Client;
using NotionMarkdownConverter.Models;

namespace NotionMarkdownConverter.Transformer.Strategies;

/// <summary>
/// リンクプレビュー変換ストラテジー
/// </summary>
public class LinkPreviewTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.LinkPreview;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        return string.Empty;
    }
}