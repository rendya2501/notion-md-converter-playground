using Notion.Client;
using System.Security.Cryptography;
using System.Text;

public interface IMarkdownExporter
{
    Task<bool> ExportPageToMarkdownAsync(Page page, DateTime now, bool forceExport = false);
}

public class MarkdownExporter : IMarkdownExporter
{
    private readonly NotionClient client;
    private readonly IPropertyParser2 propertyParser;
    private readonly string outputDirectoryPathTemplate;

    private const string notionTitlePropertyName = "Title";
    private const string notionTypePropertyName = "Type";
    private const string notionPublishedAtPropertyName = "PublishedAt";
    private const string notionRequestPublisingPropertyName = "RequestPublishing";
    private const string notionCrawledAtPropertyName = "_SystemCrawledAt";
    private const string notionTagsPropertyName = "Tags";
    private const string notionDescriptionPropertyName = "Description";
    private const string notionSlugPropertyName = "Slug";

    private const string frontMatterTitleName = "title";
    private const string frontMatterTypeName = "type";
    private const string frontMatterPublishedName = "date";
    private const string frontMatterDescriptionName = "description";
    private const string frontMatterTagsName = "tags";
    private const string frontMatterEyecatch = "eyecatch";

    public MarkdownExporter(NotionClient client, IPropertyParser2 propertyParser, string outputDirectoryPathTemplate)
    {
        this.client = client;
        this.propertyParser = propertyParser;
        this.outputDirectoryPathTemplate = outputDirectoryPathTemplate;
    }

    public async Task<bool> ExportPageToMarkdownAsync(Page page, DateTime now, bool forceExport = false)
    {
        bool requestPublishing = false;
        string title = string.Empty;
        string type = string.Empty;
        string slug = page.Id;
        string description = string.Empty;
        List<string>? tags = null;
        DateTime? publishedDateTime = null;
        DateTime? lastSystemCrawledDateTime = null;

        // frontmatterを構築
        foreach (var property in page.Properties)
        {
            if (property.Key == notionPublishedAtPropertyName)
            {
                if (propertyParser.TryParsePropertyValueAsDateTime(property.Value, out var parsedPublishedAt))
                {
                    publishedDateTime = parsedPublishedAt;
                }
            }
            else if (property.Key == notionCrawledAtPropertyName)
            {
                if (propertyParser.TryParsePropertyValueAsDateTime(property.Value, out var parsedCrawledAt))
                {
                    lastSystemCrawledDateTime = parsedCrawledAt;
                }
            }
            else if (property.Key == notionSlugPropertyName)
            {
                if (propertyParser.TryParsePropertyValueAsPlainText(property.Value, out var parsedSlug))
                {
                    slug = parsedSlug;
                }
            }
            else if (property.Key == notionTitlePropertyName)
            {
                if (propertyParser.TryParsePropertyValueAsPlainText(property.Value, out var parsedTitle))
                {
                    title = parsedTitle;
                }
            }
            else if (property.Key == notionDescriptionPropertyName)
            {
                if (propertyParser.TryParsePropertyValueAsPlainText(property.Value, out var parsedDescription))
                {
                    description = parsedDescription;
                }
            }
            else if (property.Key == notionTagsPropertyName)
            {
                if (propertyParser.TryParsePropertyValueAsStringSet(property.Value, out var parsedTags))
                {
                    tags = parsedTags.Select(tag => $"\"{tag}\"").ToList();
                }
            }
            else if (property.Key == notionTypePropertyName)
            {
                if (propertyParser.TryParsePropertyValueAsPlainText(property.Value, out var parsedType))
                {
                    type = parsedType;
                }
            }
            else if (property.Key == notionRequestPublisingPropertyName)
            {
                if (propertyParser.TryParsePropertyValueAsBoolean(property.Value, out var parsedBoolean))
                {
                    requestPublishing = parsedBoolean;
                }
            }
        }

        if (!requestPublishing)
        {
            Console.WriteLine($"{page.Id}(title = {title}): No request publishing.");
            return false;
        }

        if (!publishedDateTime.HasValue)
        {
            Console.WriteLine($"{page.Id}(title = {title}): Skip updating becase this page don't have publish ate.");
            return false;
        }

        if (!forceExport)
        {
            if (now < publishedDateTime.Value)
            {
                Console.WriteLine($"{page.Id}(title = {title}): Skip updating because the publication date have not been reached");
                return false;
            }
        }

        slug = string.IsNullOrEmpty(slug) ? title : slug;
        var outputDirectory = BuildOutputDirectory(publishedDateTime.Value, title, slug);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("---");

        if (!string.IsNullOrWhiteSpace(type))
        {
            stringBuilder.AppendLine($"{frontMatterTypeName}: \"{type}\"");
        }

        stringBuilder.AppendLine($"{frontMatterTitleName}: \"{title}\"");

        if (!string.IsNullOrWhiteSpace(description))
        {
            stringBuilder.AppendLine($"{frontMatterDescriptionName}: \"{description}\"");
        }
        if (tags is not null)
        {
            stringBuilder.AppendLine($"{frontMatterTagsName}: [{string.Join(',', tags)}]");
        }
        stringBuilder.AppendLine($"{frontMatterPublishedName}: \"{publishedDateTime.Value.ToString("s")}\"");

        if (page.Cover is not null && page.Cover is UploadedFile uploadedFile)
        {
            var (fileName, _) = await DownloadImage(uploadedFile.File.Url, outputDirectory);
            stringBuilder.AppendLine($"{frontMatterEyecatch}: \"./{fileName}\"");
        }

        stringBuilder.AppendLine("");
        stringBuilder.AppendLine("---");
        stringBuilder.AppendLine("");


        // ページの内容を追加
        var pagination = await client.Blocks.RetrieveChildrenAsync(page.Id);
        do
        {
            foreach (Block block in pagination.Results.Cast<Block>())
            {
                await AppendBlockLineAsync(block, string.Empty, outputDirectory, stringBuilder);
            }

            if (!pagination.HasMore)
            {
                break;
            }

            pagination = await client.Blocks.RetrieveChildrenAsync(page.Id, new BlocksRetrieveChildrenParameters
            {
                StartCursor = pagination.NextCursor,
            });
        } while (true);

        using var fileStream = File.OpenWrite($"{outputDirectory}/index.md");
        using var streamWriter = new StreamWriter(fileStream, new UTF8Encoding(false));
        await streamWriter.WriteAsync(stringBuilder.ToString());

        return true;
    }


    /// <summary>
    /// 出力ディレクトリを構築します。
    /// </summary>
    /// <param name="publishedDate">公開日</param>
    /// <param name="title">タイトル</param>
    /// <param name="slug">スラッグ</param>
    /// <returns>出力ディレクトリのパス</returns>
    string BuildOutputDirectory(DateTime publishedDate, string title, string slug)
    {
        var template = Scriban.Template.Parse(outputDirectoryPathTemplate);
        return template.Render(new
        {
            publish = publishedDate,
            title = title,
            slug = slug,
        });
    }

    /// <summary>
    /// ブロック行を追加します。
    /// </summary>
    /// <param name="block">ブロック</param>
    /// <param name="indent">インデント</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <param name="stringBuilder">StringBuilder</param>
    /// <returns>タスク</returns>
    async Task AppendBlockLineAsync(Block block, string indent, string outputDirectory, StringBuilder stringBuilder)
    {
        switch (block)
        {
            case ParagraphBlock paragraphBlock:
                foreach (var text in paragraphBlock.Paragraph.RichText)
                {
                    AppendRichText(text, stringBuilder);
                }
                stringBuilder.AppendLine(string.Empty);
                break;
            case HeadingOneBlock h1:
                stringBuilder.Append($"{indent}# ");
                foreach (var text in h1.Heading_1.RichText)
                {
                    AppendRichText(text, stringBuilder);
                }
                stringBuilder.AppendLine(string.Empty);
                break;
            case HeadingTwoBlock h2:
                stringBuilder.Append($"{indent}## ");
                foreach (var text in h2.Heading_2.RichText)
                {
                    AppendRichText(text, stringBuilder);
                }
                stringBuilder.AppendLine(string.Empty);
                break;
            case HeadingThreeBlock h3:
                stringBuilder.Append($"{indent}### ");
                foreach (var text in h3.Heading_3.RichText)
                {
                    AppendRichText(text, stringBuilder);
                }
                stringBuilder.AppendLine(string.Empty);
                break;
            case ImageBlock imageBlock:
                await AppendImageAsync(imageBlock, indent, outputDirectory, stringBuilder);
                stringBuilder.AppendLine(string.Empty);
                break;
            case CodeBlock codeBlock:
                AppendCode(codeBlock, indent, stringBuilder);
                stringBuilder.AppendLine(string.Empty);
                break;
            case BulletedListItemBlock bulletListItemBlock:
                AppendBulletListItem(bulletListItemBlock, indent, stringBuilder);
                break;
            case NumberedListItemBlock numberedListItemBlock:
                AppendNumberedListItem(numberedListItemBlock, indent, stringBuilder);
                break;
            case BookmarkBlock bookmarkBlock:
                var caption = bookmarkBlock.Bookmark.Caption.FirstOrDefault()?.PlainText;
                var url = bookmarkBlock.Bookmark.Url;
                if (!string.IsNullOrEmpty(caption))
                {
                    stringBuilder.Append($"[{caption}]({url})");
                    break;
                }
                stringBuilder.Append($"<{url}>");
                break;
            case QuoteBlock quoteBlock:
                foreach (var richText in quoteBlock.Quote.RichText)
                {
                    var text = richText.PlainText.Split("\n");
                    foreach (var t in text)
                    {
                        stringBuilder.AppendLine($">{t}");
                    }
                }
                break;
            case DividerBlock divider:
                stringBuilder.Append("---");
                break;
        }

        stringBuilder.AppendLine(string.Empty);

        if (block.HasChildren)
        {
            var pagination = await client.Blocks.RetrieveChildrenAsync(block.Id);
            do
            {
                foreach (Block childBlock in pagination.Results.Cast<Block>())
                {
                    await AppendBlockLineAsync(childBlock, $"    {indent}", outputDirectory, stringBuilder);
                }

                if (!pagination.HasMore)
                {
                    break;
                }

                pagination = await client.Blocks.RetrieveChildrenAsync(block.Id, new BlocksRetrieveChildrenParameters
                {
                    StartCursor = pagination.NextCursor,
                });
            } while (true);
        }
    }


    /// <summary>
    /// リッチテキストを追加します。
    /// </summary>
    /// <param name="richText">リッチテキスト</param>
    /// <param name="stringBuilder">StringBuilder</param>
    void AppendRichText(RichTextBase richText, StringBuilder stringBuilder)
    {
        var text = richText.PlainText;

        if (!string.IsNullOrEmpty(richText.Href))
        {
            text = $"[{text}]({richText.Href})";
        }

        if (richText.Annotations.IsCode)
        {
            text = $"`{text}`";
        }

        if (richText.Annotations.IsItalic && richText.Annotations.IsBold)
        {
            text = $"***{text}***";
        }
        else if (richText.Annotations.IsBold)
        {
            text = $"**{text}**";
        }
        else if (richText.Annotations.IsItalic)
        {
            text = $"*{text}*";
        }

        if (richText.Annotations.IsStrikeThrough)
        {
            text = $"~{text}~";
        }

        stringBuilder.Append(text);
    }


    /// <summary>
    /// 画像を追加します。
    /// </summary>
    /// <param name="imageBlock">画像ブロック</param>
    /// <param name="indent">インデント</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <param name="stringBuilder">StringBuilder</param>
    /// <returns>タスク</returns>
    async Task AppendImageAsync(ImageBlock imageBlock, string indent, string outputDirectory, StringBuilder stringBuilder)
    {
        var url = string.Empty;
        switch (imageBlock.Image)
        {
            case ExternalFile externalFile:
                url = externalFile.External.Url;
                break;
            case UploadedFile uploadedFile:
                url = uploadedFile.File.Url;
                break;
        }

        if (!string.IsNullOrEmpty(url))
        {
            var (fileName, _) = await DownloadImage(url, outputDirectory);
            stringBuilder.Append($"{indent}![](./{fileName})");
        }
    }


    /// <summary>
    /// 画像をダウンロードします。
    /// </summary>
    /// <param name="url">画像のURL</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>ファイル名とファイルパスのタプル</returns>
    async Task<(string, string)> DownloadImage(string url, string outputDirectory)
    {
        var uri = new Uri(url);
        var input = Encoding.UTF8.GetBytes(uri.LocalPath);
        var fileName = $"{Convert.ToHexString(MD5.HashData(input))}{Path.GetExtension(uri.LocalPath)}";
        var filePath = $"{outputDirectory}/{fileName}";

        using var client = new HttpClient();
        var response = await client.GetAsync(uri);
        response.EnsureSuccessStatusCode();
        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fs);

        return (fileName, filePath);
    }


    /// <summary>
    /// コードブロックを追加します。
    /// </summary>
    /// <param name="codeBlock">コードブロック</param>
    /// <param name="indent">インデント</param>
    /// <param name="stringBuilder">StringBuilder</param>
    void AppendCode(CodeBlock codeBlock, string indent, StringBuilder stringBuilder)
    {
        stringBuilder.AppendLine($"{indent}```{NotionCodeLanguageToMarkdownCodeLanguage(codeBlock.Code.Language)}");
        foreach (var richText in codeBlock.Code.RichText)
        {
            // stringBuilder.Append(indent);
            // AppendRichText(richText, stringBuilder);
            // stringBuilder.AppendLine(string.Empty);
            string text = richText.PlainText.Replace("\t", "    "); // タブをスペースに変換
            stringBuilder.Append(indent);
            stringBuilder.Append(text);
            stringBuilder.AppendLine(string.Empty);
        }
        stringBuilder.AppendLine($"{indent}```");
    }


    /// <summary>
    /// Notionのコード言語をMarkdownのコード言語に変換します。
    /// </summary>
    /// <param name="language">Notionのコード言語</param>
    /// <returns>Markdownのコード言語</returns>
    string NotionCodeLanguageToMarkdownCodeLanguage(string language)
    {
        return language switch
        {
            "c#" => "csharp",
            _ => language,
        };
    }


    /// <summary>
    /// 箇条書きリスト項目を追加します。
    /// </summary>
    /// <param name="bulletedListItemBlock">箇条書きリスト項目ブロック</param>
    /// <param name="indent">インデント</param>
    /// <param name="stringBuilder">StringBuilder</param>
    void AppendBulletListItem(BulletedListItemBlock bulletedListItemBlock, string indent, StringBuilder stringBuilder)
    {
        stringBuilder.Append($"{indent}* ");
        foreach (var item in bulletedListItemBlock.BulletedListItem.RichText)
        {
            AppendRichText(item, stringBuilder);
        }
    }


    /// <summary>
    /// 番号付きリスト項目を追加します。
    /// </summary>
    /// <param name="numberedListItemBlock">番号付きリスト項目ブロック</param>
    /// <param name="indent">インデント</param>
    /// <param name="stringBuilder">StringBuilder</param>
    void AppendNumberedListItem(NumberedListItemBlock numberedListItemBlock, string indent, StringBuilder stringBuilder)
    {
        stringBuilder.Append($"{indent}1. ");
        foreach (var item in numberedListItemBlock.NumberedListItem.RichText)
        {
            AppendRichText(item, stringBuilder);
        }
    }
}
