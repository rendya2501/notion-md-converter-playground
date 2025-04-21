using Microsoft.Extensions.Logging;
using NotionMarkdownConverter.Application.Interface;

namespace NotionMarkdownConverter.Infrastructure.Services;

/// <summary>
/// GitHub環境変数の更新を行うサービス
/// </summary>
public class GitHubEnvironmentUpdater(ILogger<GitHubEnvironmentUpdater> _logger) : IGitHubClient
{
    /// <summary>
    /// GitHub Actions の環境変数を更新します。
    /// </summary>
    /// <param name="exportedCount"></param>
    public void UpdateEnvironment(int exportedCount)
    {
        // GitHub Actions の環境変数ファイルパスを取得
        var githubOutput = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");

        // 環境変数が設定されていない場合は警告を出力
        if (string.IsNullOrEmpty(githubOutput))
        {
            _logger.LogWarning("GITHUB_OUTPUT not set, skipping environment update.");
            return;
        }

        using (StreamWriter writer = new(githubOutput, true))
        {
            writer.WriteLine($"exported_count={exportedCount}");
        }
        _logger.LogInformation("exported_count={exportedCount}", exportedCount);
    }
} 