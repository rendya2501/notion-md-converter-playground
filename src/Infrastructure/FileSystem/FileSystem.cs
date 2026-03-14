using System.Text;

namespace NotionMarkdownConverter.Infrastructure.FileSystem;

/// <summary>
/// <see cref="IFileSystem"/> の実装。
/// </summary>
public class FileSystem : IFileSystem
{
    /// <inheritdoc/>
    public void CreateDirectory(string path)
        => Directory.CreateDirectory(path);

    /// <inheritdoc/>
    public Task WriteAllTextAsync(string path, string content, Encoding encoding)
        => File.WriteAllTextAsync(path, content, encoding);
}
