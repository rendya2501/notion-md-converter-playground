using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

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
    public string Transform(NotionBlockTransformState context)
    {
        // 子ブロックを変換
        var children = context.ExecuteTransformBlocks(context.CurrentBlock.Children);
        // トグルブロックを取得
        var toggleBlock = BlockConverter.GetOriginalBlock<ToggleBlock>(context.CurrentBlock);
        // タイトルを取得して改行を追加
        var title = MarkdownUtils.LineBreak(
            MarkdownUtils.RichTextsToMarkdown(toggleBlock.Toggle.RichText), LineBreakStyle.BR);

        // 詳細を生成
        return MarkdownUtils.Details(title, children);
    }
}