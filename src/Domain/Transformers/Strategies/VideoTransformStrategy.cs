using Notion.Client;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// ビデオ変換ストラテジー
/// </summary>
public class VideoTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Video;

    public string Transform(NotionBlockTransformContext context)
    {
        // ビデオブロックを取得
        var videoBlock = BlockConverter.GetOriginalBlock<VideoBlock>(context.CurrentBlock);
        // ビデオのURLを取得
        var url = videoBlock.Video switch
        {
            ExternalFile externalFile => externalFile.External.Url,
            UploadedFile uploadedFile => uploadedFile.File.Url,
            _ => string.Empty
        };

        // ビデオのURLをMarkdown形式に変換
        return MarkdownInlineUtils.Video(url);
    }
}