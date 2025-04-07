using Notion.Client;

namespace hoge.Utils;

/// <summary>
/// プロパティ値を変換するユーティリティクラス
/// </summary>
public static class PropertyParser
{
    /// <summary>
    /// 日付プロパティからDateTimeに変換を試みます
    /// </summary>
    /// <param name="value"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
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
                dateTime = dateProperty.Date.Start.Value;
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
    /// テキストプロパティから文字列に変換を試みます
    /// </summary>
    /// <param name="value"></param>
    /// <param name="text"></param>
    /// <returns></returns>
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
    /// マルチセレクトプロパティから文字列リストに変換を試みます
    /// </summary>
    /// <param name="value"></param>
    /// <param name="items"></param>
    /// <returns></returns>
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
    /// チェックボックスプロパティからBooleanに変換を試みます
    /// </summary>
    /// <param name="value"></param>
    /// <param name="result"></param>
    /// <returns></returns>
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
    /// セレクトプロパティから指定された型のEnumに変換を試みます
    /// </summary>
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
            [.. selectPropertyValue.Select.Name.Where(c => char.IsLetterOrDigit(c))]);        
        if (Enum.TryParse<T>(normalizedName, true, out parsed))
        {
            result = parsed;
            return true;
        }

        return false;
    }
}
