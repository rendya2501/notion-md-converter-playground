using System.Text;
using System.Text.RegularExpressions;

namespace hoge.Extensions;

/// <summary>
/// Regexの拡張メソッド
/// </summary>
public static class RegexExtensions
{
    /// <summary>
    /// 非同期で正規表現の置換を実行します
    /// </summary>
    /// <param name="regex">正規表現</param>
    /// <param name="input">入力文字列</param>
    /// <param name="replacementFn">置換関数</param>
    /// <returns>置換後の文字列</returns>
    public static async Task<string> ReplaceAsync(this Regex regex, string input, Func<Match, Task<string>> replacementFn)
    {
        var matches = regex.Matches(input);
        var replacements = new Dictionary<Match, string>();

        foreach (Match match in matches)
        {
            replacements[match] = await replacementFn(match);
        }

        var result = new StringBuilder(input);
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            result.Remove(match.Index, match.Length);
            result.Insert(match.Index, replacements[match]);
        }

        return result.ToString();
    }
}
