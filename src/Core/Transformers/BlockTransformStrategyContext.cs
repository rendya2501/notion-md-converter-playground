using Notion.Client;
using NotionMarkdownConverter.Core.Transformers.States;
using NotionMarkdownConverter.Core.Transformers.Strategies;

namespace NotionMarkdownConverter.Core.Transformers;

/// <summary>
/// ブロック変換ストラテジーコンテキスト
/// </summary>
/// <param name="strategies">ブロック変換ストラテジー</param>
/// <param name="defaultStrategy">デフォルトブロック変換ストラテジー</param>
public class BlockTransformStrategyContext(
    IEnumerable<IBlockTransformStrategy> strategies,
    IDefaultBlockTransformStrategy defaultStrategy)
{
    /// <summary>
    /// ブロック変換ストラテジーのキャッシュ
    /// </summary>
    private readonly Dictionary<BlockType, IBlockTransformStrategy> _cache = strategies.ToDictionary(s => s.BlockType);

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformState context)
    {
        var strategy = _cache.GetValueOrDefault(
            context.CurrentBlock.Type, defaultStrategy);
        return strategy.Transform(context);
    }
}