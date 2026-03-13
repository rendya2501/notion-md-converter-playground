using NotionMarkdownConverter.Shared.Models;
using System.Text;

namespace NotionMarkdownConverter.Domain.Markdown.Converters;

/// <summary>
/// Notionページのプロパティ情報をMarkdownのフロントマターに変換します。
/// </summary>
public class FrontmatterConverter
{
    private const string TitleName = "title";
    private const string TypeName = "type";
    private const string PublishedName = "date";
    private const string DescriptionName = "description";
    private const string TagsName = "tags";

    /// <summary>
    /// ページプロパティをMarkdownフロントマター文字列に変換します。
    /// </summary>
    /// <param name="pageProperty">変換元のページプロパティ</param>
    /// <returns>
    /// <c>---</c> で囲まれたYAML形式のフロントマター文字列。
    /// 末尾には空行が含まれます。
    /// </returns>
    public string Convert(PageProperty pageProperty)
    {
        var sb = new StringBuilder();

        sb.AppendLine("---");

        // タイプがあれば追加
        if (!string.IsNullOrWhiteSpace(pageProperty.Type))
        {
            sb.AppendLine($"{TypeName}: \"{pageProperty.Type}\"");
        }

        // タイトルは必須
        sb.AppendLine($"{TitleName}: \"{pageProperty.Title}\"");

        // 説明文があれば追加
        if (!string.IsNullOrWhiteSpace(pageProperty.Description))
        {
            sb.AppendLine($"{DescriptionName}: \"{pageProperty.Description}\"");
        }

        // タグがあれば追加
        if (pageProperty.Tags.Count > 0)
        {
            var formattedTags = pageProperty.Tags.Select(tag => $"\"{tag}\"");
            sb.AppendLine($"{TagsName}: [{string.Join(',', formattedTags)}]");
        }

        // 公開日時があれば追加
        if (pageProperty.PublishedDateTime.HasValue)
        {
            sb.AppendLine($"{PublishedName}: \"{pageProperty.PublishedDateTime.Value:s}\"");
        }

        sb.AppendLine("---");
        sb.AppendLine();

        return sb.ToString();
    }
}
