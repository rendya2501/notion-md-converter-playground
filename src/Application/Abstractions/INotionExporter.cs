namespace NotionMarkdownConverter.Application.Abstractions;

/// <summary>
/// Notionのページをエクスポートするサービス
/// </summary>
public interface INotionExporter
{
    /// <summary>
    /// Notionのページをエクスポートします。
    /// </summary>
    /// <returns>エクスポート処理が完了したときに解決するタスク</returns>
    Task ExportPagesAsync();
}
