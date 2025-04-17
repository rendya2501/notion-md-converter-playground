using Notion.Client;
using NotionMarkdownConverter.Core.Constants;
using NotionMarkdownConverter.Core.Enums;
using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Infrastructure.Notion.Clients;

/// <summary>
/// Notionのクライアントラッパー
/// </summary>
public class NotionClientWrapper(INotionClient _client) : INotionClientWrapper
{
    /// <summary>
    /// 公開用ページを取得します。
    /// </summary>
    /// <param name="databaseId"></param>
    /// <returns></returns>
    public async Task<List<Page>> GetPagesForPublishingAsync(string databaseId)
    {
        var allPages = new List<Page>();
        var filter = new SelectFilter(NotionPagePropertyNames.PublicStatusName, PublicStatus.Queued.ToString());
        string? nextCursor = null;

        do
        {
            var pagination = await _client.Databases.QueryAsync(databaseId, new DatabasesQueryParameters
            {
                Filter = filter,
                StartCursor = nextCursor
            });

            allPages.AddRange(pagination.Results.OfType<Page>());
            nextCursor = pagination.HasMore ? pagination.NextCursor : null;
        } while (nextCursor != null);

        return allPages;
    }

    /// <summary>
    /// ページのプロパティを更新します。
    /// </summary>
    /// <param name="pageId"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    public async Task UpdatePagePropertiesAsync(string pageId, DateTime now)
    {
        await _client.Pages.UpdateAsync(pageId, new PagesUpdateParameters
        {
            Properties = new Dictionary<string, PropertyValue>
            {
                [NotionPagePropertyNames.CrawledAtPropertyName] = new DatePropertyValue
                {
                    Date = new Date
                    {
                        Start = now
                    }
                },
                [NotionPagePropertyNames.PublicStatusName] = new SelectPropertyValue
                {
                    Select = new SelectOption
                    {
                        Name = PublicStatus.Published.ToString()
                    }
                }
            }
        });
    }

    /// <summary>
    /// ページの全内容を取得します。
    /// </summary>
    /// <param name="blockId"></param>
    /// <returns></returns>
    public async Task<List<NotionBlock>> GetPageFullContentAsync(string blockId)
    {
        // ページの全内容を取得するためのリスト
        List<NotionBlock> results = [];
        // 次のカーソル
        string? nextCursor = null;

        // ページの親要素を取得
        do
        {
            // ページの親要素を取得
            var pagination = await _client.Blocks.RetrieveChildrenAsync(
                new BlockRetrieveChildrenRequest
                {
                    BlockId = blockId,
                    StartCursor = nextCursor
                }
            );
            // ページの親要素を追加
            results.AddRange(pagination.Results.Select(s => new NotionBlock(s)));
            // 次のカーソルを更新
            nextCursor = pagination.HasMore ? pagination.NextCursor : null;
        } while (nextCursor != null);

        // ページの子要素を取得
        var tasks = results
            .Where(block => block.HasChildren)
            .Select(async block => block.Children = await GetPageFullContentAsync(block.Id))
            .ToList();

        await Task.WhenAll(tasks);
        return results;
    }
}