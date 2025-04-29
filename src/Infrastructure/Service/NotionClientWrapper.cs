using Microsoft.Extensions.Logging;
using Notion.Client;
using NotionMarkdownConverter.Application.Interfaces;
using NotionMarkdownConverter.Core.Constants;
using NotionMarkdownConverter.Core.Enums;
using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Infrastructure.Service;

/// <summary>
/// Notionのクライアントラッパー
/// </summary>
public class NotionClientWrapper(INotionClient _client) : INotionWrapperClient
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


///// <summary>
///// Notionのクライアントラッパー
///// </summary>
//public class NotionClientWrapper : INotionWrapperClient
//{
//    private readonly INotionClient _client;
//    private readonly ILogger<NotionClientWrapper> _logger;
//    private const int MaxRetryCount = 3;
//    private const int RetryDelayMilliseconds = 1000;

//    public NotionClientWrapper(
//        INotionClient client,
//        ILogger<NotionClientWrapper> logger)
//    {
//        _client = client;
//        _logger = logger;
//    }

//    /// <summary>
//    /// 公開用ページを取得します。
//    /// </summary>
//    /// <param name="databaseId"></param>
//    /// <returns></returns>
//    public async Task<List<Page>> GetPagesForPublishingAsync(string databaseId)
//    {
//        try
//        {
//            _logger.LogInformation("Getting pages for publishing from database {DatabaseId}", databaseId);

//            var allPages = new List<Page>();
//            var filter = new SelectFilter(NotionPagePropertyNames.PublicStatusName, PublicStatus.Queued.ToString());
//            string? nextCursor = null;

//            do
//            {
//                var pagination = await ExecuteWithRetryAsync(() =>
//                    _client.Databases.QueryAsync(databaseId, new DatabasesQueryParameters
//                    {
//                        Filter = filter,
//                        StartCursor = nextCursor
//                    }));

//                allPages.AddRange(pagination.Results.OfType<Page>());
//                nextCursor = pagination.HasMore ? pagination.NextCursor : null;
//            } while (nextCursor != null);

//            _logger.LogInformation("Retrieved {Count} pages for publishing", allPages.Count);
//            return allPages;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to get pages for publishing from database {DatabaseId}", databaseId);
//            throw new Core.Exceptions.NotionApiException($"Failed to get pages for publishing from database {databaseId}", ex);
//        }
//    }

//    /// <summary>
//    /// ページのプロパティを更新します。
//    /// </summary>
//    /// <param name="pageId"></param>
//    /// <param name="now"></param>
//    /// <returns></returns>
//    public async Task UpdatePagePropertiesAsync(string pageId, DateTime now)
//    {
//        try
//        {
//            _logger.LogInformation("Updating page properties for page {PageId}", pageId);

//            await ExecuteWithRetryAsync(() => _client.Pages.UpdateAsync(pageId, new PagesUpdateParameters
//            {
//                Properties = new Dictionary<string, PropertyValue>
//                {
//                    [NotionPagePropertyNames.CrawledAtPropertyName] = new DatePropertyValue
//                    {
//                        Date = new Date
//                        {
//                            Start = now
//                        }
//                    },
//                    [NotionPagePropertyNames.PublicStatusName] = new SelectPropertyValue
//                    {
//                        Select = new SelectOption
//                        {
//                            Name = PublicStatus.Published.ToString()
//                        }
//                    }
//                }
//            }));

//            _logger.LogInformation("Successfully updated page properties for page {PageId}", pageId);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to update page properties for page {PageId}", pageId);
//            throw new Core.Exceptions.NotionApiException($"Failed to update page properties for page {pageId}", ex);
//        }
//    }

//    /// <summary>
//    /// ページの全内容を取得します。
//    /// </summary>
//    /// <param name="blockId"></param>
//    /// <returns></returns>
//    public async Task<List<NotionBlock>> GetPageFullContentAsync(string blockId)
//    {
//        try
//        {
//            _logger.LogInformation("Getting full content for block {BlockId}", blockId);

//            List<NotionBlock> results = [];
//            string? nextCursor = null;

//            do
//            {
//                var pagination = await ExecuteWithRetryAsync(() =>
//                    _client.Blocks.RetrieveChildrenAsync(new BlockRetrieveChildrenRequest
//                    {
//                        BlockId = blockId,
//                        StartCursor = nextCursor
//                    }));

//                results.AddRange(pagination.Results.Select(s => new NotionBlock(s)));
//                nextCursor = pagination.HasMore ? pagination.NextCursor : null;
//            } while (nextCursor != null);

//            var tasks = results
//                .Where(block => block.HasChildren)
//                .Select(async block => block.Children = await GetPageFullContentAsync(block.Id))
//                .ToList();

//            await Task.WhenAll(tasks);

//            _logger.LogInformation("Successfully retrieved full content for block {BlockId}", blockId);
//            return results;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to get full content for block {BlockId}", blockId);
//            throw new Core.Exceptions.NotionApiException($"Failed to get full content for block {blockId}", ex);
//        }
//    }

//    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
//    {
//        var retryCount = 0;
//        while (true)
//        {
//            try
//            {
//                return await operation();
//            }
//            catch (Exception ex) when (retryCount < MaxRetryCount)
//            {
//                retryCount++;
//                _logger.LogWarning(ex, "Retry {RetryCount}/{MaxRetryCount} after {Delay}ms",
//                    retryCount, MaxRetryCount, RetryDelayMilliseconds);
//                await Task.Delay(RetryDelayMilliseconds);
//            }
//        }
//    }
//}
