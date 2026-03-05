using Notion.Client;
using NotionMarkdownConverter.Core.Utils;
using NotionMarkdownConverter.Domain.Constants;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// PDF変換ストラテジー
/// </summary>
public class PDFTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.PDF;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        var block = BlockConverter.GetOriginalBlock<PDFBlock>(context.CurrentBlock);

        // キャプションがあればキャプションを、なければファイル名 or URLを表示テキストとして使用
        var caption = block.PDF.Caption.Any()
            ? MarkdownUtils.RichTextsToMarkdown(block.PDF.Caption)
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

        return MarkdownUtils.LineBreak(
            MarkdownUtils.Link(displayText, url));
    }
}