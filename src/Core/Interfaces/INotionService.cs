using NotionMarkdownConverter.Core.Models;

namespace NotionMarkdownConverter.Core.Interfaces
{
    public interface INotionService
    {
        Task<NotionBlock> GetBlockAsync(string blockId);
        Task<PageProperty> GetPagePropertiesAsync(string pageId);
        Task<UrlFilePair> ProcessBlockContentAsync(NotionBlock block);
    }
} 