using Notion.Client;

public interface INotionService
{
    Task<IEnumerable<Page>> GetPagesAsync(string databaseId, CheckboxFilter filter);
    Task UpdatePagePropertiesAsync(string pageId, Dictionary<string, PropertyValue> properties);
}

public class NotionService : INotionService
{
    private readonly NotionClient client;

    public NotionService(NotionClient client)
    {
        this.client = client;
    }

    public async Task<IEnumerable<Page>> GetPagesAsync(string databaseId, CheckboxFilter filter)
    {
        var pagination = await client.Databases.QueryAsync(databaseId, new DatabasesQueryParameters { Filter = filter });
        var pages = new List<Page>();

        do
        {
            pages.AddRange(pagination.Results);
            if (!pagination.HasMore) break;
            pagination = await client.Databases.QueryAsync(databaseId, new DatabasesQueryParameters { Filter = filter, StartCursor = pagination.NextCursor });
        } while (true);

        return pages;
    }

    public async Task UpdatePagePropertiesAsync(string pageId, Dictionary<string, PropertyValue> properties)
    {
        await client.Pages.UpdateAsync(pageId, new PagesUpdateParameters { Properties = properties });
    }
}
