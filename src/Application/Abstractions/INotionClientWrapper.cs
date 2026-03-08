using Notion.Client;
using NotionMarkdownConverter.Domain.Models;

namespace NotionMarkdownConverter.Application.Abstractions;

/// <summary>
/// Notion APIクライアントのラッパーインターフェース。
/// Notion APIの操作を抽象化し、テスト時のモック差し替えを可能にします。
/// </summary>
public interface INotionClientWrapper
{
    /// <summary>
    /// 公開待ちステータスのページ一覧を取得します。
    /// </summary>
    /// <param name="databaseId">取得対象のNotionデータベースID</param>
    /// <returns>公開待ちページのリスト</returns>
    Task<List<Page>> GetPagesForPublishingAsync(string databaseId);

    /// <summary>
    /// エクスポート完了後にページのプロパティを更新します。
    /// クロール日時を記録し、公開ステータスを公開済みに変更します。
    /// </summary>
    /// <param name="pageId">更新対象のページID</param>
    /// <param name="now">クロール日時として記録する日時（UTC）</param>
    Task UpdatePagePropertiesAsync(string pageId, DateTime now);

    /// <summary>
    /// 指定ブロックの子ブロックをすべて再帰的に取得し、ツリー構造として返します。
    /// </summary>
    /// <param name="blockId">取得起点となるブロックID（ページIDも可）</param>
    /// <returns>子ブロックを再帰的に展開したブロックのリスト</returns>
    Task<List<NotionBlock>> FetchBlockTreeAsync(string blockId);
}
