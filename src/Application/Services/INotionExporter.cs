namespace NotionMarkdownConverter.Application.Services;

/// <summary>
/// Notionのページをエクスポートするサービス
/// </summary>
public interface INotionExporter
{
    /// <summary>
    /// Notionのページをエクスポートします。
    /// </summary>
    /// <returns></returns>
    Task ExportPagesAsync();
} 