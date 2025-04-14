using Notion.Client;
using NotionMarkdownConverter.Core.Transformer.State;

namespace NotionMarkdownConverter.Core.Transformer.Strategies;

/// <summary>
/// ブラウザパス変換ストラテジー
/// </summary>
public class BreadcrumbTransformStrategy : IBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Breadcrumb;

    /// <summary>
    /// ブロックを変換します。
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformState context)
    {
        return string.Empty;
    }
}
