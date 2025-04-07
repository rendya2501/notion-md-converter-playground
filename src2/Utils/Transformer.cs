using hoge.Models;
using Notion.Client;

namespace hoge.Utils;

public static class Transformer
{
    /// <summary>
    /// ブックマークブロックをMarkdown形式に変換する
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownBookmarkTransformer()
    {
        return context =>
        {
            // 現在のブロックをブックマークブロックに変換
            var originalBlock = context.CurrentBlock.GetOriginalBlock<Block>();
            // ブックマークブロックが存在しない場合は空文字を返す
            if (originalBlock is not BookmarkBlock bookmarkBlock || string.IsNullOrEmpty(bookmarkBlock.Bookmark.Url))
            {
                return string.Empty;
            }

            // ブックマークのキャプションをMarkdown形式に変換
            string caption = MarkdownUtils.RichTextsToMarkdown(bookmarkBlock.Bookmark.Caption);
            // ブックマークのキャプションが空の場合はURLを表示する
            string text = !string.IsNullOrEmpty(caption)
                ? caption
                : bookmarkBlock.Bookmark.Url;
            // リンクを生成し、最後に改行用スペースを追加
            return MarkdownUtils.WithLineBreak(
                MarkdownUtils.Link(text, bookmarkBlock.Bookmark.Url));
        };
    }


    /// <summary>
    /// ブラウザブロックをMarkdown形式に変換する
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownBreadcrumbTransformer()
    {
        return Context => "";
    }


    /// <summary>
    /// バレットリスト変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownBulletedListItemTransformer()
    {
        return context =>
        {
            var block = context.CurrentBlock.GetOriginalBlock<BulletedListItemBlock>();
            var text = MarkdownUtils.LineBreak(
                MarkdownUtils.RichTextsToMarkdown(block.BulletedListItem.RichText));

            // テキストに改行が含まれている場合、2行目以降にインデントを適用
            var lines = text.Split('\n');
            var formattedText = lines.Length > 1
                ? $"{lines[0]}\n{string.Join("\n", lines.Skip(1).Select(line => MarkdownUtils.Indent(line)))}"
                : text;

            var children = context.CurrentBlock.HasChildren
                ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
                : string.Empty;

            return string.IsNullOrEmpty(children)
                ? MarkdownUtils.BulletList(formattedText)
                : $"{MarkdownUtils.BulletList(formattedText)}\n{MarkdownUtils.Indent(children)}";
        };
    }


    /// <summary>
    /// コードブロック変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownCodeTransformer()
    {
        return context =>
        {
            var block = context.CurrentBlock.GetOriginalBlock<CodeBlock>();
            var text = MarkdownUtils.RichTextsToMarkdown(block.Code.RichText);
            var lang = context.CurrentBlock.GetOriginalBlock<CodeBlock>().Code.Language;
            return MarkdownUtils.CodeBlock(text, lang);
        };
    }


    /// <summary>
    /// カラムリスト変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownColumnListTransformer()
    {
        return context =>
        {
            var columns = context.CurrentBlock.Children;
            var columnsText = columns.Select(column => context.ExecuteTransformBlocks(column.Children));

            return string.Join("\n", columnsText);
        };
    }


    /// <summary>
    /// コールアウトブロックをMarkdown形式に変換する
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownCalloutTransformer()
    {
        //    foreach (var richText in calloutBlock.Callout.RichText)
        //    {
        //        var lines = richText.PlainText.Split([Environment.NewLine, "\n"], StringSplitOptions.None);
        //        foreach (var line in lines)
        //        {
        //            sb.Append($"{indent}> ");
        //            var tempRichText = new RichTextBase
        //            {
        //                PlainText = line,
        //                Annotations = richText.Annotations,
        //                Href = richText.Href
        //            };
        //            AppendRichText(tempRichText);
        //            if (!string.IsNullOrWhiteSpace(tempRichText.PlainText)) sb.Append("  ");
        //            sb.AppendLine();
        //        }
        //    }
        return context =>
        {
            var children = context.CurrentBlock.HasChildren
                ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
                : string.Empty;

            var text = MarkdownUtils.LineBreak(
                MarkdownUtils.RichTextsToMarkdown(
                    context.CurrentBlock.GetOriginalBlock<CalloutBlock>().Callout.RichText));
            
            var result = string.IsNullOrEmpty(children) ? text : $"{text}\n{children}";

            return MarkdownUtils.Blockquote(result);
        };
    }


    /// <summary>
    /// 水平線変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownDividerTransformer()
    {
        return context => MarkdownUtils.HorizontalRule();
    }


    /// <summary>
    /// 埋め込み変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownEmbedTransformer()
    {
        return context =>
        {
            // var captionMetadata = FromRichText(context.CurrentBlock.GetOriginalBlock<EmbedBlock>().Embed.Caption);
            // if (enableEmbed && supportedEmbedProviders)
            // {
            //     var result = ProviderUtils.embedByUrl(block.embed.url, captionMetadata, {
            //         supportedEmbedProviders,
            //     });
            //     if (result)
            //     {
            //         return result;
            //     }
            // }

            var embedBlock = context.CurrentBlock.GetOriginalBlock<EmbedBlock>();
            var caption = MarkdownUtils.RichTextsToMarkdown(embedBlock.Embed.Caption);
            var url = embedBlock.Embed.Url;
            return MarkdownUtils.WithLineBreak(
                MarkdownUtils.Link(caption ?? url, url));
        };
    }


    /// <summary>
    /// 数式変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownEquationTransformer()
    {
        return context =>
        {
            var block = context.CurrentBlock.GetOriginalBlock<EquationBlock>();
            var text = block.Equation.Expression;
            var result = block.Type == BlockType.Code
                ? MarkdownUtils.CodeBlock(text, "txt")
                : MarkdownUtils.BlockEquation(text);
            return result;
        };
    }


    /// <summary>
    /// ファイル変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownFileTransformer()
    {
        return context =>
        {
            // var captionMetadata = CaptionMetadata.fromRichText(context.CurrentBlock.GetOriginalBlock<FileBlock>().File.Caption);
            var fileBlock = context.CurrentBlock.GetOriginalBlock<FileBlock>().File;
            // var { url } = fileAdapter(block.file);
            // var caption = fileBlock.Caption.Any()
            //     ? RichTextsToMarkdown(fileBlock.Caption)
            //     : fileBlock.Name;
            // return Link(caption, fileBlock.);
            return MarkdownUtils.RichTextsToMarkdown(fileBlock.Caption);
        };
    }


    /// <summary>
    /// 見出し変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownHeadingTransformer()
    {
        return context =>
        {
            var block = context.CurrentBlock.GetOriginalBlock<Block>();
            var (text, level) = block switch
            {
                HeadingOneBlock headingOneBlock => (MarkdownUtils.RichTextsToMarkdown(headingOneBlock.Heading_1.RichText), 1),
                HeadingTwoBlock headingTwoBlock => (MarkdownUtils.RichTextsToMarkdown(headingTwoBlock.Heading_2.RichText), 2),
                HeadingThreeBlock headingThreeBlock => (MarkdownUtils.RichTextsToMarkdown(headingThreeBlock.Heading_3.RichText), 3),
                _ => (string.Empty, 1),
            };

            return MarkdownUtils.Heading(text, level);
        };
    }


    /// <summary>
    /// リンクプレビュー変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownLinkPreviewTransformer()
    {

        return context => "";
    }


    /// <summary>
    /// 番号付きリスト変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownNumberedListItemTransformer()
    {
        //static string execute(NotionBlockTransformContext context)
        //{
        //    // 現在のブロックの前にあるブロックを取得
        //    var listCount = context.Blocks
        //        .Take(context.CurrentBlockIndex)
        //        .Reverse()
        //        .TakeWhile(b => b.OriginalBlock is NumberedListItemBlock)
        //        .Count() + 1;

        //    // 現在のブロックを取得
        //    var block = context.CurrentBlock.GetOriginalBlock<NumberedListItemBlock>();
        //    // 現在のブロックのテキストを取得
        //    var text = RichTextsToMarkdown(block.NumberedListItem.RichText);
        //    // 現在のブロックの子ブロックを取得
        //    var children = context.CurrentBlock.HasChildren
        //        ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
        //        : string.Empty;
        //    // 現在のブロックの子ブロックが空の場合は番号付きリストを返す
        //    return string.IsNullOrEmpty(children)
        //        ? NumberedList(text, listCount)
        //        : $"{NumberedList(text, listCount)}\n{Indent(children, 3)}";
        //}
        //return execute;

        return context =>
        {
            var listCount = context.Blocks
                .Take(context.CurrentBlockIndex)
                .Reverse()
                .TakeWhile(b => b.OriginalBlock is NumberedListItemBlock)
                .Count() + 1;

            var block = context.CurrentBlock.GetOriginalBlock<NumberedListItemBlock>();
            var text = MarkdownUtils.LineBreak(
                MarkdownUtils.RichTextsToMarkdown(
                    block.NumberedListItem.RichText));

            // テキストに改行が含まれている場合、2行目以降にインデントを適用
            var lines = text.Split('\n');
            var formattedText = lines.Length > 1
                ? $"{lines[0]}\n{string.Join("\n", lines.Skip(1).Select(line => MarkdownUtils.Indent(line, 3)))}"
                : text;

            var children = context.CurrentBlock.HasChildren
                ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
                : string.Empty;

            return string.IsNullOrEmpty(children)
                ? MarkdownUtils.NumberedList(formattedText, listCount)
                : $"{MarkdownUtils.NumberedList(formattedText, listCount)}\n{MarkdownUtils.Indent(children, 3)}";
        };
    }


    // string execute((NumberedListItemBlock Block, string Children, int Index) args)
    // {
    //     string text = RichTextsToMarkdown(args.Block.NumberedListItem.RichText);
    //     string formattedChildren = Indent(args.Children, 3);
    //     string bulletText = NumberedList(text, args.Index);

    //     if (string.IsNullOrEmpty(args.Children))
    //     {
    //         return bulletText;
    //     }

    //     return $"{bulletText}\n{formattedChildren}";
    // }

    // string createTransfomer(NotionBlockTransformContext context)
    // {
    //     // 現在のブロックの前にあるブロックを取得
    //     var beforeBlocks = context.Blocks.Take(context.CurrentBlockIndex).ToList();
    //     // NumberedListItemBlockではないブロックが出てくるまでカウント
    //     // そのカウント数がリストのインデックスとなる
    //     var listCount = 1;
    //     for (var index = beforeBlocks.Count - 1; index >= 0; index--)
    //     {
    //         if (beforeBlocks[index].OriginalBlock is not NumberedListItemBlock)
    //         {
    //             break;
    //         }
    //         listCount++;
    //     }

    //     return execute((
    //         Block: context.CurrentBlock.GetOriginalBlock<NumberedListItemBlock>(),
    //         Children: context.ExecuteTransformBlocks(context.CurrentBlock.Children),
    //         Index: listCount));
    // }

    // return createTransfomer;


    /// <summary>
    /// 段落変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownParagraphTransformer()
    {
        return context =>
        {
            var currentBlock = context.CurrentBlock;

            var children = currentBlock.HasChildren
                ? context.ExecuteTransformBlocks(currentBlock.Children)
                : string.Empty;

            var text = MarkdownUtils.LineBreak(
                MarkdownUtils.RichTextsToMarkdown(
                    currentBlock.GetOriginalBlock<ParagraphBlock>().Paragraph.RichText));

            var convertedMarkdown = string.IsNullOrEmpty(children)
                ? text
                : $"{text}{Environment.NewLine}{children}";
            return convertedMarkdown;
        };
    }


    /// <summary>
    /// 引用変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownQuoteTransformer()
    {
        //    foreach (var richText in quoteBlock.Quote.RichText)
        //    {
        //        var lines = richText.PlainText.Split([Environment.NewLine, "\n"], StringSplitOptions.None);
        //        foreach (var line in lines)
        //        {
        //            sb.Append($"{indent}> ");
        //            var tempRichText = new RichTextBase
        //            {
        //                PlainText = line,
        //                Annotations = richText.Annotations,
        //                Href = richText.Href
        //            };
        //            AppendRichText(tempRichText);
        //            if (!string.IsNullOrWhiteSpace(tempRichText.PlainText)) sb.Append("  ");
        //            sb.AppendLine();
        //        }
        //    }

        return context =>
        {
            var children = context.CurrentBlock.HasChildren
                ? context.ExecuteTransformBlocks(context.CurrentBlock.Children)
                : string.Empty;

            var text = MarkdownUtils.LineBreak(
                MarkdownUtils.RichTextsToMarkdown(
                    context.CurrentBlock.GetOriginalBlock<QuoteBlock>().Quote.RichText));

            return MarkdownUtils.Blockquote(string.IsNullOrEmpty(children) ? text : $"{text}\n{children}");
        };
    }


    /// <summary>
    /// 同期ブロック変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownSyncedBlockTransformer()
    {
        return context => "";
    }


    /// <summary>
    /// 目次変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownTableOfContentsTransformer()
    {
        return context => "";
    }


    /// <summary>
    /// テーブル変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownTableTransformer()
    {
        return context => "";
    }


    /// <summary>
    /// タスクリスト変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownTodoListItemTransformer()
    {
        return context => "";
    }


    /// <summary>
    /// トグル変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownToggleTransformer()
    {
        return context =>
        {
            var children = context.ExecuteTransformBlocks(context.CurrentBlock.Children);
            var title = MarkdownUtils.LineBreak(
                MarkdownUtils.RichTextsToMarkdown(
                    context.CurrentBlock.GetOriginalBlock<ToggleBlock>().Toggle.RichText));
            
            return MarkdownUtils.Details(title, children);
        };
    }


    /// <summary>
    /// 画像変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownImageTransformer()
    {
        return context =>
        {
            var block = context.CurrentBlock.GetOriginalBlock<ImageBlock>();
            var url = block.Image switch
            {
                ExternalFile externalFile => externalFile.External.Url,
                UploadedFile uploadedFile => uploadedFile.File.Url,
                _ => string.Empty
            };
            var title = MarkdownUtils.RichTextsToMarkdown(block.Image.Caption);
            
            return MarkdownUtils.LineBreak(
                MarkdownUtils.Image(title, url));
        };
    }


    /// <summary>
    /// PDF変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownPDFTransformer()
    {
        return context => "";
    }


    /// <summary>
    /// ビデオ変換
    /// </summary>
    /// <returns></returns>
    public static Func<NotionBlockTransformContext, string> CreateMarkdownVideoTransformer()
    {
        return context =>
        {
            var aaa = context.CurrentBlock.GetOriginalBlock<VideoBlock>();

            return "";
        };
    }
}
