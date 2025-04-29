using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Application.Interfaces;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Core.Models;
using Scriban;

namespace NotionMarkdownConverter.Infrastructure.Service;

/// <summary>
/// 出力ディレクトリ構築サービス
/// </summary>
public class OutputDirectoryBuilder(
    IOptions<AppConfiguration> _config,
    ILogger<OutputDirectoryBuilder> _logger) : IDirectoryBuilder
{
    /// <summary>
    /// 出力ディレクトリを構築します。
    /// </summary>
    /// <param name="pageProperty"></param>
    /// <returns></returns>
    public string Build(PageProperty pageProperty)
    {
        // 出力ディレクトリのパスをテンプレートから生成
        var template = Template.Parse(_config.Value.OutputDirectoryPathTemplate);
        // スラグが設定されていない場合はタイトルを使用
        var slug = !string.IsNullOrEmpty(pageProperty.Slug)
            ? pageProperty.Slug
            : pageProperty.Title;

        // 出力ディレクトリパスをレンダリング
        var outputDirectory = template.Render(new
        {
            publish = pageProperty.PublishedDateTime!.Value,
            title = pageProperty.Title,
            slug
        });

        // ディレクトリを作成
        Directory.CreateDirectory(outputDirectory);
        _logger.LogInformation("Created output directory: {OutputDirectory}", outputDirectory);

        return outputDirectory;
    }
}