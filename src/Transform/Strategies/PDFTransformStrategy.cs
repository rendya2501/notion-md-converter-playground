using Notion.Client;
using NotionMarkdownConverter.Shared.Constants;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Context;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// PDF変換ストラテジー
/// </summary>
public class PDFTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.PDF;

    public string Transform(NotionBlockTransformContext context)
    {
        var block = BlockAccessor.GetOriginalBlock<PDFBlock>(context.CurrentBlock);

        // キャプションがあればキャプションを、なければファイル名 or URLを表示テキストとして使用
        var caption = block.PDF.Caption.Any()
            ? MarkdownRichTextUtils.RichTextsToMarkdown(block.PDF.Caption)
            : null;

        var url = block.PDF switch
        {
            ExternalFile externalFile => externalFile.External.Url,
            UploadedFile uploadedFile => LinkConstants.DownloadMarker + uploadedFile.File.Url,
            _ => string.Empty
        };

        // 表示テキストの決定
        var displayText = caption
            ?? (block.PDF is UploadedFile uploaded
                ? Path.GetFileName(new Uri(uploaded.File.Url).LocalPath)
                : url);

        return MarkdownBlockUtils.ApplyLineBreaks(
            MarkdownInlineUtils.Link(displayText, url));
    }
}