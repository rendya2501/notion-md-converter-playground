using Notion.Client;
using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Application.Interfaces;

/// <summary>
/// Notionのクライアントラッパーインターフェース
/// </summary>
public interface INotionWrapperClient
{
    /// <summary>
    /// 公開用ページを取得します。
    /// </summary>
    /// <param name="databaseId">データベースID</param>
    /// <returns>公開用ページのリスト</returns>
    Task<List<Page>> GetPagesForPublishingAsync(string databaseId);

    /// <summary>
    /// ページのプロパティを更新します。
    /// </summary>
    /// <param name="pageId">ページID</param>
    /// <param name="now">現在日時</param>
    /// <returns></returns>
    Task UpdatePagePropertiesAsync(string pageId, DateTime now);

    /// <summary>
    /// ページの全内容を取得します。
    /// </summary>
    /// <param name="blockId">ブロックID</param>
    /// <returns>ページの全内容</returns>
    Task<List<NotionBlock>> GetPageFullContentAsync(string blockId);
} 