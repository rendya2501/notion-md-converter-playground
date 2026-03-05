using Microsoft.Extensions.Logging;
using NotionMarkdownConverter.Core.Models;
using NotionMarkdownConverter.Core.Transformers;
using NotionMarkdownConverter.Core.Transformers.States;
using System.Text;

namespace NotionMarkdownConverter.Core.Markdown.Converters;

/// <summary>
/// コンテンツを生成するクラス
/// </summary>
public class ContentGenerator(
    BlockTransformDispatcher strategyContext,
    ILogger<ContentGenerator> logger)
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
        var context = new NotionBlockTransformContext
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

            try
            {
                // ブロックを変換
                var transformedBlock = strategyContext.Transform(context);

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
                logger.LogWarning(ex, "Error transforming block at index : {index}", index);
                throw;
            }
        }

        return sb.ToString();
    }
}
