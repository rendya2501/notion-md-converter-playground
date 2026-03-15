using Notion.Client;
using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Infrastructure.Notion;

/// <summary>
/// Notionからページ・ブロックを読み取る操作のインターフェース。
/// Extractステージで使用します。
/// </summary>
public interface INotionPageReader
{
    /// <summary>
    /// 公開待ちステータスのページ一覧を取得します。
    /// </summary>
    /// <param name="databaseId">取得対象のNotionデータベースID</param>
    /// <returns>公開待ちページのリスト</returns>
    Task<List<Page>> GetPagesForPublishingAsync(string databaseId);

    /// <summary>
    /// 指定ブロックの子ブロックをすべて再帰的に取得し、ツリー構造として返します。
    /// </summary>
    /// <param name="blockId">取得起点となるブロックID（ページIDも可）</param>
    /// <returns>子ブロックを再帰的に展開したブロックのリスト</returns>
    Task<List<NotionBlock>> FetchBlockTreeAsync(string blockId);
}
