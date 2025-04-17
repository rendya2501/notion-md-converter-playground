using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// リンクプレビュー変換ストラテジー
/// </summary>
public class LinkPreviewTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.LinkPreview;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public Task<string> TransformAsync(NotionBlockTransformState context)
    {
        var linkPreview = BlockConverter.GetOriginalBlock<LinkPreviewBlock>(context.CurrentBlock);
        var url = linkPreview.LinkPreview.Url;
        
        return Task.FromResult(
            MarkdownUtils.Link(url,url));
    }
}