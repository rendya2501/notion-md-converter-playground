namespace NotionMarkdownConverter.Core.Utils;

/// <summary>
/// 一般ユーティリティ
/// </summary>
public static class CommonUtils
{
    /// <summary>
    /// 文字列がURLかどうかを判断します。
    /// </summary>
    /// <param name="text">文字列</param>
    /// <returns>URLかどうか</returns>
    public static bool IsURL(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        return Uri.TryCreate(text, UriKind.Absolute, out Uri? uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
} 