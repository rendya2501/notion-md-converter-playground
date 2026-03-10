using Notion.Client;

namespace NotionMarkdownConverter.Domain.Utils;

/// <summary>
/// プロパティ値を変換するユーティリティクラス
/// </summary>
public static class PropertyParser
{
    /// <summary>
    /// 日付プロパティからDateTimeへの変換を試みます。
    /// <see cref="DatePropertyValue"/>、<see cref="CreatedTimePropertyValue"/>、
    /// <see cref="LastEditedTimePropertyValue"/>、およびテキスト形式の日付文字列に対応します。
    /// </summary>
    /// <param name="value">変換対象のプロパティ値</param>
    /// <param name="dateTime">変換に成功した場合、取得した日時。失敗した場合は <see cref="DateTime.MinValue"/>。</param>
    /// <returns>変換に成功した場合は <c>true</c>、失敗した場合は <c>false</c></returns>
    public static bool TryParseAsDateTime(PropertyValue value, out DateTime dateTime)
    {
        dateTime = default;

        switch (value)
        {
            case DatePropertyValue dateProperty:
                if (dateProperty.Date?.Start == null)
                {
                    return false;
                }
                dateTime = dateProperty.Date.Start.Value.DateTime;
                return true;

            case CreatedTimePropertyValue createdTimeProperty:
                return DateTime.TryParse(createdTimeProperty.CreatedTime, out dateTime);

            case LastEditedTimePropertyValue lastEditedTimeProperty:
                return DateTime.TryParse(lastEditedTimeProperty.LastEditedTime, out dateTime);

            default:
                if (TryParseAsPlainText(value, out var text) &&
                    DateTime.TryParse(text, out dateTime))
                {
                    return true;
                }
                return false;
        }
    }

    /// <summary>
    /// テキスト系プロパティからプレーンテキスト文字列への変換を試みます。
    /// <see cref="RichTextPropertyValue"/>、<see cref="TitlePropertyValue"/>、
    /// <see cref="SelectPropertyValue"/> に対応します。
    /// </summary>
    /// <param name="value">変換対象のプロパティ値</param>
    /// <param name="text">変換に成功した場合、取得したプレーンテキスト。失敗した場合は空文字列。</param>
    /// <returns>変換に成功した場合は <c>true</c>、失敗した場合は <c>false</c></returns>
    public static bool TryParseAsPlainText(PropertyValue value, out string text)
    {
        text = string.Empty;

        switch (value)
        {
            case RichTextPropertyValue richTextProperty:
                text = string.Join(string.Empty, richTextProperty.RichText.Select(rt => rt.PlainText));
                return true;

            case TitlePropertyValue titleProperty:
                text = string.Join(string.Empty, titleProperty.Title.Select(t => t.PlainText));
                return true;

            case SelectPropertyValue selectProperty:
                text = selectProperty.Select?.Name ?? string.Empty;
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// マルチセレクトプロパティから文字列リストへの変換を試みます。
    /// </summary>
    /// <param name="value">変換対象のプロパティ値</param>
    /// <param name="items">変換に成功した場合、選択された項目名のリスト。失敗した場合は空リスト。</param>
    /// <returns>変換に成功した場合は <c>true</c>、失敗した場合は <c>false</c></returns>
    public static bool TryParseAsStringList(PropertyValue value, out List<string> items)
    {
        items = [];

        if (value is MultiSelectPropertyValue multiSelectProperty)
        {
            items.AddRange(multiSelectProperty.MultiSelect.Select(s => s.Name));
            return true;
        }

        return false;
    }

    /// <summary>
    /// チェックボックスプロパティからboolへの変換を試みます。
    /// </summary>
    /// <param name="value">変換対象のプロパティ値</param>
    /// <param name="result">変換に成功した場合、チェックボックスの値。失敗した場合は <c>false</c>。</param>
    /// <returns>変換に成功した場合は <c>true</c>、失敗した場合は <c>false</c></returns>
    public static bool TryParseAsBoolean(PropertyValue value, out bool result)
    {
        result = false;

        if (value is CheckboxPropertyValue checkboxProperty)
        {
            result = checkboxProperty.Checkbox;
            return true;
        }

        return false;
    }

    /// <summary>
    /// セレクトプロパティから指定した列挙型への変換を試みます。
    /// 完全一致のほか、スペースや記号を除去した正規化後の文字列でも照合します。
    /// </summary>
    /// <typeparam name="T">変換先の列挙型</typeparam>
    /// <param name="value">変換対象のプロパティ値</param>
    /// <param name="result">変換に成功した場合、対応する列挙値。失敗した場合は <typeparamref name="T"/> のデフォルト値。</param>
    /// <returns>変換に成功した場合は <c>true</c>、失敗した場合は <c>false</c></returns>
    public static bool TryParseAsEnum<T>(PropertyValue value, out T result) where T : struct, Enum
    {
        result = default;
        // セレクトプロパティでない場合、または名前がnullの場合はfalse
        if (value is not SelectPropertyValue selectPropertyValue ||
            selectPropertyValue.Select?.Name == null)
        {
            return false;
        }

        // 完全一致を試みる
        if (Enum.TryParse<T>(selectPropertyValue.Select.Name, true, out var parsed))
        {
            result = parsed;
            return true;
        }

        // スペースや特殊文字を除去して試みる
        var normalizedName = new string(
            selectPropertyValue.Select.Name.Where(char.IsLetterOrDigit).ToArray());
        if (Enum.TryParse(normalizedName, true, out parsed))
        {
            result = parsed;
            return true;
        }

        return false;
    }
}
