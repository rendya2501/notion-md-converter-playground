namespace NotionMarkdownConverter.Infrastructure.GitHub.Services;

/// <summary>
/// GitHub環境変数の更新を行うサービスのインターフェース
/// </summary>
public interface IGitHubEnvironmentUpdater
{
    /// <summary>
    /// GitHub Actions の環境変数を更新します。
    /// </summary>
    /// <param name="exportedCount"></param>
    void UpdateEnvironment(int exportedCount);
} 