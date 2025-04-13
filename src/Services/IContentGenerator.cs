using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Services;

/// <summary>
/// コンテンツを生成するインターフェース
/// </summary>
public interface IContentGenerator
{
    /// <summary>
    /// コンテンツを生成します。
    /// </summary>
    /// <param name="blocks"></param>
    /// <returns></returns>
    string GenerateContentAsync(List<NotionBlock> blocks);
}
