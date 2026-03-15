namespace NotionMarkdownConverter.Infrastructure.Notion;

/// <summary>
/// Notionのページプロパティを更新する操作のインターフェース。
/// Loadステージで使用します。
/// </summary>
public interface INotionPageWriter
{
    /// <summary>
    /// エクスポート完了後にページのプロパティを更新します。
    /// クロール日時を記録し、公開ステータスを公開済みに変更します。
    /// </summary>
    /// <param name="pageId">更新対象のページID</param>
    /// <param name="now">クロール日時として記録する日時（UTC）</param>
    Task UpdatePagePropertiesAsync(string pageId, DateTime now);
}
