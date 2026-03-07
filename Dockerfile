# ─────────────────────────────────────────────────────────────
# ステージ1: ビルドステージ
#
# マルチステージビルドを使用しています。
# このステージでビルド・発行を行い、成果物だけを次のステージに引き渡します。
# SDK イメージはビルドにしか使わないので、最終イメージには含まれません。
# ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app

# nuget restore を先に分離することでキャッシュを活用します。
# .csproj が変わっていない限り、restore レイヤーが再利用されます。
# ソースコードだけ変更した場合のビルドが高速になります。
COPY *.sln .
COPY src/*.csproj ./src/
COPY tests/*.csproj ./tests/
RUN dotnet restore

# ソースコードをすべてコピーしてから発行します。
# -c Release     : リリース構成でビルド
# -o out         : 出力先ディレクトリ
# --no-self-contained : ランタイムを含めず、別途ランタイムイメージに依存する形にする
COPY . ./
RUN dotnet publish ./src/NotionMarkdownConverter.csproj -c Release -o out --no-self-contained

# ─────────────────────────────────────────────────────────────
# ステージ2: 実行ステージ
#
# SDK（約800MB）ではなく runtime（約200MB）イメージを使います。
# ビルド成果物だけをコピーするので、最終イメージが軽量になります。
# ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/runtime:8.0

# GitHub Actions のメタデータ
# これらのラベルは GitHub Actions Marketplace に表示される情報です。
# icon と color の選択肢: https://docs.github.com/en/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.name="notion-md-converter"
LABEL com.github.actions.description="Custom action to export Notion database to local markdown files."
LABEL com.github.actions.icon="activity"
LABEL com.github.actions.color="orange"

WORKDIR /app

# ビルドステージの成果物だけをコピーします。
# ソースコードやテストは含まれません。
COPY --from=build-env /app/out .

# アプリケーションのエントリーポイント
# action.yml の args で渡されたコマンドライン引数がそのまま渡されます。
ENTRYPOINT ["dotnet", "/app/NotionMarkdownConverter.dll"]