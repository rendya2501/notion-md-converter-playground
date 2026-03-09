using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Types;
using System.Text;
using System.Text.RegularExpressions;

namespace NotionMarkdownConverter.Domain.Markdown.Utils;

/// <summary>
/// Markdownインライン要素変換ユーティリティ
/// </summary>
public static class MarkdownInlineUtils
{
    private static readonly ColorMap ColorMap = new()
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

    private static readonly Color[] BackgroundColorKeys =
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

    private static readonly Color[] TextColorKeys =
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
    /// <param name="text">装飾するテキスト</param>
    /// <param name="decoration">装飾に使用する記号（例: "**", "*"）</param>
    /// <returns>装飾されたMarkdown文字列。空白のみの場合はそのまま返します。</returns>
    public static string Decoration(string text, string decoration)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var match = Regex.Match(text, @"^(\s*)(.+?)(\s*)$");
        if (!match.Success)
        {
            return text;
        }

        var leading = match.Groups[1].Value;
        var content = match.Groups[2].Value;
        var trailing = match.Groups[3].Value;
        return $"{leading}{decoration}{content}{decoration}{trailing}";
    }

    /// <summary>テキストを太字に変換します。</summary>
    public static string Bold(string text) => Decoration(text, "**");
    /// <summary>テキストをイタリックに変換します。</summary>
    public static string Italic(string text) => Decoration(text, "*");
    /// <summary>テキストを太字イタリックに変換します。</summary>
    public static string BoldItalic(string text) => Decoration(text, "***");
    /// <summary>テキストに取り消し線を適用します。</summary>
    public static string Strikethrough(string text) => Decoration(text, "~~");
    /// <summary>テキストをインラインコードに変換します。</summary>
    public static string InlineCode(string text) => $"`{text}`";
    /// <summary>テキストに下線を適用します。</summary>
    public static string Underline(string text) => $"_{text}_";
    /// <summary>数式をインライン数式記法に変換します。</summary>
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
    public static string Video(string url) => 
        $"<video controls src=\"{url}\"></video>";

    /// <summary>iframeタグに変換</summary>
    public static string IFrame(string url) =>
        $"<iframe src=\"{url}\" allowfullscreen></iframe>";

    /// <summary>コメント変換</summary>
    public static string Comment(string text) => $"<!-- {text} -->";

    /// <summary>
    /// 1行のテキスト末尾にMarkdown改行用トレイリングスペース（半角スペース2つ）を追加します。
    /// </summary>
    /// <remarks>
    /// これはMarkdownの強制改行構文です。
    /// 複数行にまとめて適用したい場合は <see cref="MarkdownBlockUtils.ApplyLineBreaks"/> を使用してください。
    /// </remarks>
    /// <param name="text">処理するテキスト</param>
    /// <returns>末尾にスペース2つが追加されたテキスト</returns>
    public static string AppendTrailingSpaces(string text) => $"{text}  ";

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

        colorMap ??= ColorMap;
        color ??= Notion.Client.Color.Default;

        var spanProps = new Dictionary<string, string>();

        if (BackgroundColorKeys.Any(w => w == color))
        {
            var bgColor = colorMap.TryGetValue((Color)color, out string? value) ? value : null;
            if (!string.IsNullOrEmpty(bgColor))
            {
                spanProps["style"] = $"background-color: {bgColor};";
            }
        }

        if (TextColorKeys.Any(w => w == color))
        {
            var textColor = colorMap.TryGetValue((Color)color, out string? value) ? value : null;
            if (!string.IsNullOrEmpty(textColor))
            {
                spanProps["style"] = $"color: {textColor};";
            }
        }

        if (spanProps.Count > 0)
        {
            var propsStr = ObjectToPropertiesStr(spanProps);
            return $"<span {propsStr}>{text}</span>";
        }

        return text;
    }

    /// <summary>
    /// プロパティ辞書をHTML属性文字列に変換します。
    /// </summary>
    /// <param name="props">プロパティの辞書（値が空のエントリは無視されます）</param>
    /// <returns>HTML属性形式の文字列</returns>
    private static string ObjectToPropertiesStr(Dictionary<string, string> props)
    {
        // props は非nullable かつ呼び出し元でCount > 0 を確認済みのため、空チェックは不要
        var sb = new StringBuilder();
        foreach (var prop in props)
        {
            if (!string.IsNullOrEmpty(prop.Value))
            {
                sb.Append($"{prop.Key}=\"{prop.Value}\" ");
            }
        }
        return sb.ToString().TrimEnd();
    }
}
