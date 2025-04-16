using Notion.Client;
using NotionMarkdownConverter.Core.Constants;
using NotionMarkdownConverter.Core.Enums;
using NotionMarkdownConverter.Core.Models;
using NotionMarkdownConverter.Infrastructure.Notion.Parsers;

namespace NotionMarkdownConverter.Core.Mappers;

public class PagePropertyMapper
{
    /// <summary>
    /// ページのプロパティをコピーします。
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public static PageProperty CopyPageProperties(Page page)
    {
        var pageProperty = new PageProperty { PageId = page.Id };

        foreach (var property in page.Properties)
        {
            if (property.Key == NotionPagePropertyNames.PublishedAtPropertyName)
            {
                if (PropertyParser.TryParseAsDateTime(property.Value, out var publishedAt))
                {
                    pageProperty.PublishedDateTime = publishedAt;
                }
            }
            else if (property.Key == NotionPagePropertyNames.CrawledAtPropertyName)
            {
                if (PropertyParser.TryParseAsDateTime(property.Value, out var crawledAt))
                {
                    pageProperty.LastCrawledDateTime = crawledAt;
                }
            }
            else if (property.Key == NotionPagePropertyNames.SlugPropertyName)
            {
                if (PropertyParser.TryParseAsPlainText(property.Value, out var slug))
                {
                    pageProperty.Slug = slug;
                }
            }
            else if (property.Key == NotionPagePropertyNames.TitlePropertyName)
            {
                if (PropertyParser.TryParseAsPlainText(property.Value, out var title))
                {
                    pageProperty.Title = title;
                }
            }
            else if (property.Key == NotionPagePropertyNames.DescriptionPropertyName)
            {
                if (PropertyParser.TryParseAsPlainText(property.Value, out var description))
                {
                    pageProperty.Description = description;
                }
            }
            else if (property.Key == NotionPagePropertyNames.TagsPropertyName)
            {
                if (PropertyParser.TryParseAsStringList(property.Value, out var tags))
                {
                    pageProperty.Tags = tags;
                }
            }
            else if (property.Key == NotionPagePropertyNames.TypePropertyName)
            {
                if (PropertyParser.TryParseAsPlainText(property.Value, out var type))
                {
                    pageProperty.Type = type;
                }
            }
            else if (property.Key == NotionPagePropertyNames.PublicStatusName)
            {
                if (PropertyParser.TryParseAsEnum<PublicStatus>(property.Value, out var publicStatus))
                {
                    pageProperty.PublicStatus = publicStatus;
                }
            }
        }

        return pageProperty;
    }
}
