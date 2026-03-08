using Notion.Client;
using NotionMarkdownConverter.Application.Abstractions;
using NotionMarkdownConverter.Domain.Constants;
using NotionMarkdownConverter.Domain.Enums;
using NotionMarkdownConverter.Domain.Models;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Mappers;

/// <summary>
/// NotionページのプロパティをPagePropertyモデルに変換するマッパー
/// </summary>
public class PagePropertyMapper : IPagePropertyMapper
{
    /// <summary>
    /// NotionページのプロパティをPagePropertyモデルにマッピングします。
    /// </summary>
    /// <param name="page">マッピング元のNotionページ</param>
    /// <returns>マッピング結果の<see cref="PageProperty"/></returns>
    public PageProperty Map(Page page)
    {
        var pageProperty = new PageProperty { PageId = page.Id };

        foreach (var property in page.Properties)
        {
            // プロパティ名をキーにしてマッピング処理を振り分けます。
            // 未知のプロパティは無視します。
            switch (property.Key)
            {
                case NotionPagePropertyNames.PublishedAtPropertyName:
                    if (PropertyParser.TryParseAsDateTime(property.Value, out var publishedAt))
                        pageProperty.PublishedDateTime = publishedAt;
                    break;

                case NotionPagePropertyNames.CrawledAtPropertyName:
                    if (PropertyParser.TryParseAsDateTime(property.Value, out var crawledAt))
                        pageProperty.LastCrawledDateTime = crawledAt;
                    break;

                case NotionPagePropertyNames.SlugPropertyName:
                    if (PropertyParser.TryParseAsPlainText(property.Value, out var slug))
                        pageProperty.Slug = slug;
                    break;

                case NotionPagePropertyNames.TitlePropertyName:
                    if (PropertyParser.TryParseAsPlainText(property.Value, out var title))
                        pageProperty.Title = title;
                    break;

                case NotionPagePropertyNames.DescriptionPropertyName:
                    if (PropertyParser.TryParseAsPlainText(property.Value, out var description))
                        pageProperty.Description = description;
                    break;

                case NotionPagePropertyNames.TagsPropertyName:
                    if (PropertyParser.TryParseAsStringList(property.Value, out var tags))
                        pageProperty.Tags = tags;
                    break;

                case NotionPagePropertyNames.TypePropertyName:
                    if (PropertyParser.TryParseAsPlainText(property.Value, out var type))
                        pageProperty.Type = type;
                    break;

                case NotionPagePropertyNames.PublicStatusName:
                    if (PropertyParser.TryParseAsEnum<PublicStatus>(property.Value, out var publicStatus))
                        pageProperty.PublicStatus = publicStatus;
                    break;
            }
        }

        return pageProperty;
    }
}
