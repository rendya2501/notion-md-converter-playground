using NotionMarkdownConverter.Domain.Models;
using NotionMarkdownConverter.Domain.Transformers;
using NotionMarkdownConverter.Domain.Transformers.Context;

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
        // ブロックが空の場合は早期リターン
        if (blocks is null || blocks.Count == 0)
        {
            return string.Empty;
        }

        // 変換コンテキストを作成
        // ExecuteTransformBlocks に自身を渡すことで、子ブロックを持つストラテジーが
        // 再帰的にこのメソッドを呼び出せるようにしています。
        var context = new NotionBlockTransformContext
        {
            ExecuteTransformBlocks = Convert,
            Blocks = blocks,
            CurrentBlock = null!,
            CurrentBlockIndex = 0
        };

        // 各ブロックをMarkdown文字列に変換します。
        // コンテキストを使い回すことで、ストラテジー側が前後のブロック情報を
        // 参照できるようにしています（番号付きリストの連番管理など）。
        // 空ブロックは "" として変換され、改行として出力されます。
        var results = blocks.Select((block, index) =>
        {
            context.CurrentBlock = block;
            context.CurrentBlockIndex = index;
            return _dispatcher.Transform(context);
        });

        // ブロック間を改行で結合して本文を組み立てます。
        return string.Join("\n", results);
    }
}
