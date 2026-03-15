# CLAUDE.md

Claude Code がこのリポジトリで作業する際のガイドです。

## プロジェクト概要

NotionデータベースのページをZenn向けMarkdownファイルとしてエクスポートするGitHub Actionsツールです。  
.NET 8 / C# で実装されており、ETLパイプライン（Pipes and Filters パターン）を採用しています。  

```txt
Notion API → [Extract] → [Transform] → [Load] → Markdownファイル
```

アーキテクチャの詳細は `docs/structure.md` を参照してください。

## ディレクトリ構成

```txt
notion-to-markdown-converter-playground/
├── src/                          # アプリケーション本体
├── tests/                        # テスト
│   ├── Integration/              # 統合テスト（Notion APIを実際に叩く）
│   └── Unit/                     # ユニットテスト
├── docs/
│   └── structure.md              # アーキテクチャドキュメント
├── action.yml                    # GitHub Actions定義（Docker使用）
└── action.example.yml            # GitHub Actions定義（composite使用）
```

## 開発コマンド

```bash
# ビルド
dotnet build ./src/NotionMarkdownConverter.csproj

# テスト実行
dotnet test

# 統合テスト用シークレット設定
cd tests
dotnet user-secrets set "Notion:AuthToken" "secret_xxx"
dotnet user-secrets set "Notion:DatabaseId" "xxx"
```

## アーキテクチャの注意点

### ETLの流れ

- `Extract`: `NotionPageExtractor` が `INotionPageReader` を使ってページとブロックを取得
- `Transform`: `NotionPageTransformer` がMarkdownに変換。ブロック種別ごとに `IBlockTransformStrategy` の実装が対応
- `Load`: `NotionPageLoader` が `IFileSystem` でファイル書き出し、`INotionPageWriter` でNotion更新

### インターフェース設計の方針

- `INotionPageReader` / `INotionPageWriter` に分離（CQS的な分割）
- 実装は `NotionClientWrapper` が両方を実装し、DIで同一インスタンスを両インターフェースに登録
- `IFileSystem` でファイルシステム操作を抽象化（テスト時のFake差し替え用）

### Strategyパターン

- 各Notionブロック種別に対して `IBlockTransformStrategy` の実装が1つ存在
- `BlockTransformDispatcher` がブロックタイプでディスパッチ
- `DependencyInjection.cs` で全Strategyを `IBlockTransformStrategy` として登録

### Zenn固有の出力

- 数式ブロック: `$$` で囲む（Zennがレンダリング）
- Embed / LinkPreview / Video: URLをそのまま出力（ZennがリンクカードまたはYouTube埋め込みとして処理）
- 画像・PDFなどNotionホストのファイル: `DownloadMarker` 付きURLを生成し、`MarkdownLinkReplacer` でローカルファイル名に置換後、`IFileDownloader` でダウンロード

### テストの方針

- 各クラスはインターフェース経由でFakeを差し込んでテスト
- `NotionPageTransformerTests` のみ `AddApplicationServices` を使用（DIコンテナ経由でConverterを取得）
- ファイル書き込みを検証するテストのみ `new FileSystem()` を使用し、それ以外は `FakeFileSystem`
