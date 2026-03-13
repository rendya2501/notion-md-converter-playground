using Microsoft.Extensions.Options;
using NotionMarkdownConverter.Application.Configuration;
using NotionMarkdownConverter.Shared.Models;
using Scriban;

namespace NotionMarkdownConverter.Infrastructure.FileSystem;

/// <summary>
/// 出力ディレクトリのパスを構築し、ディレクトリを作成するサービス。
/// <see cref="NotionExportOptions.OutputDirectoryPathTemplate"/> のテンプレートを展開してパスを生成します。
/// </summary>
/// <remarks>
/// パスの構築とディレクトリ作成をセットで行います。
/// 現時点では呼び出し元（NotionExporter）が常にセットで必要とするため一本化しています。
/// パスのみ取得したいケースが生じた場合は、
/// IOutputPathBuilder / IOutputDirectoryCreator への分離を検討してください。
/// </remarks>
public class OutputDirectoryProvider(IOptions<NotionExportOptions> _config) : IOutputDirectoryProvider
{
    /// <summary>
    /// ページプロパティをもとに出力ディレクトリのパスを構築し、ディレクトリを作成します。
    /// </summary>
    /// <param name="pageProperty">パス構築に使用するページプロパティ</param>
    /// <returns>作成した出力ディレクトリの絶対パス</returns>
    /// <exception cref="InvalidOperationException">
    /// <see cref="PageProperty.PublishedDateTime"/> が null の場合にスローします。
    /// 呼び出し元（PageExportEligibilityChecker.ShouldExport）でガード済みですが、
    /// 将来の別の呼び出し元からの誤用を防ぐために明示的にチェックしています。
    /// </exception>
    public string BuildAndCreate(PageProperty pageProperty)
    {
        // PageExportEligibilityChecker.ShouldExportでガード済みだが、このクラス単体で呼ばれた場合の誤用を防ぐ。
        if (!pageProperty.PublishedDateTime.HasValue)
        {
            throw new InvalidOperationException(
                $"公開日時が設定されていません。PageId={pageProperty.PageId}");
        }

        // スラグが未設定の場合はタイトルをフォールバックとして使用します。
        // Notionページにスラグプロパティがない運用でも動作するようにしています。
        var slug = !string.IsNullOrEmpty(pageProperty.Slug)
            ? pageProperty.Slug
            : pageProperty.Title;

        // Scribanテンプレートを展開して出力ディレクトリのパスを生成します。
        // テンプレート例: "content/{{ publish | date.to_string '%Y' }}/{{ slug }}"
        var template = Template.Parse(_config.Value.OutputDirectoryPathTemplate);
        var outputDirectory = template.Render(new
        {
            publish = pageProperty.PublishedDateTime.Value,
            title = pageProperty.Title,
            slug
        });

        // パスを構築した後にディレクトリを作成します。
        // 既に存在する場合は何もしません（Directory.CreateDirectoryは冪等）。
        Directory.CreateDirectory(outputDirectory);

        return outputDirectory;
    }
}