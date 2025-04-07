using Notion.Client;

public interface IPropertyParser2
{
    bool TryParsePropertyValueAsDateTime(PropertyValue value, out DateTime dateTime);
    bool TryParsePropertyValueAsPlainText(PropertyValue value, out string text);
    bool TryParsePropertyValueAsStringSet(PropertyValue value, out List<string> set);
    bool TryParsePropertyValueAsBoolean(PropertyValue value, out bool boolean);
}

public class PropertyParser2 : IPropertyParser2
{
    /// <summary>
    /// プロパティ値をDateTimeとして解析します。
    /// </summary>
    /// <param name="value">プロパティ値</param>
    /// <param name="dateTime">解析されたDateTime</param>
    /// <returns>解析が成功したかどうか</returns>
    bool TryParsePropertyValueAsDateTime(PropertyValue value, out DateTime dateTime)
    {
        dateTime = default;
        switch (value)
        {
            case DatePropertyValue dateProperty:
                if (dateProperty.Date == null) return false;
                if (!dateProperty.Date.Start.HasValue) return false;

                dateTime = dateProperty.Date.Start.Value;

                break;
            case CreatedTimePropertyValue createdTimeProperty:
                if (!DateTime.TryParse(createdTimeProperty.CreatedTime, out dateTime))
                {
                    return false;
                }

                break;
            case LastEditedTimePropertyValue lastEditedTimeProperty:
                if (!DateTime.TryParse(lastEditedTimeProperty.LastEditedTime, out dateTime))
                {
                    return false;
                }

                break;

            default:
                if (!TryParsePropertyValueAsPlainText(value, out var plainText))
                {
                    return false;
                }

                if (!DateTime.TryParse(plainText, out dateTime))
                {
                    return false;
                }

                break;
        }

        return true;
    }


    /// <summary>
    /// プロパティ値をプレーンテキストとして解析します。
    /// </summary>
    /// <param name="value">プロパティ値</param>
    /// <param name="text">解析されたテキスト</param>
    /// <returns>解析が成功したかどうか</returns>
    bool TryParsePropertyValueAsPlainText(PropertyValue value, out string text)
    {
        text = string.Empty;
        switch (value)
        {
            case RichTextPropertyValue richTextProperty:
                foreach (var richText in richTextProperty.RichText)
                {
                    text += richText.PlainText;
                }
                break;
            case TitlePropertyValue titleProperty:
                foreach (var richText in titleProperty.Title)
                {
                    text += richText.PlainText;
                }
                break;
            case SelectPropertyValue selectPropertyValue:
                text = selectPropertyValue.Select?.Name ?? "";
                break;
            default:
                return false;
        }

        return true;
    }


    /// <summary>
    /// プロパティ値を文字列セットとして解析します。
    /// </summary>
    /// <param name="value">プロパティ値</param>
    /// <param name="set">解析された文字列セット</param>
    /// <returns>解析が成功したかどうか</returns>
    bool TryParsePropertyValueAsStringSet(PropertyValue value, out List<string> set)
    {
        //set = new List<string>();
        set = [];
        switch (value)
        {
            case MultiSelectPropertyValue multiSelectProperty:
                foreach (var selectValue in multiSelectProperty.MultiSelect)
                {
                    set.Add(selectValue.Name);
                }
                break;
            default:
                return false;
        }

        return true;
    }


    /// <summary>
    /// プロパティ値をブール値として解析します。
    /// </summary>
    /// <param name="value">プロパティ値</param>
    /// <param name="boolean">解析されたブール値</param>
    /// <returns>解析が成功したかどうか</returns>
    bool TryParsePropertyValueAsBoolean(PropertyValue value, out bool boolean)
    {
        boolean = false;
        switch (value)
        {
            case CheckboxPropertyValue checkboxProperty:
                boolean = checkboxProperty.Checkbox;
                break;
            default:
                return false;
        }

        return true;
    }

    bool IPropertyParser2.TryParsePropertyValueAsDateTime(PropertyValue value, out DateTime dateTime)
    {
        return TryParsePropertyValueAsDateTime(value, out dateTime);
    }

    bool IPropertyParser2.TryParsePropertyValueAsPlainText(PropertyValue value, out string text)
    {
        return TryParsePropertyValueAsPlainText(value, out text);
    }

    bool IPropertyParser2.TryParsePropertyValueAsStringSet(PropertyValue value, out List<string> set)
    {
        return TryParsePropertyValueAsStringSet(value, out set);
    }

    bool IPropertyParser2.TryParsePropertyValueAsBoolean(PropertyValue value, out bool boolean)
    {
        return TryParsePropertyValueAsBoolean(value, out boolean);
    }
}
