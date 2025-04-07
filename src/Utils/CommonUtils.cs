namespace hoge.Utils;

/// <summary>
/// 一般ユーティリティ
/// </summary>
public static class Utils
{
    public static bool IsURL(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        return Uri.TryCreate(text, UriKind.Absolute, out Uri? uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
} 