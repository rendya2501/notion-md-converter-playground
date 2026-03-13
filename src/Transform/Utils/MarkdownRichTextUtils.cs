using Notion.Client;
using NotionMarkdownConverter.Transform.Types;
using System.Text;

namespace NotionMarkdownConverter.Transform.Utils;

/// <summary>
/// NotionリッチテキストのMarkdown変換ユーティリティ
/// </summary>
public static class MarkdownRichTextUtils
{
    /// <summary>
    /// リッチテキストアノテーション有効化設定
    /// </summary>
    public class AnnotationOptions
    {
        public bool Bold { get; set; } = true;
        public bool Italic { get; set; } = true;
        public bool Strikethrough { get; set; } = true;
        public bool Underline { get; set; } = true;
        public bool Code { get; set; } = true;
        public bool Equation { get; set; } = true;
        public bool Link { get; set; } = true;
        public bool Color { get; set; } = false;
    }

    /// <summary>
    /// リッチテキストをMarkdownに変換
    /// </summary>
    public static string RichTextsToMarkdown(
        IEnumerable<RichTextBase> richTexts,
        AnnotationOptions? enableAnnotations = null,
        ColorMap? colorMap = null)
    {
        enableAnnotations ??= new AnnotationOptions();
        var result = new StringBuilder();

        foreach (var text in richTexts)
        {
            var markdown = text.PlainText;

            if (text.Annotations.IsCode && enableAnnotations.Code)
            {
                markdown = MarkdownInlineUtils.InlineCode(markdown);
            }

            if (text.Type == RichTextType.Equation && enableAnnotations.Equation)
            {
                markdown = MarkdownInlineUtils.InlineEquation(markdown);
            }

            if (text.Annotations.IsBold && text.Annotations.IsItalic
                && enableAnnotations.Bold && enableAnnotations.Italic)
            {
                markdown = MarkdownInlineUtils.BoldItalic(markdown);
            }
            else if (text.Annotations.IsBold && enableAnnotations.Bold)
            {
                markdown = MarkdownInlineUtils.Bold(markdown);
            }
            else if (text.Annotations.IsItalic && enableAnnotations.Italic)
            {
                markdown = MarkdownInlineUtils.Italic(markdown);
            }

            if (text.Annotations.IsStrikeThrough && enableAnnotations.Strikethrough)
            {
                markdown = MarkdownInlineUtils.Strikethrough(markdown);
            }

            if (text.Annotations.IsUnderline && enableAnnotations.Underline)
            {
                markdown = MarkdownInlineUtils.Underline(markdown);
            }

            if (text.Annotations.Color is not Color.Default && enableAnnotations.Color)
            {
                markdown = MarkdownInlineUtils.Color(markdown, text.Annotations.Color, colorMap);
            }

            if (!string.IsNullOrEmpty(text.Href) && IsUrl(text.Href) && enableAnnotations.Link)
            {
                markdown = MarkdownInlineUtils.Link(markdown, text.Href);
            }

            result.Append(markdown);
        }

        return result.ToString();
    }

    /// <summary>
    /// 文字列がURLかどうかを判定
    /// </summary>
    private static bool IsUrl(string? href)
        => !string.IsNullOrEmpty(href)
           && Uri.TryCreate(href, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}