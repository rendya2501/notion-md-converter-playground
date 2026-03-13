using NotionMarkdownConverter.Shared.Models;
using NotionMarkdownConverter.Transform.Context;

namespace NotionMarkdownConverter.Transform.Converters;

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
        if (blocks.Count == 0)
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
        // forループでインデックスを明示的に管理することで、
        // コンテキストへの副作用を伴う処理をLINQのSelectに混在させません。
        var results = new List<string>(blocks.Count);
        for (var i = 0; i < blocks.Count; i++)
        {
            context.CurrentBlock = blocks[i];
            context.CurrentBlockIndex = i;
            results.Add(_dispatcher.Transform(context));
        }

        // ブロック間を改行で結合して本文を組み立てます。
        // 空ブロックは""として変換され、改行として出力されます。
        return string.Join("\n", results);
    }
}
