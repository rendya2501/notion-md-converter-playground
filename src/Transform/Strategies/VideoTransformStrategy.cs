using Notion.Client;
using NotionMarkdownConverter.Shared.Constants;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// ビデオ変換ストラテジー
/// </summary>
/// <remarks>
/// UploadedFileはダウンロードしてローカルパスに置換します。
/// ExternalFile（YouTube等）はURLをそのまま出力します。
/// ZennはYouTubeのURLを自動で埋め込みに変換し、その他のURLはリンクカードとして処理します。
/// </remarks>
public class VideoTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Video;

    public string Transform(NotionBlockTransformContext context)
    {
        var videoBlock = BlockAccessor.GetOriginalBlock<VideoBlock>(context.CurrentBlock);

        return videoBlock.Video switch
        {
            // Notionにアップロードされたファイルはダウンロードしてローカルパスに置換
            UploadedFile uploadedFile => MarkdownInlineUtils.AppendTrailingSpaces(
                MarkdownInlineUtils.Link(
                    Path.GetFileName(new Uri(uploadedFile.File.Url).LocalPath),
                    LinkConstants.DownloadMarker + uploadedFile.File.Url)),

            // 外部URL（YouTube等）はそのまま出力（Zennが処理）
            ExternalFile externalFile => MarkdownInlineUtils.AppendTrailingSpaces(externalFile.External.Url),

            _ => string.Empty
        };
    }
}
