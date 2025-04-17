using MediatR;
using Notion.Client;
using NotionMarkdownConverter.Core.Services.Test;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// 画像変換ストラテジー
/// </summary>
public class ImageTransformStrategy(IMediator _mediator, IMarkdownLinkProcessor2 _linkProcessor) : IBlockTransformStrategy
{
    public BlockType BlockType => BlockType.Image;

    ///  <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformState context)
    {
        // 画像ブロックを取得
        var block = BlockConverter.GetOriginalBlock<ImageBlock>(context.CurrentBlock);

        // 画像のURLを取得
        var url = string.Empty;
        switch (block.Image)
        {
            case ExternalFile externalFile:
                url = externalFile.External.Url;
                break;
            case UploadedFile uploadedFile:
                url = uploadedFile.File.Url;
                // アップロードした画像ファイルのみダウンロードURLをDownloadLinkCollectorに通知
                _mediator.Publish(new FileDownloadNotification(url)).GetAwaiter().GetResult();

                //var outputDirectory = context.OutputDirectory;
                //url = _linkProcessor.ProcessLinksAsync(uploadedFile.File.Url, outputDirectory);
                break;
        }

        // 画像のタイトルを取得
        var title = MarkdownUtils.RichTextsToMarkdown(block.Image.Caption);

        // 画像シグネチャを生成して改行を追加
        return MarkdownUtils.LineBreak(
            MarkdownUtils.Image(title, url));
    }
}