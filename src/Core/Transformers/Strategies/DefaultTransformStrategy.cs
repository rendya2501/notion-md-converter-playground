using Notion.Client;
using NotionMarkdownConverter.Core.Transformers.States;

namespace NotionMarkdownConverter.Core.Transformers.Strategies;

/// <summary>
/// デフォルトの変換ストラテジー
/// </summary>
public class DefaultTransformStrategy : IDefaultBlockTransformStrategy
{
    /// <summary>
    /// ブロックタイプ
    /// </summary>
    /// <value></value>
    public BlockType BlockType => BlockType.Unsupported;

    /// <summary>
    /// ブロックを変換します
    /// </summary>
    /// <param name="context">変換コンテキスト</param>
    /// <returns>変換されたマークダウン文字列</returns>
    public string Transform(NotionBlockTransformContext context)
    {
        return string.Empty;
    }
}