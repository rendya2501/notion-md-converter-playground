using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Core.Services.Markdown;

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
    string GenerateContent(List<NotionBlock> blocks);
}
