using hoge.Services;

namespace hoge.Models;

/// <summary>
/// ページのプロパティを表します。
/// </summary>
public class PageProperty
{
    public string PageId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
    public DateTime? PublishedDateTime { get; set; }
    public DateTime? LastCrawledDateTime { get; set; }
    public PublicStatus PublicStatus { get; set; } = PublicStatus.Unpublished;
}
