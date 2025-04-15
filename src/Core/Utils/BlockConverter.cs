using Notion.Client;
using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Core.Utils;

/// <summary>
/// ブロック変換サービス
/// </summary>
public static class BlockConverter
{
    /// <summary>
    /// ブロックのオリジナルを取得します。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="block"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException">指定された型にキャストできない場合</exception>
    public static T GetOriginalBlock<T>(NotionBlock block) where T : Block
    {
        if (block.OriginalBlock is not T originalBlock)
        {
            throw new InvalidCastException($"Cannot cast block of type {block.OriginalBlock.GetType().Name} to {typeof(T).Name}");
        }
        return originalBlock;
    }
} 