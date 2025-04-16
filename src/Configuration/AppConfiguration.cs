namespace NotionMarkdownConverter.Configuration;

/// <summary>
/// アプリケーションの設定
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// Notionの認証トークン
    /// </summary>
    /// <value></value>
    public string NotionAuthToken { get;  set; } = string.Empty;

    /// <summary>
    /// NotionのデータベースID
    /// </summary>
    /// <value></value>
    public string NotionDatabaseId { get;  set; } = string.Empty;

    /// <summary>
    /// 出力ディレクトリのパステンプレート
    /// </summary>
    /// <value></value>
    public string OutputDirectoryPathTemplate { get;  set; } = string.Empty;
}
