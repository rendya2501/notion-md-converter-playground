using NotionMarkdownConverter.Domain.Models;

namespace NotionMarkdownConverter.Domain.Transformers.Context;

/// <summary>
/// ブロック変換処理のコンテキスト。
/// 現在処理中のブロック、ブロックリスト、子ブロック変換のデリゲートを保持します。
/// </summary>
public class NotionBlockTransformContext
{
    /// <summary>
    /// 子ブロックのリストをMarkdown文字列に変換するデリゲート。
    /// 子ブロックを持つストラテジーが再帰的に変換処理を呼び出すために使用します。
    /// </summary>
    public required Func<List<NotionBlock>, string> ExecuteTransformBlocks { get; set; }

    /// <summary>
    /// 変換対象のブロックリスト全体。
    /// 番号付きリストのカウンター計算など、前後のブロックを参照する処理で使用します。
    /// </summary>
    public required List<NotionBlock> Blocks { get; set; }

    /// <summary>
    /// 現在処理中のブロック。
    /// 各反復で <see cref="ContentConverter"/> によって更新されます。
    /// </summary>
    public required NotionBlock CurrentBlock { get; set; }

    /// <summary>
    /// <see cref="Blocks"/> 内における <see cref="CurrentBlock"/> のインデックス。
    /// </summary>
    public int CurrentBlockIndex { get; set; }
}
