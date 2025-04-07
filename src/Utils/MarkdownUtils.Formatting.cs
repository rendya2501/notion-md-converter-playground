using Notion.Client;

namespace hoge.Utils;

public static partial class MarkdownUtils
{
    /// <summary>
    /// 色付け変換
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

    /// <summary>
    /// リンク変換
    /// </summary>
    public static string Link(string text, string url)
    {
        return $"[{text}]({url})";
    }

    /// <summary>
    /// 画像変換
    /// </summary>
    public static string Image(string text, string url, string? width = null)
    {
        var urlText = url;
        if (!string.IsNullOrEmpty(width))
        {
            urlText += $" ={width}x";
        }
        return $"![{text}]({urlText})";
    }

    /// <summary>
    /// videoタグに変換
    /// </summary>
    public static string Video(string url)
    {
        return $"<video controls src=\"{url}\"></video>";
    }

    /// <summary>
    /// コメント変換
    /// </summary>
    public static string Comment(string text)
    {
        return $"<!-- {text} -->";
    }

    /// <summary>
    /// 改行用のスペースを追加
    /// </summary>
    /// <param name="text">元のテキスト</param>
    /// <returns>改行用スペースが追加されたテキスト</returns>
    public static string WithLineBreak(string text)
    {
        return $"{text}  ";
    }

    // /// <summary>
    // /// 改行を追加
    // /// </summary>
    // /// <param name="text">元のテキスト</param>
    // /// <returns>改行が追加されたテキスト</returns>
    // public static string WithNewLine(string text)
    // {
    //     return $"{text}\n";
    // }

    // /// <summary>
    // /// 改行用のスペースと改行を追加
    // /// </summary>
    // /// <param name="text">元のテキスト</param>
    // /// <returns>改行用スペースと改行が追加されたテキスト</returns>
    // public static string WithLineBreakAndNewLine(string text)
    // {
    //     return $"{text}  \n";
    // }
} 