using NotionMarkdownConverter.Shared.Enums;
using NotionMarkdownConverter.Shared.Models;

namespace NotionMarkdownConverter.Extract;

/// <summary>
/// ページをエクスポートすべきかどうかの判定を担う。
/// </summary>
/// <remarks>
/// 副作用なし・純粋なロジックのため単体テスト可能。
/// </remarks>
public class PageExportEligibilityChecker
{
    /// <summary>
    /// ページをエクスポートするかどうかを判定します。
    /// 公開ステータスが公開待ち、かつ公開日時が現在日時以前の場合にエクスポート対象と判断します。
    /// </summary>
    /// <param name="pageProperty">判定対象のページプロパティ</param>
    /// <param name="now">判定基準時刻</param>
    /// <returns>エクスポート対象の場合は <c>true</c></returns>
    public bool ShouldExport(PageProperty pageProperty, DateTime now)
    {
        // 公開ステータスが公開待ちでない場合はスキップ
        if (pageProperty.PublicStatus != PublicStatus.Queued) return false;
        // 公開日時が未設定の場合はスキップ
        if (!pageProperty.PublishedDateTime.HasValue) return false;
        // 公開日時が未来の場合はスキップ
        if (now < pageProperty.PublishedDateTime.Value) return false;

        return true;
    }
}
