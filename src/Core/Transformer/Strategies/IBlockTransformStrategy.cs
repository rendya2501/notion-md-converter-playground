using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

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
    Task<string> TransformAsync(NotionBlockTransformState context);
} 