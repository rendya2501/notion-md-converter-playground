# API仕様書

## 概要

このドキュメントでは、NotionからMarkdownへの変換ツールのAPI仕様について説明します。

## 認証

### Notion API認証

```csharp
public class NotionConfiguration
{
    public string ApiKey { get; set; }
    public string DatabaseId { get; set; }
}
```

## 主要なAPI

### 1. Notionデータの取得

```csharp
public interface INotionService
{
    Task<NotionPage> GetPageAsync(string pageId);
    Task<IEnumerable<NotionPage>> GetPagesFromDatabaseAsync(string databaseId);
}
```

### 2. Markdown変換

```csharp
public interface IMarkdownConverter
{
    string ConvertToMarkdown(NotionPage page);
    Task<string> ConvertToMarkdownAsync(NotionPage page);
}
```

### 3. ファイル出力

```csharp
public interface IFileService
{
    Task SaveMarkdownAsync(string content, string filePath);
    Task SaveMarkdownsAsync(IDictionary<string, string> contents, string directoryPath);
}
```

## データモデル

### NotionPage

```csharp
public class NotionPage
{
    public string Id { get; set; }
    public string Title { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime LastEditedTime { get; set; }
    public IList<NotionBlock> Blocks { get; set; }
}
```

### NotionBlock

```csharp
public class NotionBlock
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }
    public IList<NotionBlock> Children { get; set; }
}
```

## エラーハンドリング

### 例外

```csharp
public class NotionApiException : Exception
{
    public string ErrorCode { get; }
    public string ErrorMessage { get; }
}

public class MarkdownConversionException : Exception
{
    public string BlockType { get; }
    public string Content { get; }
}
```

## 使用例

### 基本的な使用例

```csharp
var config = new NotionConfiguration
{
    ApiKey = "your-api-key",
    DatabaseId = "your-database-id"
};

var notionService = new NotionService(config);
var converter = new MarkdownConverter();
var fileService = new FileService();

// ページの取得と変換
var page = await notionService.GetPageAsync("page-id");
var markdown = converter.ConvertToMarkdown(page);
await fileService.SaveMarkdownAsync(markdown, "output.md");
```

### データベースからの一括変換

```csharp
var pages = await notionService.GetPagesFromDatabaseAsync(config.DatabaseId);
var markdowns = new Dictionary<string, string>();

foreach (var page in pages)
{
    var markdown = converter.ConvertToMarkdown(page);
    markdowns.Add($"{page.Title}.md", markdown);
}

await fileService.SaveMarkdownsAsync(markdowns, "output-directory");
```

## 設定オプション

### 変換オプション

```csharp
public class ConversionOptions
{
    public bool IncludeMetadata { get; set; } = true;
    public bool PreserveFormatting { get; set; } = true;
    public string ImageDirectory { get; set; } = "images";
}
```

## 制限事項

- APIレート制限に注意
- 大きなページの処理には時間がかかる可能性
- 画像の処理には追加の設定が必要 