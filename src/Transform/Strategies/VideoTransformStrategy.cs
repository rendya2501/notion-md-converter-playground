using Notion.Client;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Strategies.Context;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// ビデオ変換ストラテジー
/// </summary>
/// <remarks>
/// UploadedFile・ExternalFileともにURLをそのまま出力します。
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
            UploadedFile uploadedFile => uploadedFile.File.Url,
            ExternalFile externalFile => externalFile.External.Url,
            _ => string.Empty
        };
    }
}
