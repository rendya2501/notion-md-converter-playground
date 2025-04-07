using Notion.Client;
using System.Text;

namespace hoge.Utils;

/// <summary>
/// Markdown変換ユーティリティ
/// </summary>
public static partial class MarkdownUtils
{
    private static readonly ColorMap COLOR_MAP = new()
    {
        { Notion.Client.Color.Default, string.Empty },
        { Notion.Client.Color.Red, "#A83232" },
        { Notion.Client.Color.RedBackground, "#E8CCCC" },
        { Notion.Client.Color.Orange, "#C17F46" },
        { Notion.Client.Color.OrangeBackground, "#E8D5C2" },
        { Notion.Client.Color.Yellow, "#9B8D27" },
        { Notion.Client.Color.YellowBackground, "#E6E6C8" },
        { Notion.Client.Color.Brown, "#8B6C55" },
        { Notion.Client.Color.BrownBackground, "#E0D5CC" },
        { Notion.Client.Color.Green, "#4E7548" },
        { Notion.Client.Color.GreenBackground, "#D5E0D1" },
        { Notion.Client.Color.Blue, "#3A6B9F" },
        { Notion.Client.Color.BlueBackground, "#D0DEF0" },
        { Notion.Client.Color.Purple, "#6B5B95" },
        { Notion.Client.Color.PurpleBackground, "#D8D3E6" },
        { Notion.Client.Color.Pink, "#B5787D" },
        { Notion.Client.Color.PinkBackground, "#E8D5D8" },
        { Notion.Client.Color.Gray, "#777777" },
        { Notion.Client.Color.GrayBackground, "#D0D0D0" }
    };

    private static readonly Color[] BACKGROUND_COLOR_KEY =
    [
        Notion.Client.Color.RedBackground,
        Notion.Client.Color.OrangeBackground,
        Notion.Client.Color.YellowBackground,
        Notion.Client.Color.GreenBackground,
        Notion.Client.Color.BlueBackground,
        Notion.Client.Color.PurpleBackground,
        Notion.Client.Color.PinkBackground,
        Notion.Client.Color.GrayBackground,
        Notion.Client.Color.BrownBackground
    ];

    private static readonly Color[] TEXT_COLOR_KEY =
    [
        Notion.Client.Color.Red,
        Notion.Client.Color.Orange,
        Notion.Client.Color.Yellow,
        Notion.Client.Color.Green,
        Notion.Client.Color.Blue,
        Notion.Client.Color.Purple,
        Notion.Client.Color.Pink,
        Notion.Client.Color.Gray,
        Notion.Client.Color.Brown
    ];

    /// <summary>
    /// リッチテキストアノテーション有効化設定
    /// </summary>
    public class EnableAnnotations
    {
        public bool Bold { get; set; } = true;
        public bool Italic { get; set; } = true;
        public bool Strikethrough { get; set; } = true;
        public bool Underline { get; set; } = true;
        public bool Code { get; set; } = true;
        public bool Equation { get; set; } = true;
        public bool Link { get; set; } = true;
        public bool Color { get; set; } = false; // bool または ColorMap
    }
} 