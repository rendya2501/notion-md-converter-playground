using Notion.Client;
using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Infrastructure.Notion.Clients;

/// <summary>
/// Notionのクライアントラッパー
/// </summary>
public interface INotionWrapperClient
{
    /// <summary>
    /// 公開用ページを取得します。
    /// </summary>
    /// <param name="databaseId"></param>
    /// <returns></returns>
    Task<List<Page>> GetPagesForPublishingAsync(string databaseId);

    /// <summary>
    /// ページのプロパティを更新します。
    /// </summary>
    /// <param name="pageId"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    Task UpdatePagePropertiesAsync(string pageId, DateTime now);

    /// <summary>
    /// ページの全内容を取得します。
    /// </summary>
    /// <param name="blockId"></param>
    /// <returns></returns>
    Task<List<NotionBlock>> GetPageFullContentAsync(string blockId);
} 