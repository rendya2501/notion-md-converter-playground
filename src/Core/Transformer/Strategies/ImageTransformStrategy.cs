using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;
using NotionMarkdownConverter.Infrastructure.Http.Services;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// 画像変換ストラテジー
/// </summary>
public class ImageTransformStrategy(IFileDownloader _fileDownloader)
    : IBlockTransformStrategy, IEventSubscriber<OutputDirectoryChangedEvent>
{
    private string? _outputDirectory;

    public BlockType BlockType => BlockType.Image;

    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>

    public void OnEvent(OutputDirectoryChangedEvent eventData)
    {
        _outputDirectory = eventData.OutputDirectory;
    }

    ///  <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformState context)
    {
        if (string.IsNullOrEmpty(_outputDirectory))
            throw new InvalidOperationException("Output directory is not set.");

        // 画像ブロックを取得
        var block = BlockConverter.GetOriginalBlock<ImageBlock>(context.CurrentBlock);
        // 画像のURLを取得
        var url = block.Image switch
        {
            ExternalFile externalFile => externalFile.External.Url,
            UploadedFile uploadedFile => uploadedFile.File.Url,
            _ => string.Empty
        };

        var aaa = _fileDownloader.DownloadAsync(url, _outputDirectory);

        // 画像のタイトルを取得
        var title = MarkdownUtils.RichTextsToMarkdown(block.Image.Caption);

        // 画像シグネチャを生成して改行を追加
        return MarkdownUtils.LineBreak(
            MarkdownUtils.Image(title, url));
    }
}