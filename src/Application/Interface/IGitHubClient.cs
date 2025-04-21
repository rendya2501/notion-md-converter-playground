namespace NotionMarkdownConverter.Application.Interface;

/// <summary>
/// GitHub クライアントインターフェース
/// </summary>
public interface IGitHubClient
{
    /// <summary>
    /// GitHub Actions の環境変数を更新します。
    /// </summary>
    /// <param name="exportedCount"></param>
    void UpdateEnvironment(int exportedCount);
} 