# NotionMarkdownConverter

NotionデータベースのページをMarkdownファイルとしてエクスポートするGitHub Actionsツールです。
公開待ち（Queued）ステータスのページを取得し、Zenn向けのMarkdownに変換してリポジトリに書き出します。

## 機能

- Notionデータベースから公開待ちページを自動取得
- フロントマター（title, description, tags, date, type）を自動生成
- 画像・PDFなどの添付ファイルをローカルにダウンロード
- エクスポート完了後にNotionの公開ステータスを自動更新
- エクスポート件数をGitHub Actionsのoutputとして出力

## 対応ブロック

| ブロック               | 出力形式                                     |
| ---------------------- | -------------------------------------------- |
| Paragraph              | テキスト（Markdown改行付き）                 |
| Heading 1 / 2 / 3      | `#` / `##` / `###`                           |
| Bulleted list item     | `- `                                         |
| Numbered list item     | `1.` `2.` ...（連番自動計算）                |
| To do                  | `- [ ]` / `- [x]`                            |
| Toggle                 | `<details>` / `<summary>`                    |
| Quote                  | `>`                                          |
| Callout                | `>`                                          |
| Code                   | コードフェンス（言語指定付き）               |
| Equation               | `$$` ブロック数式                            |
| Divider                | `---`                                        |
| Table                  | Markdownテーブル                             |
| Column list            | 左から右に並べて出力                         |
| Image                  | `![]()` （Notionホスト画像はローカル保存）   |
| File / PDF             | リンク（Notionホストファイルはローカル保存） |
| Bookmark               | リンク                                       |
| Embed / Link Preview   | URL直接出力（Zennがリンクカードとして処理）  |
| Video                  | URL直接出力（ZennがYouTubeを自動埋め込み）   |
| Synced block           | 内部ブロックをそのまま出力                   |
| Breadcrumb             | 出力なし                                     |
| Table of contents      | 出力なし                                     |

## セットアップ

### 1. Notion の準備

1. [Notion Integrations](https://www.notion.so/my-integrations) でインテグレーションを作成し、APIトークンを取得します。
2. エクスポート対象のデータベースにインテグレーションを接続します。
3. データベースに以下のプロパティを追加します。

| プロパティ名  | 種類           | 用途                                              |
| ------------- | -------------- | ------------------------------------------------- |
| `PublishedAt` | 日時           | 公開日時（未来日時の場合はスキップ）              |
| `Slug`        | テキスト       | 出力ディレクトリ名（未設定時はタイトルを使用）    |
| `Title`       | タイトル       | 記事タイトル                                      |
| `Type`        | セレクト       | 記事タイプ（例: `Tech`, `Idea`）               |
| `Tags`        | マルチセレクト | タグ一覧                                          |
| `PublicStatus`| セレクト       | `Unpublished`（非公開） / `Queued`（公開待ち）/ `Published`（公開済み）|
| `Description` | テキスト       | 記事の説明文                                      |
| `_SystemCrawledAt`   | 日時           | エクスポート日時（ツールが自動更新）              |

![](https://private-user-images.githubusercontent.com/60930906/563999838-c50d592f-4440-4d70-80cc-73f5cf7d8db9.png?jwt=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3NzM2NTEzNTQsIm5iZiI6MTc3MzY1MTA1NCwicGF0aCI6Ii82MDkzMDkwNi81NjM5OTk4MzgtYzUwZDU5MmYtNDQ0MC00ZDcwLTgwY2MtNzNmNWNmN2Q4ZGI5LnBuZz9YLUFtei1BbGdvcml0aG09QVdTNC1ITUFDLVNIQTI1NiZYLUFtei1DcmVkZW50aWFsPUFLSUFWQ09EWUxTQTUzUFFLNFpBJTJGMjAyNjAzMTYlMkZ1cy1lYXN0LTElMkZzMyUyRmF3czRfcmVxdWVzdCZYLUFtei1EYXRlPTIwMjYwMzE2VDA4NTA1NFomWC1BbXotRXhwaXJlcz0zMDAmWC1BbXotU2lnbmF0dXJlPTUzMDZjYTZmNTU2MWNiMTY1YjczMjM0YjA3MzA3Y2NmNGZmZDkzNTAxZjQ3YTU0NTNhODcwNDliZTFmZWU2MjUmWC1BbXotU2lnbmVkSGVhZGVycz1ob3N0In0.W7zvGrsHp241lbYtG_df0v-mkLML7IxaODxoIkHPAUM)

### 2. GitHub Secrets の設定

リポジトリの Settings > Secrets and variables > Actions に以下を追加します。

| シークレット名         | 値                                     |
| ---------------------- | -------------------------------------- |
| `NOTION_AUTH_TOKEN`    | NotionのAPIトークン                    |
| `NOTION_DATABASE_ID`   | エクスポート対象のデータベースID       |

### 3. GitHub Actions ワークフローの設定

```yaml
name: Export Notion to Markdown

on:
  workflow_dispatch:
  schedule:
    - cron: '0 0 * * *'  # 毎日0時に実行

jobs:
  export:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Export Notion pages
        id: notion-export
        uses: your-account/notion-to-markdown-converter-playground@main
        with:
          notion_auth_token: ${{ secrets.NOTION_AUTH_TOKEN }}
          notion_database_id: ${{ secrets.NOTION_DATABASE_ID }}
          output_directory_path_template: "articles/{{ publish | date.to_string '%Y' }}/{{ publish | date.to_string '%m' }}/{{ slug }}"

      - name: Commit and push
        if: steps.notion-export.outputs.exported_count != '0'
        run: |
          git config user.name "github-actions[bot]"
          git config user.email "github-actions[bot]@users.noreply.github.com"
          git add .
          git commit -m "chore: export notion pages"
          git push
```

## 入力パラメータ

| パラメータ名                      | 必須 | デフォルト値                                          | 説明                            |
| --------------------------------- | ---- | ----------------------------------------------------- | ------------------------------- |
| `notion_auth_token`               | ✅   | -                                                     | Notion APIの認証トークン        |
| `notion_database_id`              | ✅   | -                                                     | エクスポート対象のデータベースID |
| `output_directory_path_template`  | -    | `output/{{publish\|date.to_string('%Y/%m')}}/{{slug}}` | 出力ディレクトリのパステンプレート |

### output_directory_path_template の書き方

[Scriban](https://github.com/scriban/scriban) テンプレート構文を使用します。

```txt
利用可能な変数:
  publish  - 公開日時（DateTime型）
  title    - 記事タイトル
  slug     - スラグ（未設定時はtitleが使用される）

使用例:
  articles/{{ publish | date.to_string '%Y' }}/{{ publish | date.to_string '%m' }}/{{ slug }}
  → articles/2024/03/my-post
```

## 出力

| アウトプット名   | 説明                         |
| ---------------- | ---------------------------- |
| `exported_count` | エクスポートに成功したページ数 |

エクスポートされた各ページは `{output_directory}/index.md` として書き出されます。

## 開発

### 必要環境

- .NET 8.0 SDK

### セットアップ

```bash
git clone https://github.com/your-account/notion-to-markdown-converter-playground.git
cd notion-to-markdown-converter-playground
dotnet restore ./src/NotionMarkdownConverter.csproj
```

### テスト

```bash
dotnet test
```

### 統合テスト

統合テストはNotionのAPIを実際に叩くため、User Secretsの設定が必要です。

```bash
cd tests
dotnet user-secrets set "Notion:AuthToken" "secret_xxx"
dotnet user-secrets set "Notion:DatabaseId" "xxx"
dotnet test --filter "Category=Integration"
```

## ライセンス

MIT
