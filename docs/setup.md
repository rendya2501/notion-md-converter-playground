# セットアップガイド

このドキュメントでは、NotionからMarkdownへの変換ツールのセットアップ方法について説明します。

## 必要条件

- .NET 8.0 SDK
- Visual Studio 2022 または Visual Studio Code
- Git
- Notion APIキー

## インストール手順

### 1. リポジトリのクローン

```bash
git clone https://github.com/your-username/notion-to-markdown-converter-playground.git
cd notion-to-markdown-converter-playground
```

### 2. 依存関係のインストール

```bash
dotnet restore
```

### 3. Notion APIキーの設定

1. [Notion Developers](https://www.notion.so/my-integrations)にアクセス
2. 新しいインテグレーションを作成
3. APIキーを取得
4. 以下のいずれかの方法でAPIキーを設定：

#### 環境変数を使用する場合

```bash
# Windows
set NOTION_API_KEY=your-api-key

# Linux/macOS
export NOTION_API_KEY=your-api-key
```

#### 設定ファイルを使用する場合

`appsettings.json`を作成：

```json
{
  "Notion": {
    "ApiKey": "your-api-key",
    "DatabaseId": "your-database-id"
  }
}
```

## ビルド

```bash
dotnet build
```

## テストの実行

```bash
dotnet test
```

## Dockerを使用する場合

### イメージのビルド

```bash
docker build -t notion-markdown-converter .
```

### コンテナの実行

```bash
docker run -e NOTION_API_KEY=your-api-key notion-markdown-converter
```

## トラブルシューティング

### 一般的な問題

1. **APIキーが認識されない**
   - 環境変数が正しく設定されているか確認
   - 設定ファイルのパスが正しいか確認

2. **ビルドエラー**
   - .NET SDKのバージョンを確認
   - 依存関係を再インストール

3. **テストが失敗する**
   - テスト環境の設定を確認
   - 必要な環境変数が設定されているか確認

### ログの確認

```bash
dotnet run -- --log-level Debug
```

## 次のステップ

- [API仕様書](api.md)を参照して使用方法を確認
- [アーキテクチャ設計書](architecture.md)で詳細を確認
- サンプルコードを試す 