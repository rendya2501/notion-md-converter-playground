namespace NotionMarkdownConverter.Infrastructure.GitHub;

/// <summary>
/// GitHub Actions の出力に関する定数を定義するクラス
/// </summary>
public static class GitHubOutputConstants
{
    /// <summary>
    /// GitHub Actions の出力ファイルパスを示す環境変数名
    /// </summary>
    /// <remarks>
    /// GitHub Actions のランナーが設定する環境変数です。
    /// この環境変数が未設定の場合、ローカル実行と判断してスキップします。
    /// </remarks>
    public const string EnvVarName = "GITHUB_OUTPUT";

    /// <summary>
    /// エクスポートしたページ数を出力するキー名
    /// </summary>
    /// <remarks>
    /// action.yml の outputs セクションの exported_count と対応しています。
    /// </remarks>
    public const string ExportedCountKey = "exported_count";
}
