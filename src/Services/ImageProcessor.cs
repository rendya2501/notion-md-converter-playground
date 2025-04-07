using hoge.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace hoge.Services
{
    /// <summary>
    /// マークダウンの画像を処理するクラス
    /// </summary>
    public class ImageProcessor : IImageProcessor
    {
        private readonly ILogger<ImageProcessor> _logger;
        private readonly HttpClient _httpClient;
        private readonly ImageDownloaderOptions _options;

        public ImageProcessor(
            ILogger<ImageProcessor> logger,
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
        /// マークダウン内の画像を処理する
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="outputDirectory"></param>
        /// <returns>マークダウン</returns>
        public async Task<string> ProcessMarkdownImagesAsync(string markdown, string outputDirectory)
        {
            var imageUrls = ExtractImageUrls(markdown);
            var semaphore = new SemaphoreSlim(_options.MaxConcurrentDownloads);

            var downloadTasks = imageUrls.Select(async url =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await DownloadImageAsync(url, outputDirectory);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var downloadedImages = await Task.WhenAll(downloadTasks);
            return ReplaceImageUrls(markdown, downloadedImages);
        }

        /// <summary>
        /// マークダウン内の画像URLを抽出する
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        private static IEnumerable<string> ExtractImageUrls(string markdown)
        {
            var pattern = @"!\[.*?\]\((.*?)\)";
            return Regex.Matches(markdown, pattern)
                .Select(m => m.Groups[1].Value)
                .Distinct();
        }

        /// <summary>
        /// 画像をダウンロードする
        /// </summary>
        /// <param name="url"></param>
        /// <param name="outputDirectory"></param>
        /// <returns></returns>
        private async Task<(string OriginalUrl, string FileName)> DownloadImageAsync(string url, string outputDirectory)
        {
            var uri = new Uri(url);
            var fileNameBytes = Encoding.UTF8.GetBytes(uri.LocalPath);
            var fileName = $"{Convert.ToHexString(MD5.HashData(fileNameBytes))}{Path.GetExtension(uri.LocalPath)}";
            var filePath = Path.Combine(outputDirectory, fileName);

            if (_options.SkipExistingFiles && File.Exists(filePath))
            {
                _logger.LogInformation("スキップ: 既に存在するファイル {FileName}", fileName);
                return (url, fileName);
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
                    return (url, fileName);
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
        /// マークダウン内の画像URLを置換する
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="downloadedImages"></param>
        /// <returns></returns>
        private static string ReplaceImageUrls(string markdown, IEnumerable<(string OriginalUrl, string FileName)> downloadedImages)
        {
            var result = markdown;
            foreach (var (originalUrl, fileName) in downloadedImages)
            {
                result = result.Replace(originalUrl, fileName);
            }
            return result;
        }
    }
} 