using System.Text;

namespace hoge.Utils;

/// <summary>
/// HTMLユーティリティ
/// </summary>
public static class HTMLUtils
{
    public static string ObjectToPropertiesStr(Dictionary<string, string> props)
    {
        if (props == null || props.Count == 0)
            return string.Empty;

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