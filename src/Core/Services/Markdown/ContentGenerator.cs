using Microsoft.Extensions.Logging;
using NotionMarkdownConverter.Core.Models;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Transformer.Strategies;
using System.Text;

namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// コンテンツを生成するクラス
/// </summary>
public class ContentGenerator(
    IEnumerable<IBlockTransformStrategy> _strategies,
    IDefaultBlockTransformStrategy _defaultStrategy,
    ILogger<ContentGenerator> _logger) : IContentGenerator
{
    /// <summary>
    /// コンテンツを生成します
    /// </summary>
    /// <param name="blocks">変換するブロック</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string GenerateContent(List<NotionBlock> blocks)
    {
        if (blocks is null || blocks.Count == 0)
        {
            return string.Empty;
        }

        // 変換コンテキストを作成
        var context = new NotionBlockTransformState
        {
            ExecuteTransformBlocks = GenerateContent,
            Blocks = blocks,
            CurrentBlock = null!,
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
                ?? (IBlockTransformStrategy)_defaultStrategy;

            try
            {
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
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error transforming block at index : {index}", index);
                throw;
            }
        }

        return sb.ToString();
    }
}
