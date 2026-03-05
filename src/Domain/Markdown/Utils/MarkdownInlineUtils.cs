using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Types;
using NotionMarkdownConverter.Domain.Utils;
using System.Text.RegularExpressions;

namespace NotionMarkdownConverter.Domain.Markdown.Utils;

/// <summary>
/// Markdownインライン要素変換ユーティリティ
/// </summary>
public static class MarkdownInlineUtils
{
    private static readonly ColorMap COLOR_MAP = new()
    {
        { Notion.Client.Color.Default, string.Empty },
        { Notion.Client.Color.Red, "#A83232" },
        { Notion.Client.Color.RedBackground, "#E8CCCC" },
        { Notion.Client.Color.Orange, "#C17F46" },
        { Notion.Client.Color.OrangeBackground, "#E8D5C2" },
        { Notion.Client.Color.Yellow, "#9B8D27" },
        { Notion.Client.Color.YellowBackground, "#E6E6C8" },
        { Notion.Client.Color.Brown, "#8B6C55" },
        { Notion.Client.Color.BrownBackground, "#E0D5CC" },
        { Notion.Client.Color.Green, "#4E7548" },
        { Notion.Client.Color.GreenBackground, "#D5E0D1" },
        { Notion.Client.Color.Blue, "#3A6B9F" },
        { Notion.Client.Color.BlueBackground, "#D0DEF0" },
        { Notion.Client.Color.Purple, "#6B5B95" },
        { Notion.Client.Color.PurpleBackground, "#D8D3E6" },
        { Notion.Client.Color.Pink, "#B5787D" },
        { Notion.Client.Color.PinkBackground, "#E8D5D8" },
        { Notion.Client.Color.Gray, "#777777" },
        { Notion.Client.Color.GrayBackground, "#D0D0D0" }
    };

    private static readonly Color[] BACKGROUND_COLOR_KEY =
    [
        Notion.Client.Color.RedBackground,
        Notion.Client.Color.OrangeBackground,
        Notion.Client.Color.YellowBackground,
        Notion.Client.Color.GreenBackground,
        Notion.Client.Color.BlueBackground,
        Notion.Client.Color.PurpleBackground,
        Notion.Client.Color.PinkBackground,
        Notion.Client.Color.GrayBackground,
        Notion.Client.Color.BrownBackground
    ];

    private static readonly Color[] TEXT_COLOR_KEY =
    [
        Notion.Client.Color.Red,
        Notion.Client.Color.Orange,
        Notion.Client.Color.Yellow,
        Notion.Client.Color.Green,
        Notion.Client.Color.Blue,
        Notion.Client.Color.Purple,
        Notion.Client.Color.Pink,
        Notion.Client.Color.Gray,
        Notion.Client.Color.Brown
    ];

    // ── テキスト装飾 ──────────────────────────────

    /// <summary>装飾の共通処理（前後の空白を保持）</summary>
    public static string Decoration(string text, string decoration)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var match = Regex.Match(text, @"^(\s*)(.+?)(\s*)$");
        if (!match.Success) return text;

        var leading = match.Groups[1].Value;
        var content = match.Groups[2].Value;
        var trailing = match.Groups[3].Value;
        return $"{leading}{decoration}{content}{decoration}{trailing}";
    }

    public static string Bold(string text) => Decoration(text, "**");
    public static string Italic(string text) => Decoration(text, "*");
    public static string BoldItalic(string text) => Decoration(text, "***");
    public static string Strikethrough(string text) => Decoration(text, "~~");
    public static string InlineCode(string text) => $"`{text}`";
    public static string Underline(string text) => $"_{text}_";
    public static string InlineEquation(string equation) => $"${equation}$";

    // ── リンク・メディア ──────────────────────────

    /// <summary>リンク変換</summary>
    public static string Link(string text, string url) => $"[{text}]({url})";


    /// <summary>画像変換</summary>
    public static string Image(string text, string url, string? width = null)
    {
        var urlText = string.IsNullOrEmpty(width) ? url : $"{url} ={width}x";
        return $"![{text}]({urlText})";
    }

    /// <summary>videoタグに変換</summary>
    public static string Video(string url) => $"<video controls src=\"{url}\"></video>";

    /// <summary>コメント変換</summary>
    public static string Comment(string text) => $"<!-- {text} -->";

    /// <summary>行末に改行用スペースを追加</summary>
    public static string WithLineBreak(string text) => $"{text}  ";

    // ── 色 ───────────────────────────────────────

    /// <summary>
    /// 色付きspanタグに変換
    /// </summary>
    public static string Color(string text, Color? color, ColorMap? colorMap = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        colorMap ??= COLOR_MAP;
        color ??= Notion.Client.Color.Default;

        var spanProps = new Dictionary<string, string>();

        if (BACKGROUND_COLOR_KEY.Any(w => w == color))
        {
            var bgColor = colorMap.TryGetValue((Color)color, out string? value) ? value : null;
            if (!string.IsNullOrEmpty(bgColor))
            {
                spanProps["style"] = $"background-color: {bgColor};";
            }
        }

        if (TEXT_COLOR_KEY.Any(w => w == color))
        {
            var textColor = colorMap.TryGetValue((Color)color, out string? value) ? value : null;
            if (!string.IsNullOrEmpty(textColor))
            {
                spanProps["style"] = $"color: {textColor};";
            }
        }

        if (spanProps.Count > 0)
        {
            var propsStr = HTMLUtils.ObjectToPropertiesStr(spanProps);
            return $"<span {propsStr}>{text}</span>";
        }

        return text;
    }
}
