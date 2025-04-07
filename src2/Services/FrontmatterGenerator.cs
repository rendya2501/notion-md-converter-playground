using hoge.Models;
using System.Text;

namespace hoge.Services;

/// <summary>
/// フロントマターを生成するクラスです。
/// </summary>
public class FrontmatterGenerator : IFrontmatterGenerator
{
    private const string TitleName = "title";
    private const string TypeName = "type";
    private const string PublishedName = "date";
    private const string DescriptionName = "description";
    private const string TagsName = "tags";

    /// <summary>
    /// フロントマターを生成します。
    /// </summary>
    /// <param name="pageData"></param>
    /// <returns></returns>
    public StringBuilder GenerateFrontmatter(PageProperty pageData)
    {
        var sb = new StringBuilder();

        sb.AppendLine("---");

        // タイプがあれば追加
        if (!string.IsNullOrWhiteSpace(pageData.Type))
        {
            sb.AppendLine($"{TypeName}: \"{pageData.Type}\"");
        }

        // タイトルは必須
        sb.AppendLine($"{TitleName}: \"{pageData.Title}\"");

        // 説明文があれば追加
        if (!string.IsNullOrWhiteSpace(pageData.Description))
        {
            sb.AppendLine($"{DescriptionName}: \"{pageData.Description}\"");
        }

        // タグがあれば追加
        if (pageData.Tags.Count > 0)
        {
            var formattedTags = pageData.Tags.Select(tag => $"\"{tag}\"");
            sb.AppendLine($"{TagsName}: [{string.Join(',', formattedTags)}]");
        }

        // 公開日時があれば追加
        if (pageData.PublishedDateTime.HasValue)
        {
            sb.AppendLine($"{PublishedName}: \"{pageData.PublishedDateTime.Value:s}\"");
        }

        sb.AppendLine("---");
        sb.AppendLine();

        return sb;
    }
}
