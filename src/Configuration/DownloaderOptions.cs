namespace NotionMarkdownConverter.Configuration;

/// <summary>
/// ダウンローダーのオプション
/// </summary>
public class DownloaderOptions
{
    /// <summary>
    /// 最大リトライ回数
    /// </summary>
    /// <value></value>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// リトライ遅延ミリ秒
    /// </summary>
    /// <value></value>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// タイムアウト秒
    /// </summary>
    /// <value></value>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 既存ファイルをスキップするかどうか
    /// </summary>
    /// <value></value>
    public bool SkipExistingFiles { get; set; } = true;
} 