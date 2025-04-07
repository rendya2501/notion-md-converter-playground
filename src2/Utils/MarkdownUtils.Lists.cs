namespace hoge.Utils;

public static partial class MarkdownUtils
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