using MediatR;
using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// ファイル変換ストラテジー
/// </summary>
public class FileTransformStrategy(IMediator _mediator) : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.File;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformState context)
    {
        // ファイルブロックを取得
        var block = BlockConverter.GetOriginalBlock<FileBlock>(context.CurrentBlock);

        // ファイルのURLを取得
        var url = string.Empty;
        switch (block.File)
        {
            case ExternalFile externalFile:
                url = externalFile.External.Url;
                break;
            case UploadedFile uploadedFile:
                url = uploadedFile.File.Url;
                // アップロードしたファイルのみダウンロードURLをDownloadLinkCollectorに通知
                _mediator.Publish(new FileDownloadNotification(url)).GetAwaiter().GetResult();
                break;
        }

        // キャプションを取得
        var caption = block.File.Caption.Any()
            ? MarkdownUtils.RichTextsToMarkdown(block.File.Caption)
            : block.File.Name;

        // リンクを生成して改行を追加
        return MarkdownUtils.LineBreak(
            MarkdownUtils.Link(caption, url));
    }
}
