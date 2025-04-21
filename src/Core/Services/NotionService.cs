using Notion.Client;
using NotionMarkdownConverter.Core.Interfaces;
using NotionMarkdownConverter.Core.Models;
using NotionMarkdownConverter.Core.Utils;
using NotionMarkdownConverter.Infrastructure.Notion.Clients;

namespace NotionMarkdownConverter.Core.Services
{
    public class NotionService(Infrastructure.Notion.Clients.INotionWrapperClient notionClient) : Interfaces.INotionService
    {
        public async Task<NotionBlock> GetBlockAsync(string blockId)
        {
            var blocks = await notionClient.GetPageFullContentAsync(blockId);
            return blocks.FirstOrDefault() ?? throw new InvalidOperationException($"Block not found: {blockId}");
        }

        public async Task<PageProperty> GetPagePropertiesAsync(string pageId)
        {
            var blocks = await notionClient.GetPageFullContentAsync(pageId);
            var firstBlock = blocks.FirstOrDefault() ?? throw new InvalidOperationException($"Page not found: {pageId}");
            
            return new PageProperty
            {
                PageId = firstBlock.Id,
                Type = firstBlock.Type.ToString(),
                // 他のプロパティは後で設定
            };
        }

        public async Task<UrlFilePair> ProcessBlockContentAsync(NotionBlock block)
        {
            // ブロックの内容を処理するロジックを実装
            string url = string.Empty;
            
            // ブロックタイプに応じてURLを取得
            switch (block.Type)
            {
                case BlockType.Image:
                    var imageBlock = BlockConverter.GetOriginalBlock<ImageBlock>(block);
                    url = imageBlock.Image switch
                    {
                        ExternalFile externalFile => externalFile.External.Url,
                        UploadedFile uploadedFile => uploadedFile.File.Url,
                        _ => string.Empty
                    };
                    break;
                case BlockType.File:
                    var fileBlock = BlockConverter.GetOriginalBlock<FileBlock>(block);
                    url = fileBlock.File switch
                    {
                        ExternalFile externalFile => externalFile.External.Url,
                        UploadedFile uploadedFile => uploadedFile.File.Url,
                        _ => string.Empty
                    };
                    break;
                case BlockType.Video:
                    var videoBlock = BlockConverter.GetOriginalBlock<VideoBlock>(block);
                    url = videoBlock.Video switch
                    {
                        ExternalFile externalFile => externalFile.External.Url,
                        UploadedFile uploadedFile => uploadedFile.File.Url,
                        _ => string.Empty
                    };
                    break;
            }

            return new UrlFilePair(
                OriginalUrl: url,
                ConversionFileName: $"{block.Id}.md"
            );
        }
    }
} 