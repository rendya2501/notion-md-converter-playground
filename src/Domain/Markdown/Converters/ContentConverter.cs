using NotionMarkdownConverter.Domain.Models;
using NotionMarkdownConverter.Domain.Transformers;
using NotionMarkdownConverter.Domain.Transformers.Context;
using System.Text;

namespace NotionMarkdownConverter.Domain.Markdown.Converters;

/// <summary>
/// Notionページのブロック列をMarkdown本文に変換します。
/// </summary>
public class ContentConverter(BlockTransformDispatcher _dispatcher)
{
    /// <summary>
    /// Markdown本文に変換します。
    /// </summary>
    /// <param name="blocks">変換するブロック</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Convert(List<NotionBlock> blocks)
    {
        if (blocks is null || blocks.Count == 0)
        {
            return string.Empty;
        }

        // 変換コンテキストを作成
        var context = new NotionBlockTransformContext
        {
            ExecuteTransformBlocks = Convert,
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

            // ブロックを変換
            var transformedBlock = _dispatcher.Transform(context);

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
