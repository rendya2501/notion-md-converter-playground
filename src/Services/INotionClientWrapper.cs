using hoge.Models;
using Notion.Client;

namespace hoge.Services;

/// <summary>
/// Notionクライアントラッパーのインターフェース
/// </summary>
public interface INotionClientWrapper
{
    /// <summary>
    /// 公開用ページを取得します。
    /// </summary>
    /// <param name="databaseId"></param>
    /// <returns></returns>
    Task<List<Page>> GetPagesForPublishingAsync(string databaseId);
    /// <summary>
    /// ページのプロパティをコピーします。
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    PageProperty CopyPageProperties(Page page);
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
    Task<List<NotionBlock>> GetPageFullContent(string blockId);
}
