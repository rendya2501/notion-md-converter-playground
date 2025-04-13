using Notion.Client;
using NotionMarkdownConverter.Infrastructure.Notion.Services;
using NotionMarkdownConverter.Models;
using NotionMarkdownConverter.Utils;

namespace NotionMarkdownConverter.Transformer;

/// <summary>
/// トグル変換ストラテジー
/// </summary>
public class ToggleTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Toggle;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        // 子ブロックを変換
        var children = context.ExecuteTransformBlocks(context.CurrentBlock.Children);
        // タイトルを取得して改行を追加
        var title = MarkdownUtils.LineBreak(
            MarkdownUtils.RichTextsToMarkdown(
                BlockConverter.GetOriginalBlock<ToggleBlock>(context.CurrentBlock).Toggle.RichText));
            
        // 詳細を生成
        return MarkdownUtils.Details(title, children);
    }
}