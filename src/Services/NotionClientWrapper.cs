using hoge.Models;
using hoge.Utils;
using Notion.Client;

namespace hoge.Services;

/// <summary>
/// Notionのクライアントラッパー
/// </summary>
public class NotionClientWrapper(INotionClient _client) : INotionClientWrapper
{
    private const string TitlePropertyName = "Title";
    private const string TypePropertyName = "Type";
    private const string PublishedAtPropertyName = "PublishedAt";
    private const string CrawledAtPropertyName = "_SystemCrawledAt";
    private const string TagsPropertyName = "Tags";
    private const string DescriptionPropertyName = "Description";
    private const string SlugPropertyName = "Slug";
    private const string PublicStatusName = "PublicStatus";

    /// <summary>
    /// 公開用ページを取得します。
    /// </summary>
    /// <param name="databaseId"></param>
    /// <returns></returns>
    public async Task<List<Page>> GetPagesForPublishingAsync(string databaseId)
    {
        var allPages = new List<Page>();
        var filter = new SelectFilter(PublicStatusName, PublicStatus.Queued.ToString());
        string? nextCursor = null;

        do
        {
            var pagination = await _client.Databases.QueryAsync(databaseId, new DatabasesQueryParameters
            {
                Filter = filter,
                StartCursor = nextCursor
            });

            allPages.AddRange(pagination.Results);
            nextCursor = pagination.HasMore ? pagination.NextCursor : null;
        } while (nextCursor != null);


        return allPages;
    }

    /// <summary>
    /// ページのプロパティをコピーします。
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public PageProperty CopyPageProperties(Page page)
    {
        var pageProperty = new PageProperty { PageId = page.Id };

        foreach (var property in page.Properties)
        {
            if (property.Key == PublishedAtPropertyName)
            {
                if (PropertyParser.TryParseAsDateTime(property.Value, out var publishedAt))
                {
                    pageProperty.PublishedDateTime = publishedAt;
                }
            }
            else if (property.Key == CrawledAtPropertyName)
            {
                if (PropertyParser.TryParseAsDateTime(property.Value, out var crawledAt))
                {
                    pageProperty.LastCrawledDateTime = crawledAt;
                }
            }
            else if (property.Key == SlugPropertyName)
            {
                if (PropertyParser.TryParseAsPlainText(property.Value, out var slug))
                {
                    pageProperty.Slug = slug;
                }
            }
            else if (property.Key == TitlePropertyName)
            {
                if (PropertyParser.TryParseAsPlainText(property.Value, out var title))
                {
                    pageProperty.Title = title;
                }
            }
            else if (property.Key == DescriptionPropertyName)
            {
                if (PropertyParser.TryParseAsPlainText(property.Value, out var description))
                {
                    pageProperty.Description = description;
                }
            }
            else if (property.Key == TagsPropertyName)
            {
                if (PropertyParser.TryParseAsStringList(property.Value, out var tags))
                {
                    pageProperty.Tags = tags;
                }
            }
            else if (property.Key == TypePropertyName)
            {
                if (PropertyParser.TryParseAsPlainText(property.Value, out var type))
                {
                    pageProperty.Type = type;
                }
            }
            else if (property.Key == PublicStatusName)
            {
                if (PropertyParser.TryParseAsEnum<PublicStatus>(property.Value, out var publicStatus))
                {
                    pageProperty.PublicStatus = publicStatus;
                }
            }
        }

        return pageProperty;
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
                [CrawledAtPropertyName] = new DatePropertyValue
                {
                    Date = new Date
                    {
                        Start = now
                    }
                },
                [PublicStatusName] = new SelectPropertyValue
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
    public async Task<List<NotionBlock>> GetPageFullContent(string blockId)
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
                blockId,
                new BlocksRetrieveChildrenParameters
                {
                    StartCursor = nextCursor
                }
            );
            // ページの親要素を追加
            results.AddRange(pagination.Results.Cast<Block>().Select(s => new NotionBlock(s)));
            // 次のカーソルを更新
            nextCursor = pagination.HasMore ? pagination.NextCursor : null;
        } while (nextCursor != null);

        // ページの子要素を取得
        var tasks = results
            .Where(block => block.HasChildren)
            .Select(async block => block.Children = await GetPageFullContent(block.Id))
            .ToList();

        await Task.WhenAll(tasks);
        return results;
    }
}


/// <summary>
/// 公開ステータス
/// </summary>
public enum PublicStatus
{
    /// <summary>
    /// 公開済み
    /// </summary>
    Published,
    /// <summary>
    /// 公開待ち
    /// </summary>
    Queued,
    /// <summary>
    /// 非公開
    /// </summary>
    Unpublished
}