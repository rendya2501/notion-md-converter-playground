using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Core.Transformer.State;

/// <summary>
/// ブロック変換の状態
/// </summary>
public class NotionBlockTransformState
{
    /// <summary>
    /// ブロックを変換する
    /// </summary>
    /// <value></value>
    public required Func<List<NotionBlock>, Task<string>> GenerateContentAsync { get; set; }

    /// <summary>
    /// ブロックのリスト
    /// </summary>
    /// <value></value>
    public required List<NotionBlock> Blocks { get; set; }

    /// <summary>
    /// 現在のブロック
    /// </summary>
    /// <value></value>
    public required NotionBlock CurrentBlock { get; set; }

    /// <summary>
    /// 現在のブロックのインデックス
    /// </summary>
    /// <value></value>
    public int CurrentBlockIndex { get; set; }
}
