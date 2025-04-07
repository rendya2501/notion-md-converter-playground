using Notion.Client;
using System.Text;

namespace hoge.Utils;

public static partial class MarkdownUtils
{
    /// <summary>
    /// リッチテキストをMarkdownに変換
    /// </summary>
    public static string RichTextsToMarkdown(
        IEnumerable<RichTextBase> richTexts,
        EnableAnnotations? enableAnnotations = null,
        ColorMap? colorMap = null)
    {
        enableAnnotations ??= new EnableAnnotations();
        var result = new StringBuilder();

        foreach (var text in richTexts)
        {
            var markdown = text.PlainText;

            // コードブロックの場合
            if (text.Annotations.IsCode && enableAnnotations.Code)
            {
                markdown = InlineCode(markdown);
            }

            // 数式の場合
            if (text.Type == RichTextType.Equation && enableAnnotations.Equation)
            {
                markdown = InlineEquation(markdown);
            }

            // 太字の場合
            if (text.Annotations.IsBold && enableAnnotations.Bold)
            {
                markdown = Bold(markdown);
            }

            // 斜体の場合
            if (text.Annotations.IsItalic && enableAnnotations.Italic)
            {
                markdown = Italic(markdown);
            }

            // 打ち消し線の場合
            if (text.Annotations.IsStrikeThrough && enableAnnotations.Strikethrough)
            {
                markdown = Strikethrough(markdown);
            }

            // 下線の場合
            if (text.Annotations.IsUnderline && enableAnnotations.Underline)
            {
                markdown = Underline(markdown);
            }

            // 色の場合
            if (text.Annotations.Color is not Notion.Client.Color.Default && enableAnnotations.Color)
            {
                markdown = Color(markdown, text.Annotations.Color, COLOR_MAP);
            }

            // リンクの場合
            if (!string.IsNullOrEmpty(text.Href) && Utils.IsURL(text.Href) && enableAnnotations.Link)
            {
                markdown = Link(markdown, text.Href);
            }

            result.Append(markdown);
        }

        return result.ToString();
    }
}