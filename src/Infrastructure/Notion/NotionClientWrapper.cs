using Notion.Client;
using NotionMarkdownConverter.Shared.Constants;
using NotionMarkdownConverter.Shared.Enums;
using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Infrastructure.Notion;

/// <summary>
/// Notion APIクライアントのラッパー。
/// ページネーションの処理や再帰的なブロック取得を隠蔽します。
/// </summary>
public class NotionClientWrapper(INotionClient _client) : INotionPageReader, INotionPageWriter
{
    /// <summary>
    /// 公開待ちステータスのページ一覧を取得します。
    /// ページネーションを処理し、すべてのページを返します。
    /// </summary>
    /// <param name="databaseId">取得対象のNotionデータベースID</param>
    /// <returns>公開待ちページのリスト</returns>
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
    /// エクスポート完了後にページのプロパティを更新します。
    /// クロール日時を記録し、公開ステータスを公開済みに変更します。
    /// </summary>
    /// <param name="pageId">更新対象のページID</param>
    /// <param name="now">クロール日時として記録する日時（UTC）</param>
    public async Task UpdatePagePropertiesAsync(string pageId, DateTime now)
    {
        await _client.Pages.UpdateAsync(pageId, new PagesUpdateParameters
        {
            Properties = new Dictionary<string, PropertyValue>
            {
                [NotionPagePropertyNames.CrawledAtPropertyName] = new DatePropertyValue
                {
                    Date = new Date { Start = now }
                },
                [NotionPagePropertyNames.PublicStatusName] = new SelectPropertyValue
                {
                    Select = new SelectOption { Name = PublicStatus.Published.ToString() }
                }
            }
        });
    }

    /// <summary>
    /// 指定ブロックの子ブロックをすべて再帰的に取得し、ツリー構造として返します。
    /// ページネーションを処理し、子ブロックを持つブロックは並列で再帰取得します。
    /// </summary>
    /// <param name="blockId">取得起点となるブロックID（ページIDも可）</param>
    /// <returns>子ブロックを再帰的に展開したブロックのリスト</returns>
    public async Task<List<NotionBlock>> FetchBlockTreeAsync(string blockId)
    {
        var blocks = new List<NotionBlock>();
        string? nextCursor = null;

        // ページネーションを処理しながら直下の子ブロックをすべて取得します。
        do
        {
            var pagination = await _client.Blocks.RetrieveChildrenAsync(
                new BlockRetrieveChildrenRequest
                {
                    BlockId = blockId,
                    StartCursor = nextCursor
                }
            );

            blocks.AddRange(pagination.Results.Select(s => new NotionBlock(s)));
            nextCursor = pagination.HasMore ? pagination.NextCursor : null;
        } while (nextCursor != null);

        // 子ブロックを持つブロックを並列で再帰取得します。
        // Task.WhenAllで並列化することでAPI呼び出しの待機時間を短縮します。
        var tasks = blocks
            .Where(block => block.HasChildren)
            .Select(async block => block.Children = await FetchBlockTreeAsync(block.Id))
            .ToList();

        await Task.WhenAll(tasks);

        return blocks;
    }
}
