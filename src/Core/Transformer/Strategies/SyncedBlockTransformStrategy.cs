using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// 同期ブロック変換ストラテジー
/// </summary>
public class SyncedBlockTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.SyncedBlock;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public Task<string> TransformAsync(NotionBlockTransformState context)
    {
        return Task.FromResult(string.Empty);
    }
}