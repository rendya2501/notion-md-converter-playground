# コントリビューションガイドライン

このプロジェクトへの貢献に興味を持っていただき、ありがとうございます。このドキュメントでは、プロジェクトへの貢献方法について説明します。

## 開発環境のセットアップ

1. 必要なツール
   - .NET 8.0 SDK
   - Visual Studio 2022 または Visual Studio Code
   - Git

2. リポジトリのクローン
   ```bash
   git clone https://github.com/your-username/notion-to-markdown-converter-playground.git
   cd notion-to-markdown-converter-playground
   ```

3. 依存関係のインストール
   ```bash
   dotnet restore
   ```

## 開発フロー

1. 新しいブランチの作成
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. 変更の実装
   - コードの変更を行います
   - テストを追加・更新します
   - ドキュメントを更新します

3. コミット
   ```bash
   git add .
   git commit -m "feat: 変更内容の説明"
   ```

4. プッシュ
   ```bash
   git push origin feature/your-feature-name
   ```

5. プルリクエストの作成
   - GitHubでプルリクエストを作成します
   - 変更内容を詳細に説明します
   - 関連するIssueをリンクします

## コーディング規約

- C#のコーディング規約に従います
- `.editorconfig`の設定に従います
- 新しい機能には必ずテストを追加します
- ドキュメントを適切に更新します

## コミットメッセージの規約

以下のプレフィックスを使用してください：
- `feat:` 新機能
- `fix:` バグ修正
- `docs:` ドキュメントの変更
- `style:` コードスタイルの変更
- `refactor:` リファクタリング
- `test:` テストの追加・修正
- `chore:` ビルドプロセスやツールの変更

## プルリクエストのレビュー

- コードレビューは必須です
- CIのテストが全て成功する必要があります
- ドキュメントが適切に更新されている必要があります

## 質問やサポート

- Issueを作成して質問してください
- ディスカッションで議論を始めることもできます

## ライセンス

このプロジェクトへの貢献は、プロジェクトのライセンスの下で公開されることに同意したものとみなされます。 