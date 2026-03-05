namespace NotionMarkdownConverter.Application.Abstractions;

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