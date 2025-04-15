using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace NotionMarkdownConverter.Infrastructure.Http.Services;

/// <summary>
/// ファイルダウンローダー
/// </summary>
public class FileDownloader: IFileDownloader
{
    private readonly ILogger<FileDownloader> _logger;
    private readonly HttpClient _httpClient;
    private readonly DownloaderOptions _options;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="options">オプション</param>
    public FileDownloader(
        ILogger<FileDownloader> logger,
        IOptions<DownloaderOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
        };
    }

    /// <summary>
    /// 出力ディレクトリへファイルをダウンロードします。
    /// </summary>
    /// <param name="downloadLink">ダウンロードリンク</param>
    /// <param name="outputDirectory">出力ディレクトリ</param>
    /// <returns>ダウンロードしたファイル名</returns>
    public async Task<string> DownloadAsync(string downloadLink, string outputDirectory)
    {
        var uri = new Uri(downloadLink);
        var fileName = GenerateFileName(uri);
        var filePath = Path.Combine(outputDirectory, fileName);

        if (_options.SkipExistingFiles && File.Exists(filePath))
        {
            _logger.LogInformation("スキップ: 既に存在するファイル {FileName}", fileName);
            return fileName;
        }

        for (var retry = 0; retry < _options.MaxRetryCount; retry++)
        {
            try
            {
                var content = await ExecuteDownloadAsync(downloadLink);
                await File.WriteAllBytesAsync(filePath, content);

                _logger.LogInformation("ダウンロード成功: {FileName}", fileName);
                return fileName;
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
    /// ダウンロードを実行します。
    /// </summary>
    /// <param name="url">URL</param>
    /// <returns>ダウンロードしたファイルのバイト配列</returns>
    private async Task<byte[]> ExecuteDownloadAsync(string url)
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
