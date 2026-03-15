using Notion.Client;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;

namespace NotionMarkdownConverter.Transform;

/// <summary>
/// ブロックタイプに応じた変換ストラテジーを選択し、変換処理を実行するディスパッチャー
/// </summary>
/// <param name="_strategies">ブロック変換ストラテジーのコレクション</param>
/// <param name="_defaultStrategy">デフォルトのブロック変換ストラテジー</param>
/// <remarks>
/// 登録された <see cref="IBlockTransformStrategy"/> の中からブロックタイプに対応するものを選択します。
/// 対応するストラテジーが存在しない場合は <see cref="IDefaultBlockTransformStrategy"/> にフォールバックします。
/// </remarks>
public class BlockTransformDispatcher(
    IEnumerable<IBlockTransformStrategy> _strategies,
    IDefaultBlockTransformStrategy _defaultStrategy)
{
    /// <summary>
    /// ブロック変換ストラテジーのキャッシュ
    /// </summary>
    private readonly Dictionary<BlockType, IBlockTransformStrategy> _cache = _strategies.ToDictionary(s => s.BlockType);

    /// <summary>
    /// ブロックタイプに対応するストラテジーを選択し、変換処理を実行します。
    /// </summary>
    /// <param name="context">変換対象のブロックと変換に必要な情報を持つコンテキスト</param>
    /// <returns>変換されたMarkdown文字列。対応するストラテジーが存在しない場合は空文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        var strategy = _cache.GetValueOrDefault(context.CurrentBlock.Type, _defaultStrategy);
        return strategy.Transform(context);
    }
}
