using Microsoft.Extensions.Logging;

namespace NotionMarkdownConverter.Infrastructure.GitHub;

/// <summary>
/// GitHub環境変数の更新を行うサービス
/// </summary>
public class GitHubEnvironmentUpdater(ILogger<GitHubEnvironmentUpdater> _logger) : IGitHubEnvironmentUpdater
{
    /// <summary>
    /// GitHub Actions の環境変数を更新します。
    /// </summary>
    /// <param name="exportedCount">エクスポートしたページ数</param>
    public void UpdateEnvironment(int exportedCount)
    {
        // GitHub Actions の環境変数ファイルパスを取得
        var githubOutput = Environment.GetEnvironmentVariable(GitHubOutputConstants.EnvVarName);

        // 環境変数が設定されていない場合は警告を出力
        if (string.IsNullOrEmpty(githubOutput))
        {
            _logger.LogWarning($"{GitHubOutputConstants.EnvVarName} not set, skipping environment update.");
            return;
        }

        using var writer = new StreamWriter(githubOutput, append: true);
        writer.WriteLine($"{GitHubOutputConstants.ExportedCountKey}={exportedCount}");
    }
}
