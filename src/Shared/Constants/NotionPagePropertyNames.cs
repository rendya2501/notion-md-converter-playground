namespace NotionMarkdownConverter.Shared.Constants;

/// <summary>
/// Notionページのプロパティ名を定義するクラス
/// </summary>
public static class NotionPagePropertyNames
{
    /// <summary>公開日時を示す日付プロパティ名</summary>
    public const string PublishedAtPropertyName = "PublishedAt";

    /// <summary>URLスラグを示すリッチテキストプロパティ名</summary>
    public const string SlugPropertyName = "Slug";

    /// <summary>Notionページのタイトルプロパティ名</summary>
    public const string TitlePropertyName = "Title";

    /// <summary>記事タイプ（例: Tech, Idea）を示すセレクトプロパティ名</summary>
    public const string TypePropertyName = "Type";

    /// <summary>タグのマルチセレクトプロパティ名</summary>
    public const string TagsPropertyName = "Tags";

    /// <summary>公開ステータス（Published / Queued / Unpublished）を示すセレクトプロパティ名</summary>
    public const string PublicStatusName = "PublicStatus";

    /// <summary>記事の説明文を示すリッチテキストプロパティ名</summary>
    public const string DescriptionPropertyName = "Description";

    /// <summary>最終クロール日時を記録するシステム管理プロパティ名</summary>
    public const string CrawledAtPropertyName = "_SystemCrawledAt";
}
