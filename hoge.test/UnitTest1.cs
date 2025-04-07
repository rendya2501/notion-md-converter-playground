using hoge.Services;
using hoge.Utils;
using System.ComponentModel;
using System.Reflection;

namespace hoge.test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var bulletText = MarkdownUtils.BulletList("test", BulletStyle.Plus);
        Assert.Equal("+ test", bulletText);
    }

    public void Test2()
    {
        var processors = new Dictionary<Type, Func<CustomType, string>>()
        {
            { typeof(CustomTypeA), obj => obj.Process() },
            { typeof(CustomTypeB), obj => obj.Process() },
            { typeof(CustomTypeC), obj => obj.Process() }
        };

        var types = new List<CustomType> {
            new CustomTypeA(),
            new CustomTypeB(),
            new CustomTypeC()
        };

        foreach (var type in types)
        {
            var aa = type switch
            {
                CustomTypeA customTypeA => processors[typeof(CustomTypeA)]?.Invoke(customTypeA) ?? string.Empty,
                CustomTypeB customTypeB => processors[typeof(CustomTypeB)]?.Invoke(customTypeB) ?? string.Empty,
                CustomTypeC customTypeC => processors[typeof(CustomTypeC)]?.Invoke(customTypeC) ?? string.Empty,
                _ => string.Empty
            };

            Assert.Equal("Processing A-specific logic", aa);
        }
    }

    [Fact]
    public void Test3()
    {
        // 使用例
        string symbol = BulletStyle2.Hyphen.GetDescription();
        Console.WriteLine(symbol); // 出力: "-"
    }


    public enum BulletStyle2
    {
        [Description("-")]
        Hyphen,
        [Description("*")]
        Asterisk,
        [Description("+")]
        Plus
    }

    [Fact]
    public void Test4()
    {
        // 使用例
        string symbol = PublicStatus.Queued.ToString();
        Console.WriteLine(symbol);
    }
}

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
}
// オリジナルな型を3つ定義
public abstract class CustomType
{
    public abstract string Process();
}

public class CustomTypeA : CustomType
{
    public override string Process() => "Processing A-specific logic";
}

public class CustomTypeB : CustomType
{
    public override string Process() => "Processing B-specific logic";
}

public class CustomTypeC : CustomType
{
    public override string Process() => "Processing C-specific logic";
}
