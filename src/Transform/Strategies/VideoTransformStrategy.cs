using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// ビデオ変換ストラテジー
/// </summary>
/// <remarks>
/// ビデオの種類によって出力形式を切り替えます。
/// <list type="bullet">
///   <item>Notionにアップロードされたファイル → &lt;video&gt;タグ</item>
///   <item>YouTube / YouTube Shorts → &lt;iframe&gt;タグ（埋め込み用URLに変換）</item>
///   <item>その他の外部URL（直接再生可能な動画ファイル想定）→ &lt;video&gt;タグ</item>
///   <item>URLが取得できない場合 → Markdownリンク（空文字リンク）</item>
/// </list>
/// </remarks>
public class VideoTransformStrategy : IBlockTransformStrategy
{
    // YouTube の通常動画・ショート動画のホスト名
    private static readonly string[] YouTubeHosts =
        ["www.youtube.com", "youtube.com", "youtu.be"];

    public BlockType BlockType => BlockType.Video;

    public string Transform(NotionBlockTransformContext context)
    {
        var videoBlock = BlockAccessor.GetOriginalBlock<VideoBlock>(context.CurrentBlock);

        return videoBlock.Video switch
        {
            // Notionにアップロードされたファイルは直接再生できるため <video> タグで出力
            UploadedFile uploadedFile => MarkdownInlineUtils.Video(uploadedFile.File.Url),

            // 外部URLはホスト名で判定して出力形式を切り替える
            ExternalFile externalFile => RenderExternalVideo(externalFile.External.Url),

            // URLが取得できないケース（API仕様変更などによる予期しない型）
            _ => string.Empty
        };
    }

    /// <summary>
    /// 外部URLのホスト名に応じて出力形式を決定します。
    /// </summary>
    /// <param name="url">外部ビデオのURL</param>
    /// <returns>Markdown/HTML文字列</returns>
    private static string RenderExternalVideo(string url)
    {
        // URLのパース失敗（不正なURL等）はMarkdownリンクにフォールバック
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return MarkdownInlineUtils.Link(url, url);
        }

        // YouTube系のURLは埋め込みURLに変換して <iframe> で出力
        if (IsYouTubeUrl(uri))
        {
            var embedUrl = ToYouTubeEmbedUrl(uri);
            return embedUrl is not null
                ? MarkdownInlineUtils.IFrame(embedUrl)
                : MarkdownInlineUtils.Link(url, url); // 埋め込みURLへの変換失敗時はリンクにフォールバック
        }

        // それ以外の外部URLは直接再生可能な動画ファイルとして <video> タグで出力
        return MarkdownInlineUtils.Video(url);
    }

    /// <summary>
    /// YouTube / YouTube Shorts のURLかどうかを判定します。
    /// </summary>
    private static bool IsYouTubeUrl(Uri uri) =>
        YouTubeHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// YouTube URLを埋め込み用URLに変換します。
    /// </summary>
    /// <remarks>
    /// 対応形式:
    /// <list type="bullet">
    ///   <item>https://www.youtube.com/watch?v=VIDEO_ID → https://www.youtube.com/embed/VIDEO_ID</item>
    ///   <item>https://youtu.be/VIDEO_ID → https://www.youtube.com/embed/VIDEO_ID</item>
    ///   <item>https://www.youtube.com/shorts/VIDEO_ID → https://www.youtube.com/embed/VIDEO_ID</item>
    /// </list>
    /// </remarks>
    /// <returns>埋め込みURL。動画IDが取得できない場合は <c>null</c></returns>
    private static string? ToYouTubeEmbedUrl(Uri uri)
    {
        var videoId = ExtractYouTubeVideoId(uri);
        return videoId is not null
            ? $"https://www.youtube.com/embed/{videoId}"
            : null;
    }

    /// <summary>
    /// YouTube URLから動画IDを抽出します。
    /// </summary>
    private static string? ExtractYouTubeVideoId(Uri uri)
    {
        // youtu.be/VIDEO_ID 形式
        if (uri.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase))
        {
            var id = uri.AbsolutePath.TrimStart('/');
            return string.IsNullOrEmpty(id) ? null : id;
        }

        // /shorts/VIDEO_ID 形式
        if (uri.AbsolutePath.StartsWith("/shorts/", StringComparison.OrdinalIgnoreCase))
        {
            var id = uri.AbsolutePath["/shorts/".Length..];
            return string.IsNullOrEmpty(id) ? null : id;
        }

        // ?v=VIDEO_ID 形式
        // System.Web への依存を避けるため、手動でクエリ文字列をパースします。
        return uri.Query.TrimStart('?')
            .Split('&')
            .Select(p => p.Split('=', 2))
            .Where(p => p.Length == 2 && p[0] == "v")
            .Select(p => p[1])
            .FirstOrDefault();
    }
}
