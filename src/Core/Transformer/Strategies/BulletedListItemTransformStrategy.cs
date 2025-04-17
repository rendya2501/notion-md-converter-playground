using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;
using NotionMarkdownConverter.Core.Utils;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// 箇条書きリスト変換ストラテジー
/// </summary>
public class BulletedListItemTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.BulletedListItem;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public async Task<string> TransformAsync(NotionBlockTransformState context)
    {
        var block = BlockConverter.GetOriginalBlock<BulletedListItemBlock>(context.CurrentBlock);
        // テキストを改行で分割
        var text = MarkdownUtils.LineBreak(
            MarkdownUtils.RichTextsToMarkdown(block.BulletedListItem.RichText));

        // テキストに改行が含まれている場合、2行目以降にインデントを適用
        var lines = text.Split('\n');
        var formattedText = lines.Length > 1
            ? $"{lines[0]}\n{string.Join("\n", lines.Skip(1).Select(line => MarkdownUtils.Indent(line)))}"
            : text;

        // 子ブロックが存在する場合、子ブロックを変換
        var children = context.CurrentBlock.HasChildren
            ? await context.GenerateContentAsync(context.CurrentBlock.Children)
            : string.Empty;

        // 子ブロックが存在しない場合、箇条書きリストを生成
        return string.IsNullOrEmpty(children)
            ? MarkdownUtils.BulletList(formattedText)
            : $"{MarkdownUtils.BulletList(formattedText)}\n{MarkdownUtils.Indent(children)}";
    }
} 