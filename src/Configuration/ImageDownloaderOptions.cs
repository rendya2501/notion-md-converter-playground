namespace hoge.Configuration;

/// <summary>
/// 画像ダウンローダーのオプション
/// </summary>
public class ImageDownloaderOptions
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
    /// 最大同時ダウンロード数
    /// </summary>
    /// <value></value>
    public int MaxConcurrentDownloads { get; set; } = 4;

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