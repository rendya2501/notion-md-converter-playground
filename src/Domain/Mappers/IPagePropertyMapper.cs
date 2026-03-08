using Notion.Client;
using NotionMarkdownConverter.Domain.Models;

namespace NotionMarkdownConverter.Domain.Mappers;

/// <summary>
/// Notionページのプロパティをドメインモデルに変換するマッパーのインターフェース
/// </summary>
public interface IPagePropertyMapper
{
    /// <summary>
    /// NotionページのプロパティをPagePropertyモデルにマッピングします。
    /// </summary>
    /// <param name="page">マッピング元のNotionページ</param>
    /// <returns>マッピング結果の<see cref="PageProperty"/></returns>
    PageProperty Map(Page page);
}
