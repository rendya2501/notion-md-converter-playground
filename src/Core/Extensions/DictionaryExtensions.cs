namespace NotionMarkdownConverter.Core.Extensions;

/// <summary>
/// Dictionary拡張
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// IDictionaryにGetValueOrDefaultメソッドを追加
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static TValue GetValueOrDefault<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue defaultValue = default!)
    {
        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
