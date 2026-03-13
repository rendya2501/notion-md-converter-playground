using Notion.Client;
using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Shared.Utils;

/// <summary>
/// Notionブロックを具体的な型として取り出すユーティリティ
/// </summary>
public static class BlockAccessor
{
    /// <summary>
    /// Notionブロックを指定した型にキャストして返します。
    /// </summary>
    /// <typeparam name="T">キャスト先のブロック型</typeparam>
    /// <param name="block">キャスト対象のNotionブロック</param>
    /// <returns>指定した型にキャストされたブロック</returns>
    /// <exception cref="InvalidCastException">
    /// ブロックが指定された型にキャストできない場合にスローします。
    /// </exception>
    public static T GetOriginalBlock<T>(NotionBlock block) where T : Block
    {
        if (block.OriginalBlock is not T originalBlock)
        {
            throw new InvalidCastException(
                $"{block.OriginalBlock.GetType().Name}を{typeof(T).Name}にキャストできません。");
        }
        return originalBlock;
    }
}
