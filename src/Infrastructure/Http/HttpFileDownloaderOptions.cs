namespace NotionMarkdownConverter.Infrastructure.Http;

/// <summary>
/// <see cref="HttpFileDownloader"/> の動作を制御する設定オプション。
/// DI の Configure&lt;DownloaderOptions&gt; または appsettings.json から設定できます。
/// </summary>
public class HttpFileDownloaderOptions
{
    private int _maxRetryCount = 3;

    /// <summary>
    /// ダウンロード失敗時の最大リトライ回数。1以上の値を指定してください。
    /// </summary>
    /// <remarks>
    /// 0以下を設定すると <see cref="ArgumentOutOfRangeException"/> をスローします。
    /// デフォルト: 3
    /// </remarks>
    public int MaxRetryCount
    {
        get => _maxRetryCount;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(MaxRetryCount),
                    "MaxRetryCountは1以上である必要があります。");
            }
            _maxRetryCount = value;
        }
    }

    /// <summary>
    /// リトライ間隔（ミリ秒）。
    /// サーバーへの連続リクエストを避けるためのウェイトです。
    /// デフォルト: 1000ms
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// HTTPリクエストのタイムアウト（秒）。
    /// この時間を超えた場合はリトライ対象になります。
    /// デフォルト: 30秒
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 出力先に同名ファイルが存在する場合にダウンロードをスキップするかどうか。
    /// true にすると再実行時のダウンロードを省略できます。
    /// デフォルト: true
    /// </summary>
    public bool SkipExistingFiles { get; set; } = true;
}
