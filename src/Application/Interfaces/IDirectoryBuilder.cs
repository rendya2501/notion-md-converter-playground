using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Application.Interfaces;

/// <summary>
/// 出力ディレクトリ構築サービスのインターフェース
/// </summary>
public interface IDirectoryBuilder
{
    /// <summary>
    /// 出力ディレクトリを構築します。
    /// </summary>
    /// <param name="pageProperty"></param>
    /// <returns></returns>
    string Build(PageProperty pageProperty);
} 