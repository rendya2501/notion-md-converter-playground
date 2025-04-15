using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// ビデオ変換ストラテジー
/// </summary>
public class VideoTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Video;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformState context)
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
        return MarkdownUtils.Video(url);
    }
}