using NotionMarkdownConverter.Core.Enums;

namespace NotionMarkdownConverter.Core.Models;

/// <summary>
/// ページのプロパティ
/// </summary>
public class PageProperty
{
    /// <summary>
    /// ページID
    /// </summary>
    public string PageId { get; set; } = string.Empty;

    /// <summary>
    /// タイトル
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 説明
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// スラッグ
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// タグ
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// タイプ
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 公開日時
    /// </summary>
    public DateTime? PublishedDateTime { get; set; }

    /// <summary>
    /// 最終クロール日時
    /// </summary>
    public DateTime? LastCrawledDateTime { get; set; }

    /// <summary>
    /// 公開ステータス
    /// </summary>
    public PublicStatus PublicStatus { get; set; } = PublicStatus.Unpublished;
} 