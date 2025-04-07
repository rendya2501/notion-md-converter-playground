using hoge.Models;
using System.Text;

namespace hoge.Services;

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
