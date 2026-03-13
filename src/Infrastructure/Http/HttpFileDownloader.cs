using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Infrastructure.Http;

/// <summary>
/// URLからファイルをダウンロードし、指定ディレクトリに保存するサービス。
/// リトライ・タイムアウト・スキップの動作は <see cref="HttpFileDownloaderOptions"/> で制御します。
/// </summary>
public class HttpFileDownloader : IFileDownloader
{
    private readonly ILogger<HttpFileDownloader> _logger;
    private readonly HttpClient _httpClient;
    private readonly HttpFileDownloaderOptions _options;

    /// <summary>
    /// コンストラクタ。
    /// HttpClient のタイムアウトをオプションの値で初期化します。
    /// </summary>
    /// <param name="logger">ログ出力に使用するロガー</param>
    /// <param name="options">ダウンロード動作の設定オプション</param>
    /// <param name="httpClientFactory">HttpClient の生成ファクトリ</param>
    public HttpFileDownloader(
        ILogger<HttpFileDownloader> logger,
        IOptions<HttpFileDownloaderOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = httpClientFactory.CreateClient();
        // タイムアウトはインスタンス生成時に一度だけ設定します。
        // HttpClient はシングルトンではないため、ここで設定しても他のリクエストに影響しません。
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    /// <summary>
    /// 指定URLからファイルをダウンロードし、出力ディレクトリに保存します。
    /// 失敗した場合は <see cref="HttpFileDownloaderOptions.MaxRetryCount"/> の回数までリトライします。
    /// </summary>
    /// <param name="urlFilePair">ダウンロード元URLとローカル保存ファイル名のペア</param>
    /// <param name="outputDirectory">ファイルの保存先ディレクトリ</param>
    /// <exception cref="HttpRequestException">リトライ上限に達してもダウンロードに失敗した場合</exception>

    public async Task DownloadAsync(UrlFilePair urlFilePair, string outputDirectory)
    {
        var url = urlFilePair.OriginalUrl;
        var fileName = urlFilePair.LocalFileName;
        var filePath = Path.Combine(outputDirectory, fileName);

        // 再実行時の無駄なダウンロードを防ぐため、既存ファイルはスキップします。
        if (_options.SkipExistingFiles && File.Exists(filePath))
        {
            _logger.LogInformation("スキップ: 既に存在するファイル {FileName}", fileName);
            return;
        }

        for (var retry = 0; retry < _options.MaxRetryCount; retry++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsByteArrayAsync();

                await File.WriteAllBytesAsync(filePath, content);

                _logger.LogInformation("ダウンロード成功: {FileName}", fileName);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "リトライ {Retry}/{MaxRetry}: {FileName} のダウンロードに失敗",
                    retry + 1, _options.MaxRetryCount, fileName);

                var isLastRetry = retry == _options.MaxRetryCount - 1;
                if (isLastRetry)
                {
                    // リトライ上限に達したため例外を再スローします。
                    // 呼び出し元でのハンドリングに委ねます。
                    _logger.LogError(ex, "ダウンロード失敗: {FileName}", fileName);
                    throw;
                }

                // 次のリトライまでサーバー負荷を考慮してウェイトします。
                await Task.Delay(_options.RetryDelayMilliseconds);
            }
        }
    }
}
