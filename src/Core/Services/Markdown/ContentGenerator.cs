using NotionMarkdownConverter.Core.Models;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Transformer.Strategies;
using System.Text;

namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// コンテンツを生成するクラス
/// </summary>
public class ContentGenerator(IEnumerable<IBlockTransformStrategy> _strategies) : IContentGenerator
{
    /// <summary>
    /// コンテンツを生成します。
    /// </summary>
    /// <param name="blocks">変換するブロック</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string GenerateContentAsync(List<NotionBlock> blocks)
    {
        // 変換コンテキストを作成
        var context = new NotionBlockTransformState
        {
            ExecuteTransformBlocks = GenerateContentAsync,
            Blocks = blocks,
            CurrentBlock = blocks.First(),
            CurrentBlockIndex = 0
        };

        var sb = new StringBuilder();

        // ブロックを変換
        foreach (var (block, index) in blocks.Select((block, index) => (block, index)))
        {
            // 変換コンテキストを更新
            context.CurrentBlock = block;
            context.CurrentBlockIndex = index;

            // ブロックタイプに応じた変換ストラテジーを選択
            var strategy = _strategies.FirstOrDefault(s => s.BlockType == block.Type)
                ?? new DefaultTransformStrategy();

            // ブロックを変換
            var transformedBlock = strategy.Transform(context);

            // 変換されたブロックが存在する場合
            if (transformedBlock is not null)
            {
                if (sb.Length > 0)
                {
                    sb.Append('\n');
                }
                
                sb.Append(transformedBlock);
            }
        }

        return sb.ToString();
    }
}
