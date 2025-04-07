namespace hoge.Utils;

public static partial class MarkdownUtils
{
    /// <summary>
    /// 見出しを作成
    /// </summary>
    public static string Heading(string text, int level)
    {
        if (level < 1) level = 1;
        if (level > 6) level = 6;
        return $"{new string('#', level)} {text}";
    }

    /// <summary>
    /// コードブロック変換
    /// </summary>
    public static string CodeBlock(string code, string? language = null)
    {
        return $"``` {language ?? string.Empty}\n{code}\n```";
    }

    /// <summary>
    /// ブロック数式変換
    /// </summary>
    public static string BlockEquation(string equation)
    {
        return $"$$\n{equation}\n$$";
    }

    /// <summary>
    /// 引用変換
    /// </summary>
    public static string Blockquote(string text)
    {
        var lines = text.Split('\n');
        return string.Join("\n", lines.Select(line => $"> {line}"));
    }

    /// <summary>
    /// 水平線変換
    /// </summary>
    public static string HorizontalRule(HolizontalRuleStyle? style = HolizontalRuleStyle.Hyphen)
    {
        return style switch
        {
            HolizontalRuleStyle.Hyphen => "---",
            HolizontalRuleStyle.Asterisk => "***",
            HolizontalRuleStyle.Underscor => "___",
            _ => "---"
        };
    }

    /// <summary>
    /// 文字列の各行にインデントを追加
    /// </summary>
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
    /// 文字列の各行末に改行を追加
    /// </summary>
    /// <param name="richTexts"></param>
    /// <returns></returns>
    public static string LineBreak(string text)
    {
        var lines = text.Split('\n');
        var indentedLines = lines.Select(line =>
            string.IsNullOrWhiteSpace(line)
                ? line
                : WithLineBreak(line));
        return string.Join("\n", indentedLines);

        //var result = new StringBuilder();

        //// テキストが空の場合は改行を追加しない
        //if (texts.Length == 0)
        //{
        //    return texts;
        //}

        //var lines = texts.Split([Environment.NewLine, "\n"], StringSplitOptions.None);
        //foreach (var (line, index) in lines.Select((line, index) => (line, index)))
        //{
        //    // その行が空白の場合は処理終了
        //    if (string.IsNullOrWhiteSpace(line))
        //    {
        //        result.Append(line);
        //        result.Append('\n');
        //        continue;
        //    }

        //    // 文字列の末尾にマークダウンの改行であるスペース2つを追記
        //    result.Append(WithLineBreak(line));

        //    // 最後の要素に到達したらループを終了
        //    if (index == lines.Length - 1)
        //    {
        //        break;
        //    }

        //    result.AppendLine();
        //}

        //return result.ToString();
    }
    
    /// <summary>
    /// detailsタグに変換
    /// </summary>
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