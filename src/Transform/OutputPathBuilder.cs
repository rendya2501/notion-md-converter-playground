using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Configuration;
using NotionMarkdownConverter.Shared.Models;
using Scriban;

namespace NotionMarkdownConverter.Transform;

/// <summary>
/// ページプロパティをもとに出力ディレクトリのパスを組み立てます。
/// Scribanテンプレートの展開のみを担い、ディレクトリ作成は行いません。
/// </summary>
public class OutputPathBuilder(IOptions<NotionExportOptions> _config)
{
    /// <summary>
    /// ページプロパティをもとに出力ディレクトリのパスを組み立てます。
    /// </summary>
    /// <param name="pageProperty">パス構築に使用するページプロパティ</param>
    /// <returns>出力ディレクトリの絶対パス</returns>
    /// <exception cref="InvalidOperationException">
    /// <see cref="PageProperty.PublishedDateTime"/> が null の場合にスローします。
    /// </exception>
    public string Build(PageProperty pageProperty)
    {
        if (!pageProperty.PublishedDateTime.HasValue)
        {
            throw new InvalidOperationException(
                $"公開日時が設定されていません。PageId={pageProperty.PageId}");
        }

        var slug = !string.IsNullOrEmpty(pageProperty.Slug)
            ? pageProperty.Slug
            : pageProperty.Title;

        var template = Template.Parse(_config.Value.OutputDirectoryPathTemplate);
        return template.Render(new
        {
            publish = pageProperty.PublishedDateTime.Value,
            title = pageProperty.Title,
            slug
        });
    }
}
