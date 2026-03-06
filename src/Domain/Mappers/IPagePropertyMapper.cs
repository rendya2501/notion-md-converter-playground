using Notion.Client;
using NotionMarkdownConverter.Domain.Models;

namespace NotionMarkdownConverter.Domain.Mappers;

/// <summary>
/// Notionページのプロパティをドメインモデルに変換するマッパーのインターフェース
/// </summary>
public interface IPagePropertyMapper
{
    /// <summary>
    /// Notionページのプロパティをコピーし、<see cref="PageProperty"/>に変換します。
    /// </summary>
    /// <param name="page">変換元のNotionページ</param>
    /// <returns>変換された<see cref="PageProperty"/></returns>
    PageProperty CopyPageProperties(Page page);
}
