namespace NotionMarkdownConverter.Infrastructure;

/// <summary>
/// GitHub環境変数の更新を行うサービスのインターフェース
/// </summary>
public interface IGitHubEnvironmentUpdater
{
    /// <summary>
    /// GitHub Actions の環境変数を更新します。
    /// </summary>
    /// <param name="exportedCount">エクスポートしたページ数。GITHUB_OUTPUT に書き込まれます。</param>
    void UpdateEnvironment(int exportedCount);
}
