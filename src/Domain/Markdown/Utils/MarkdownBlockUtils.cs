using NotionMarkdownConverter.Domain.Markdown.Enums;

namespace NotionMarkdownConverter.Domain.Markdown.Utils;

/// <summary>
/// Markdownブロックレベル要素の変換ユーティリティ
/// </summary>
public static class MarkdownBlockUtils
{
    /// <summary>
    /// 見出しを作成します。
    /// </summary>
    /// <param name="text">見出しのテキスト</param>
    /// <param name="level">見出しレベル（1〜6）。範囲外は自動的にクランプされます。</param>
    /// <returns>Markdown形式の見出し文字列</returns>
    public static string Heading(string text, int level)
    {
        if (level < 1)
        {
            level = 1;
        }
        if (level > 6)
        {
            level = 6;
        }
        return $"{new string('#', level)} {text}";
    }

    /// <summary>
    /// コードブロックに変換します。
    /// </summary>
    /// <param name="code">コードの内容</param>
    /// <param name="language">シンタックスハイライト用の言語名。省略可能。</param>
    /// <returns>Markdown形式のコードブロック文字列</returns>
    public static string CodeBlock(string code, string? language = null)
    {
        return $"``` {language ?? string.Empty}\n{code}\n```";
    }

    /// <summary>
    /// ブロック数式に変換します。
    /// </summary>
    /// <param name="equation">数式の内容</param>
    /// <returns>Markdown形式のブロック数式文字列</returns>
    public static string BlockEquation(string equation)
    {
        return $"$$\n{equation}\n$$";
    }

    /// <summary>
    /// 引用ブロックに変換します。複数行の場合、各行に引用記号を付与します。
    /// </summary>
    /// <param name="text">引用するテキスト</param>
    /// <returns>Markdown形式の引用ブロック文字列</returns>
    public static string Blockquote(string text)
    {
        var lines = text.Split('\n');
        return string.Join("\n", lines.Select(line => $"> {line}"));
    }

    /// <summary>
    /// 水平線に変換します。
    /// </summary>
    /// <param name="style">水平線のスタイル。デフォルトはハイフン。</param>
    /// <returns>Markdown形式の水平線文字列</returns>
    public static string HorizontalRule(HorizontalRuleStyle? style = HorizontalRuleStyle.Hyphen)
    {
        return style switch
        {
            HorizontalRuleStyle.Hyphen => "---",
            HorizontalRuleStyle.Asterisk => "***",
            HorizontalRuleStyle.Underscore => "___",
            _ => "---"
        };
    }

    /// <summary>
    /// 文字列の各行にインデントを追加します。空行はインデントしません。
    /// </summary>
    /// <param name="text">インデントするテキスト</param>
    /// <param name="spaces">インデントのスペース数。デフォルトは2。</param>
    /// <returns>インデントされた文字列</returns>
    public static string Indent(string text, int spaces = 2)
    {
        var lines = text.Split('\n');
        var indentedLines = lines.Select(line =>
            string.IsNullOrEmpty(line)
                ? line
                : $"{new string(' ', spaces)}{line}");
        return string.Join("\n", indentedLines);
    }

    /// <summary>
    /// 文字列の各行末にMarkdown改行用スペースを追加します。空白のみの行はスキップします。
    /// </summary>
    /// <param name="text">処理するテキスト</param>
    /// <param name="lineBreakType">改行スタイル。デフォルトは改行コード（\n）。</param>
    /// <returns>行末にスペースが追加された文字列</returns>
    public static string LineBreak(string text, LineBreakStyle lineBreakType = LineBreakStyle.Newline)
    {
        var lineBreak = lineBreakType switch
        {
            LineBreakStyle.Newline => "\n",
            LineBreakStyle.BR => "<BR>",
            _ => "\n"
        };

        var lines = text.Split('\n');
        var indentedLines = lines.Select(line =>
            string.IsNullOrWhiteSpace(line)
                ? line
                : MarkdownInlineUtils.WithLineBreak(line));
        return string.Join(lineBreak, indentedLines);
    }

    /// <summary>
    /// HTMLのdetailsタグに変換します。
    /// </summary>
    /// <param name="title">summaryに表示するタイトルテキスト</param>
    /// <param name="content">折りたたまれるコンテンツ</param>
    /// <returns>details/summaryタグで囲まれた文字列</returns>
    public static string Details(string title, string content)
    {
        // summaryでインデントを入れるとnest構造がおかしくなる時があるので、インデントを入れない
        var result = new[]
        {
            "<details>",
            "<summary>",
            title,
            "</summary>",
            string.Empty, // 改行
            content,
            "</details>",
        };
        return string.Join("\n", result);
    }
}
