using Notion.Client;
using NotionMarkdownConverter.Domain.Mappers;
using NotionMarkdownConverter.Shared.Constants;
using NotionMarkdownConverter.Shared.Enums;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// PagePropertyMapperのユニットテスト
/// </summary>
public class PagePropertyMapperTests
{
    private readonly PagePropertyMapper _sut = new();

    // ── ヘルパー ──────────────────────────────────────────────────────

    private static Page MakePage(string id, Dictionary<string, PropertyValue>? properties = null) =>
        new() { Id = id, Properties = properties ?? [] };

    private static RichTextText MakeRichText(string text) =>
        new() { PlainText = text, Text = new Text { Content = text } };

    // ── PageId ────────────────────────────────────────────────────────

    [Fact]
    public void Map_Always_SetsPageIdFromPage()
    {
        var result = _sut.Map(MakePage("abc-123"));
        Assert.Equal("abc-123", result.PageId);
    }

    // ── Title ─────────────────────────────────────────────────────────

    [Fact]
    public void Map_WithTitle_SetsTitle()
    {
        var page = MakePage("id", new()
        {
            [NotionPagePropertyNames.TitlePropertyName] = new TitlePropertyValue
            {
                Title = [MakeRichText("テストタイトル")]
            }
        });
        Assert.Equal("テストタイトル", _sut.Map(page).Title);
    }

    [Fact]
    public void Map_WithMultiSegmentTitle_ConcatenatesSegments()
    {
        // Notionのタイトルは複数のRichTextセグメントで構成されることがある
        var page = MakePage("id", new()
        {
            [NotionPagePropertyNames.TitlePropertyName] = new TitlePropertyValue
            {
                Title = [MakeRichText("前半"), MakeRichText("後半")]
            }
        });
        Assert.Equal("前半後半", _sut.Map(page).Title);
    }

    [Fact]
    public void Map_WithoutTitle_DefaultsToEmptyString()
    {
        Assert.Equal(string.Empty, _sut.Map(MakePage("id")).Title);
    }

    // ── PublicStatus ──────────────────────────────────────────────────

    [Theory]
    [InlineData("Published", PublicStatus.Published)]
    [InlineData("Queued", PublicStatus.Queued)]
    [InlineData("Unpublished", PublicStatus.Unpublished)]
    public void Map_WithPublicStatus_ParsesEnum(string statusName, PublicStatus expected)
    {
        var page = MakePage("id", new()
        {
            [NotionPagePropertyNames.PublicStatusName] = new SelectPropertyValue
            {
                Select = new SelectOption { Name = statusName }
            }
        });
        Assert.Equal(expected, _sut.Map(page).PublicStatus);
    }

    [Fact]
    public void Map_WithoutPublicStatus_DefaultsToUnpublished()
    {
        Assert.Equal(PublicStatus.Unpublished, _sut.Map(MakePage("id")).PublicStatus);
    }

    // ── PublishedDateTime ─────────────────────────────────────────────

    [Fact]
    public void Map_WithPublishedAt_SetsPublishedDateTime()
    {
        var expectedDate = new DateTimeOffset(2024, 3, 15, 10, 0, 0, TimeSpan.Zero);
        var page = MakePage("id", new()
        {
            [NotionPagePropertyNames.PublishedAtPropertyName] = new DatePropertyValue
            {
                Date = new Date { Start = expectedDate }
            }
        });
        Assert.Equal(expectedDate.DateTime, _sut.Map(page).PublishedDateTime);
    }

    [Fact]
    public void Map_WithoutPublishedAt_LeavesPublishedDateTimeNull()
    {
        Assert.Null(_sut.Map(MakePage("id")).PublishedDateTime);
    }

    // ── CrawledAt ────────────────────────────────────────────────────

    [Fact]
    public void Map_WithCrawledAt_SetsLastCrawledDateTime()
    {
        var crawledDate = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero);
        var page = MakePage("id", new()
        {
            [NotionPagePropertyNames.CrawledAtPropertyName] = new DatePropertyValue
            {
                Date = new Date { Start = crawledDate }
            }
        });
        Assert.Equal(crawledDate.DateTime, _sut.Map(page).LastCrawledDateTime);
    }

    // ── Slug ──────────────────────────────────────────────────────────

    [Fact]
    public void Map_WithSlug_SetsSlug()
    {
        var page = MakePage("id", new()
        {
            [NotionPagePropertyNames.SlugPropertyName] = new RichTextPropertyValue
            {
                RichText = [MakeRichText("my-post-slug")]
            }
        });
        Assert.Equal("my-post-slug", _sut.Map(page).Slug);
    }

    [Fact]
    public void Map_WithoutSlug_DefaultsToEmptyString()
    {
        Assert.Equal(string.Empty, _sut.Map(MakePage("id")).Slug);
    }

    // ── Description ───────────────────────────────────────────────────

    [Fact]
    public void Map_WithDescription_SetsDescription()
    {
        var page = MakePage("id", new()
        {
            [NotionPagePropertyNames.DescriptionPropertyName] = new RichTextPropertyValue
            {
                RichText = [MakeRichText("記事の説明文")]
            }
        });
        Assert.Equal("記事の説明文", _sut.Map(page).Description);
    }

    // ── Tags ──────────────────────────────────────────────────────────

    [Fact]
    public void Map_WithTags_SetsTagsList()
    {
        var page = MakePage("id", new()
        {
            [NotionPagePropertyNames.TagsPropertyName] = new MultiSelectPropertyValue
            {
                MultiSelect = [
                    new SelectOption { Name = "C#" },
                    new SelectOption { Name = ".NET" }
                ]
            }
        });
        var result = _sut.Map(page);

        Assert.Equal(2, result.Tags.Count);
        Assert.Contains("C#", result.Tags);
        Assert.Contains(".NET", result.Tags);
    }

    [Fact]
    public void Map_WithoutTags_DefaultsToEmptyList()
    {
        Assert.Empty(_sut.Map(MakePage("id")).Tags);
    }

    // ── Type ──────────────────────────────────────────────────────────

    [Fact]
    public void Map_WithType_SetsType()
    {
        var page = MakePage("id", new()
        {
            [NotionPagePropertyNames.TypePropertyName] = new SelectPropertyValue
            {
                Select = new SelectOption { Name = "article" }
            }
        });
        Assert.Equal("article", _sut.Map(page).Type);
    }

    // ── 不明なプロパティ ──────────────────────────────────────────────

    [Fact]
    public void Map_WithUnknownProperty_IsIgnoredWithoutException()
    {
        var page = MakePage("id", new()
        {
            ["UnknownProp"] = new RichTextPropertyValue { RichText = [MakeRichText("value")] }
        });
        var ex = Record.Exception(() => _sut.Map(page));
        Assert.Null(ex);
    }

    [Fact]
    public void Map_EmptyProperties_UsesAllDefaults()
    {
        var result = _sut.Map(MakePage("id"));

        Assert.Equal(string.Empty, result.Title);
        Assert.Equal(string.Empty, result.Slug);
        Assert.Equal(string.Empty, result.Description);
        Assert.Equal(string.Empty, result.Type);
        Assert.Empty(result.Tags);
        Assert.Null(result.PublishedDateTime);
        Assert.Null(result.LastCrawledDateTime);
        Assert.Equal(PublicStatus.Unpublished, result.PublicStatus);
    }

    // ── 全プロパティ同時 ──────────────────────────────────────────────

    [Fact]
    public void Map_WithAllProperties_MapsAllFields()
    {
        var publishDate = new DateTimeOffset(2024, 6, 1, 9, 0, 0, TimeSpan.Zero);
        var page = MakePage("full-id", new()
        {
            [NotionPagePropertyNames.TitlePropertyName] = new TitlePropertyValue
            { Title = [MakeRichText("フルタイトル")] },
            [NotionPagePropertyNames.PublicStatusName] = new SelectPropertyValue
            { Select = new SelectOption { Name = "Queued" } },
            [NotionPagePropertyNames.PublishedAtPropertyName] = new DatePropertyValue
            { Date = new Date { Start = publishDate } },
            [NotionPagePropertyNames.TagsPropertyName] = new MultiSelectPropertyValue
            { MultiSelect = [new SelectOption { Name = "tag1" }, new SelectOption { Name = "tag2" }] },
            [NotionPagePropertyNames.SlugPropertyName] = new RichTextPropertyValue
            { RichText = [MakeRichText("full-post")] },
            [NotionPagePropertyNames.DescriptionPropertyName] = new RichTextPropertyValue
            { RichText = [MakeRichText("説明文")] },
            [NotionPagePropertyNames.TypePropertyName] = new SelectPropertyValue
            { Select = new SelectOption { Name = "article" } }
        });

        var result = _sut.Map(page);

        Assert.Equal("full-id", result.PageId);
        Assert.Equal("フルタイトル", result.Title);
        Assert.Equal(PublicStatus.Queued, result.PublicStatus);
        Assert.Equal(publishDate.DateTime, result.PublishedDateTime);
        Assert.Equal(2, result.Tags.Count);
        Assert.Equal("full-post", result.Slug);
        Assert.Equal("説明文", result.Description);
        Assert.Equal("article", result.Type);
    }
}
