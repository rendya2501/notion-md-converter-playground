using Notion.Client;
using NotionMarkdownConverter.Domain.Constants;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// 画像変換ストラテジー
/// </summary>
public class ImageTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Image;

    public string Transform(NotionBlockTransformContext context)
    {
        // 画像ブロックを取得
        var block = BlockAccessor.GetOriginalBlock<ImageBlock>(context.CurrentBlock);
        // 画像のタイトルを取得
        var title = MarkdownRichTextUtils.RichTextsToMarkdown(block.Image.Caption);
        // 画像のURLを取得
        var url = block.Image switch
        {
            ExternalFile externalFile => externalFile.External.Url,
            UploadedFile uploadedFile => LinkConstants.DownloadMarker + uploadedFile.File.Url,
            _ => string.Empty
        };

        // 画像シグネチャを生成して改行を追加
        return MarkdownBlockUtils.LineBreak(
            MarkdownInlineUtils.Image(title, url));
    }
}