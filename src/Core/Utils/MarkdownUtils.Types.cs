using Notion.Client;

namespace NotionMarkdownConverter.Core.Utils;

/// <summary>
/// 箇条書きスタイル
/// </summary>
public enum BulletStyle
{
    Hyphen = '-',
    Asterisk = '*',
    Plus = '+'
}

/// <summary>
/// 水平線スタイル
/// </summary>
public enum HolizontalRuleStyle
{
    Hyphen,
    Asterisk,
    Underscor
}

/// <summary>
/// 改行スタイル
/// </summary>
public enum LineBreakStyle
{
    Newline, // \n
    BR       // <BR>
}

/// <summary>
/// カラーマップの型
/// </summary>
public class ColorMap : Dictionary<Color, string> { } 

///// <summary>
///// テーブルセル
///// </summary>
//public class TableCell
//{
//    public required string Content { get; set; }
//}

///// <summary>
///// テーブルヘッダー
///// </summary>
//public class TableHeader
//{
//    public required string Content { get; set; }
//    public Alignment Alignment { get; set; }
//}

//public enum Alignment
//{
//    Left,
//    Center,
//    Right
//}
