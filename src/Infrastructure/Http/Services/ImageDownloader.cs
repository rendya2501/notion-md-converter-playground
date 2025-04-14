using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Infrastructure.Http.Model;
using System.Security.Cryptography;
using System.Text;

namespace NotionMarkdownConverter.Infrastructure.Http.Services;

/// <summary>
/// 画像ダウンローダー
/// </summary>
public class ImageDownloader : IImageDownloader
{
    private readonly ILogger<ImageDownloader> _logger;
    private readonly HttpClient _httpClient;
    private readonly ImageDownloaderOptions _options;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="options">オプション</param>
    public ImageDownloader(
        ILogger<ImageDownloader> logger,
        IOptions<ImageDownloaderOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
        };
    }

    /// <summary>
    /// 画像をダウンロードします。
    /// </summary>
    /// <param name="url">画像URL</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>ダウンロードした画像の情報</returns>
    public async Task<DownloadedImage> DownloadAsync(string url, string outputDirectory)
    {
        var uri = new Uri(url);
        var fileName = GenerateFileName(uri);
        var filePath = Path.Combine(outputDirectory, fileName);

        if (_options.SkipExistingFiles && File.Exists(filePath))
        {
            _logger.LogInformation("スキップ: 既に存在するファイル {FileName}", fileName);
            return new DownloadedImage(url, fileName);
        }

        for (var retry = 0; retry < _options.MaxRetryCount; retry++)
        {
            try
            {
                var content = await DownloadImageAsync(url);
                await File.WriteAllBytesAsync(filePath, content);

                _logger.LogInformation("ダウンロード成功: {FileName}", fileName);
                return new DownloadedImage(url, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "リトライ {Retry}/{MaxRetry}: {FileName} のダウンロードに失敗",
                    retry + 1, _options.MaxRetryCount, fileName);

                if (retry < _options.MaxRetryCount - 1)
                {
                    await Task.Delay(_options.RetryDelayMilliseconds);
                }
                else
                {
                    _logger.LogError(ex, "ダウンロード失敗: {FileName}", fileName);
                    throw;
                }
            }
        }

        throw new InvalidOperationException("リトライ回数を超えました");
    }

    /// <summary>
    /// 画像をダウンロードします。
    /// </summary>
    /// <param name="url">画像URL</param>
    /// <returns>ダウンロードした画像のバイト配列</returns>
    private async Task<byte[]> DownloadImageAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// ファイル名を生成します。
    /// </summary>
    /// <param name="uri">URI</param>
    /// <returns>ファイル名</returns>
    private static string GenerateFileName(Uri uri)
    {
        var fileNameBytes = Encoding.UTF8.GetBytes(uri.LocalPath);
        return $"{Convert.ToHexString(MD5.HashData(fileNameBytes))}{Path.GetExtension(uri.LocalPath)}";
    }
}
