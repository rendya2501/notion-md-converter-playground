namespace NotionMarkdownConverter.Application.Configuration;

/// <summary>
/// Notionエクスポートの実行オプション
/// </summary>
/// <remarks>
/// コマンドライン引数から生成され、アプリケーション起動時に一度だけ設定されます。
/// </remarks>
public class NotionExportOptions
{
    /// <summary>
    /// Notion APIの認証トークン
    /// </summary>
    public string NotionAuthToken { get; set; } = string.Empty;

    /// <summary>
    /// エクスポート対象のNotionデータベースID
    /// </summary>
    public string NotionDatabaseId { get; set; } = string.Empty;

    /// <summary>
    /// 出力ディレクトリのパステンプレート
    /// </summary>
    /// <remarks>
    /// Scribanテンプレート構文を使用します。例: output/{{publish|date.to_string('%Y/%m')}}/{{slug}}
    /// </remarks>
    public string OutputDirectoryPathTemplate { get; set; } = string.Empty;
}
