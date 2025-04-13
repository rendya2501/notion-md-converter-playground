using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Infrastructure.FileSystem.Services;

/// <summary>
/// 出力ディレクトリ構築サービスのインターフェース
/// </summary>
public interface IOutputDirectoryBuilder
{
    /// <summary>
    /// 出力ディレクトリを構築します。
    /// </summary>
    /// <param name="pageProperty"></param>
    /// <returns></returns>
    string Build(PageProperty pageProperty);
} 