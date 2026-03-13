using Notion.Client;
using NotionMarkdownConverter.Shared.Constants;
using NotionMarkdownConverter.Shared.Utils;
using NotionMarkdownConverter.Transform.Context;
using NotionMarkdownConverter.Transform.Strategies.Abstractions;
using NotionMarkdownConverter.Transform.Utils;

namespace NotionMarkdownConverter.Transform.Strategies;

/// <summary>
/// ファイル変換ストラテジー
/// </summary>
public class FileTransformStrategy : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.File;

    public string Transform(NotionBlockTransformContext context)
    {
        // ファイルブロックを取得
        var block = BlockAccessor.GetOriginalBlock<FileBlock>(context.CurrentBlock);
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
        return MarkdownBlockUtils.ApplyLineBreaks(
            MarkdownInlineUtils.Link(caption, url));
    }
}
