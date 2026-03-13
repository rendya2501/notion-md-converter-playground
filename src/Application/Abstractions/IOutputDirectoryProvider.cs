using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Application.Abstractions;

/// <summary>
/// 出力ディレクトリのパスを構築し、ディレクトリを作成するサービスのインターフェース。
/// </summary>
/// <remarks>
/// パスの構築とディレクトリ作成をセットで行います。
/// 現時点では呼び出し元（NotionExporter）が常にセットで必要とするため一本化しています。
/// パスのみ取得したいケースが生じた場合は、
/// IOutputPathBuilder / IOutputDirectoryCreator への分離を検討してください。
/// </remarks>
public interface IOutputDirectoryProvider
{
    /// <summary>
    /// ページプロパティをもとに出力ディレクトリのパスを構築し、ディレクトリを作成します。
    /// </summary>
    /// <param name="pageProperty">パス構築に使用するページプロパティ</param>
    /// <returns>作成した出力ディレクトリの絶対パス</returns>
    /// <exception cref="InvalidOperationException">
    /// <see cref="PageProperty.PublishedDateTime"/> が null の場合にスローします。
    /// </exception>
    string BuildAndCreate(PageProperty pageProperty);
}
