using Microsoft.Extensions.Logging.Abstractions;
using NotionMarkdownConverter.Infrastructure.GitHub;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// GitHubEnvironmentUpdaterのユニットテスト。
/// <see cref="GitHubOutputConstants.EnvVarName"/> 環境変数の有無による分岐と、ファイルへの書き込みを検証します。
/// </summary>
public class GitHubEnvironmentUpdaterTests : IDisposable
{
    // テスト間の環境変数汚染を防ぐため、元の値を保持して復元します。
    private readonly string? _originalGitHubOutput;

    public GitHubEnvironmentUpdaterTests()
    {
        _originalGitHubOutput = Environment.GetEnvironmentVariable(GitHubOutputConstants.EnvVarName);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(GitHubOutputConstants.EnvVarName, _originalGitHubOutput);
        GC.SuppressFinalize(this);
    }

    private static GitHubEnvironmentUpdater CreateSut() =>
        new(NullLogger<GitHubEnvironmentUpdater>.Instance);

    // ── GITHUB_OUTPUT 未設定 ──────────────────────────────────────────

    [Fact]
    public void UpdateEnvironment_WhenGitHubOutputNotSet_DoesNotThrow()
    {
        // GitHubOutputConstants.EnvVarName が未設定の場合は警告ログを出力して正常終了する
        Environment.SetEnvironmentVariable(GitHubOutputConstants.EnvVarName, null);
        var ex = Record.Exception(() => CreateSut().UpdateEnvironment(5));
        Assert.Null(ex);
    }

    // ── GITHUB_OUTPUT 設定済み ────────────────────────────────────────

    [Fact]
    public void UpdateEnvironment_WhenGitHubOutputSet_WritesExportedCountToFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            Environment.SetEnvironmentVariable(GitHubOutputConstants.EnvVarName, tempFile);
            CreateSut().UpdateEnvironment(42);

            var content = File.ReadAllText(tempFile);
            Assert.Contains($"{GitHubOutputConstants.ExportedCountKey}=42", content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void UpdateEnvironment_WhenGitHubOutputSet_WritesZeroCount()
    {
        // エクスポートが0件でも正しく書き込まれる
        var tempFile = Path.GetTempFileName();
        try
        {
            Environment.SetEnvironmentVariable(GitHubOutputConstants.EnvVarName, tempFile);
            CreateSut().UpdateEnvironment(0);

            var content = File.ReadAllText(tempFile);
            Assert.Contains($"{GitHubOutputConstants.ExportedCountKey}=0", content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void UpdateEnvironment_WhenGitHubOutputSet_AppendsToExistingContent()
    {
        // StreamWriter は append: true で開くため、既存の内容を上書きしない
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "previous_step=done\n");
            Environment.SetEnvironmentVariable(GitHubOutputConstants.EnvVarName, tempFile);
            CreateSut().UpdateEnvironment(3);

            var content = File.ReadAllText(tempFile);
            Assert.Contains("previous_step=done", content);
            Assert.Contains($"{GitHubOutputConstants.ExportedCountKey}=3", content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
