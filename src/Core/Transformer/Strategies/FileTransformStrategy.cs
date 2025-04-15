using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// ファイル変換ストラテジー
/// </summary>
public class FileTransformStrategy : IBlockTransformStrategy
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
        var url = block.File switch
        {
            ExternalFile externalFile => externalFile.External.Url,
            UploadedFile uploadedFile => uploadedFile.File.Url,
            _ => string.Empty
        };
        // キャプションを取得
        var caption = block.File.Caption.Any()
            ? MarkdownUtils.RichTextsToMarkdown(block.File.Caption)
            : block.File.Name;

        // リンクを生成して改行を追加
        return MarkdownUtils.LineBreak(
            MarkdownUtils.Link(caption, url));
    }
}
