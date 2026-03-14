# NotionMarkdownConverter アーキテクチャ

## 概要

NotionMarkdownConverter は、NotionページをMarkdownファイルとしてエクスポートするGitHub Actionsツールです。
アーキテクチャは **ETLパイプライン（Pipes and Filters パターン）** を採用しています。
```
Notion API → [Extract] → [Transform] → [Load] → Markdownファイル
```

---

## ディレクトリ構造
```
src/
├── Configuration/               # アプリケーション設定
│   └── NotionExportOptions.cs   # コマンドライン引数から生成される設定値
│
├── Extract/                     # Extractステージ: Notionから公開対象ページを取得
│   ├── IPagePropertyMapper.cs
│   ├── PagePropertyMapper.cs    # Notion APIのPageオブジェクト → PagePropertyへのマッピング
│   ├── PageExportEligibilityChecker.cs  # 公開対象の判定ロジック
│   └── NotionPageExtractor.cs   # Extractステージのエントリーポイント
│
├── Transform/                   # Transformステージ: ExtractedPage → Markdown変換
│   ├── Converters/              # FrontmatterConverter, ContentConverter
│   ├── Enums/                   # 変換設定のEnum（BulletStyle, HorizontalRuleStyle, LineBreakStyle）
│   ├── Strategies/              # ブロック種別ごとの変換ストラテジー群
│   │   ├── Abstractions/        # IBlockTransformStrategy, IDefaultBlockTransformStrategy
│   │   ├── Context/             # NotionBlockTransformContext
│   │   └── (各Strategyファイル: BookmarkTransformStrategy等 20種)
│   ├── Types/                   # ColorMap等の型定義
│   ├── Utils/                   # Markdown生成ユーティリティ群
│   │               # (MarkdownBlockUtils, MarkdownInlineUtils, MarkdownListUtils, MarkdownRichTextUtils)
│   ├── BlockTransformDispatcher.cs  # ブロック種別に応じてStrategyを呼び出す
│   ├── IMarkdownLinkProcessor.cs
│   ├── MarkdownLinkProcessor.cs # MarkdownリンクのURL変換・ファイルダウンロード
│   └── NotionPageTransformer.cs # Transformステージのエントリーポイント
│
├── Load/                        # Loadステージ: Markdownファイルの書き出し
│   └── NotionPageLoader.cs      # index.md書き出し・Notionプロパティ更新・GitHub環境変数設定
│
├── Pipeline/                    # ETLパイプラインの定義
│   ├── Models/                  # ステージ間を流れるデータモデル
│   │   ├── ExtractedPage.cs     # Extractステージの出力（PageProperty + Blocks）
│   │   └── TransformedPage.cs   # Transformステージの出力（PageProperty + Markdown + OutputDirectory）
│   └── NotionExportPipeline.cs  # Extract → Transform → Load を順に呼び出すオーケストレーター
│
├── Shared/                      # 複数ステージから参照される共有モデル・ユーティリティ
│   ├── Constants/               # LinkConstants, NotionPagePropertyNames
│   ├── Enums/                   # PublicStatus
│   ├── Models/                  # NotionBlock, PageProperty, UrlFilePair
│   └── Utils/                   # BlockAccessor, PropertyParser
│
├── Infrastructure/              # 外部サービスとの接続実装
│   ├── FileSystem/              # IOutputDirectoryProvider / OutputDirectoryProvider
│   ├── GitHub/                  # IGitHubEnvironmentUpdater / GitHubEnvironmentUpdater
│   ├── Http/                    # IFileDownloader / HttpFileDownloader / HttpFileDownloaderOptions
│   └── Notion/                  # INotionClientWrapper / NotionClientWrapper
│
├── DependencyInjection.cs       # 全サービスのDI登録（Configuration → Infrastructure → Extract → Transform → Load → Pipeline）
└── Program.cs                   # エントリーポイント
```

---

## データフロー
```
Program.cs
  └─ NotionExportPipeline.RunAsync()
       ├─ NotionPageExtractor.ExtractAsync(databaseId)
       │    ├─ INotionClientWrapper.GetPagesForPublishingAsync()
       │    ├─ IPagePropertyMapper.Map()
       │    ├─ PageExportEligibilityChecker.ShouldExport()
       │    └─ INotionClientWrapper.FetchBlockTreeAsync()
       │         → List<ExtractedPage>
       │
       ├─ NotionPageTransformer.TransformAsync(extractedPage) × N
       │    ├─ IOutputDirectoryProvider.BuildAndCreate()
       │    ├─ FrontmatterConverter.Convert()
       │    ├─ ContentConverter.Convert()
       │    │    └─ BlockTransformDispatcher → IBlockTransformStrategy
       │    └─ IMarkdownLinkProcessor.ProcessLinksAsync()
       │         → TransformedPage
       │
       └─ NotionPageLoader.LoadAsync(transformedPages)
            ├─ File.WriteAllTextAsync()           # index.md書き出し
            ├─ INotionClientWrapper.UpdatePagePropertiesAsync()
            └─ IGitHubEnvironmentUpdater.UpdateEnvironment()
```

---

## テスト構成
```
tests/
├── Integration/
│   ├── IntegrationTestBase.cs
│   └── NotionExportIntegrationTests.cs  # パイプライン全体の結合テスト
└── Unit/
    ├── NotionPageExtractorTests.cs
    ├── NotionPageTransformerTests.cs    # ゴールデンファイルテスト含む
    ├── NotionPageLoaderTests.cs
    ├── BlockTransformDispatcherTests.cs
    ├── ContentConverterTests.cs
    ├── FrontmatterConverterTests.cs
    ├── TransformStrategyTests.cs        # 全Strategyの網羅テスト
    ├── TableTransformStrategyTests.cs
    ├── VideoTransformStrategyTests.cs
    ├── MarkdownLinkProcessorTests.cs
    ├── MarkdownBlockUtilsTests.cs
    ├── MarkdownInlineUtilsTests.cs
    ├── MarkdownListUtilsTests.cs
    ├── BlockAccessorTests.cs
    ├── PropertyParserTests.cs
    ├── PagePropertyMapperTests.cs
    ├── PageExportEligibilityCheckerTests.cs
    ├── OutputDirectoryProviderTests.cs
    ├── GitHubEnvironmentUpdaterTests.cs
    └── HttpFileDownloaderOptionsTests.cs
```
