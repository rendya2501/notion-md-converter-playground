using Notion.Client;
using NotionMarkdownConverter.Shared.Models;
using NotionMarkdownConverter.Shared.Utils;

namespace NotionMarkdownConverter.Tests.Unit;

/// <summary>
/// BlockAccessorのユニットテスト
/// </summary>
public class BlockAccessorTests
{
    [Fact]
    public void GetOriginalBlock_CorrectType_ReturnsTypedBlock()
    {
        var paragraphBlock = new ParagraphBlock
        {
            Paragraph = new ParagraphBlock.Info { RichText = [] }
        };
        var notionBlock = new NotionBlock(paragraphBlock);

        var result = BlockAccessor.GetOriginalBlock<ParagraphBlock>(notionBlock);

        Assert.NotNull(result);
        Assert.IsType<ParagraphBlock>(result);
    }

    [Fact]
    public void GetOriginalBlock_WrongType_ThrowsInvalidCastException()
    {
        var paragraphBlock = new ParagraphBlock
        {
            Paragraph = new ParagraphBlock.Info { RichText = [] }
        };
        var notionBlock = new NotionBlock(paragraphBlock);

        Assert.Throws<InvalidCastException>(() =>
            BlockAccessor.GetOriginalBlock<HeadingOneBlock>(notionBlock));
    }

    [Fact]
    public void GetOriginalBlock_ExceptionMessage_ContainsTypeNames()
    {
        var paragraphBlock = new ParagraphBlock
        {
            Paragraph = new ParagraphBlock.Info { RichText = [] }
        };
        var notionBlock = new NotionBlock(paragraphBlock);

        var ex = Assert.Throws<InvalidCastException>(() =>
            BlockAccessor.GetOriginalBlock<HeadingOneBlock>(notionBlock));

        Assert.Contains("ParagraphBlock", ex.Message);
        Assert.Contains("HeadingOneBlock", ex.Message);
    }

    [Fact]
    public void GetOriginalBlock_CodeBlock_ReturnsCodeBlock()
    {
        var codeBlock = new CodeBlock
        {
            Code = new CodeBlock.Info { RichText = [], Language = "csharp" }
        };
        var notionBlock = new NotionBlock(codeBlock);

        var result = BlockAccessor.GetOriginalBlock<CodeBlock>(notionBlock);

        Assert.Equal("csharp", result.Code.Language);
    }
}
