using NotionMarkdownConverter.Core.Models;
using System.Text;

namespace NotionMarkdownConverter.Core.Services.Markdown;

/// <summary>
/// フロントマターを生成するインターフェース
/// </summary>
public interface IFrontmatterGenerator
{
    /// <summary>
    /// フロントマターを生成します。
    /// </summary>
    /// <param name="pageProperty"></param>
    /// <returns></returns>
    StringBuilder GenerateFrontmatter(PageProperty pageProperty);
}
