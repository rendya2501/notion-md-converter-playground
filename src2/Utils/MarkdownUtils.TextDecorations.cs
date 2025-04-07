using System.Text.RegularExpressions;

namespace hoge.Utils;

public static partial class MarkdownUtils
{
    /// <summary>
    /// テキスト装飾関数（共通処理）
    /// </summary>
    public static string Decoration(string text, string decoration)
    {
        // 空文字列や空白のみの場合は処理しない
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        // 正規表現を使って先頭と末尾の空白をキャプチャしながら内部テキストも取得
        var match = Regex.Match(text, @"^(\s*)(.+?)(\s*)$");

        if (!match.Success)
        {
            return text; // マッチしない場合は元のテキストを返す
        }

        var leadingSpaces = match.Groups[1].Value;
        var content = match.Groups[2].Value;
        var trailingSpaces = match.Groups[3].Value;

        // 前後の空白を保持しつつ、内部テキストを装飾記号で囲む
        return $"{leadingSpaces}{decoration}{content}{decoration}{trailingSpaces}";
    }

    /// <summary>
    /// 太字変換
    /// </summary>
    public static string Bold(string text)
    {
        return Decoration(text, "**");
    }

    /// <summary>
    /// イタリック変換
    /// </summary>
    public static string Italic(string text)
    {
        return Decoration(text, "*");
    }

    /// <summary>
    /// 取り消し線変換
    /// </summary>
    public static string Strikethrough(string text)
    {
        return Decoration(text, "~~");
    }

    /// <summary>
    /// インラインコード変換
    /// </summary>
    public static string InlineCode(string text)
    {
        return $"`{text}`";
    }

    /// <summary>
    /// 下線変換
    /// </summary>
    public static string Underline(string text)
    {
        return $"_{text}_";
    }

    /// <summary>
    /// インライン数式変換
    /// </summary>
    public static string InlineEquation(string equation)
    {
        return $"${equation}$";
    }
} 