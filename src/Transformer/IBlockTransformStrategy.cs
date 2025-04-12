using Notion.Client;
using NotionMarkdownConverter.Models;

namespace NotionMarkdownConverter.Transformer;

/// <summary>
/// ブロック変換ストラテジーのインターフェース
/// </summary>
public interface IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    BlockType BlockType { get; }

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    string Transform(NotionBlockTransformContext context);
} 