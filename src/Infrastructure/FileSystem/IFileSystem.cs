using System.Text;

namespace NotionMarkdownConverter.Infrastructure.FileSystem;

/// <summary>
/// ファイルシステム操作の抽象。
/// テスト時のFake差し替えと、実装の一元管理を目的とします。
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// 指定したパスにディレクトリを作成します。
    /// 中間ディレクトリが存在しない場合も含めて再帰的に作成します。
    /// 既に存在する場合は何もしません。
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// 指定したパスにテキストを書き込みます。
    /// </summary>
    Task WriteAllTextAsync(string path, string content, Encoding encoding);
}
