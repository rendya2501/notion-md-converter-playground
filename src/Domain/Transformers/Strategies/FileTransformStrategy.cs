using Notion.Client;
using NotionMarkdownConverter.Domain.Constants;
using NotionMarkdownConverter.Domain.Markdown.Utils;
using NotionMarkdownConverter.Domain.Transformers.Context;
using NotionMarkdownConverter.Domain.Transformers.Strategies.Abstractions;
using NotionMarkdownConverter.Domain.Utils;

namespace NotionMarkdownConverter.Domain.Transformers.Strategies;

/// <summary>
/// ファイル変換ストラテジー
/// </summary>
public class FileTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.File;

    public string Transform(NotionBlockTransformContext context)
    {
        // ファイルブロックを取得
        var block = BlockCaster.GetOriginalBlock<FileBlock>(context.CurrentBlock);
        // キャプションを取得
        var caption = block.File.Caption.Any()
            ? MarkdownRichTextUtils.RichTextsToMarkdown(block.File.Caption)
            : block.File.Name;
        // ファイルのURLを取得
        var url = block.File switch
        {
            ExternalFile externalFile => externalFile.External.Url,
            UploadedFile uploadedFile => LinkConstants.DownloadMarker + uploadedFile.File.Url,
            _ => string.Empty
        };

        // リンクを生成して改行を追加
        return MarkdownBlockUtils.LineBreak(
            MarkdownInlineUtils.Link(caption, url));
    }
}
