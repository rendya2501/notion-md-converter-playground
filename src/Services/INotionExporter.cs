namespace NotionMarkdownConverter.Services;

/// <summary>
/// Notionエクスポーターのインターフェース
/// </summary>
public interface INotionExporter
{
    /// <summary>
    /// ページをエクスポートします。
    /// </summary>
    /// <returns></returns>
    Task ExportPagesAsync();
}
