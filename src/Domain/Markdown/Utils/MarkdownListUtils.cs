using NotionMarkdownConverter.Domain.Markdown.Enums;

namespace NotionMarkdownConverter.Domain.Markdown.Utils;

/// <summary>
/// Markdownリスト変換ユーティリティ
/// </summary>
public static class MarkdownListUtils
{
    /// <summary>
    /// 箇条書きリスト変換
    /// </summary>
    public static string BulletList(string text, BulletStyle style = BulletStyle.Hyphen)
    {
        return $"{(char)style} {text}";
    }

    /// <summary>
    /// 番号付きリスト変換
    /// </summary>
    public static string NumberedList(string text, int number)
    {
        return $"{number}. {text}";
    }

    /// <summary>
    /// チェックリスト変換
    /// </summary>
    public static string CheckList(string text, bool isChecked)
    {
        return $"- [{(isChecked ? "x" : " ")}] {text}";
    }
} 
