``` txt
src/
├── Extract/
│   ├── Abstractions/
│   │   └── INotionPageExtractor.cs
│   └── NotionPageExtractor.cs
├── Transform/
│   ├── Abstractions/
│   │   └── INotionPageTransformer.cs
│   ├── Converters/         ← Domain/Markdown/Converters から移動
│   ├── Strategies/         ← Domain/Transformers/Strategies から移動
│   └── NotionPageTransformer.cs
├── Load/
│   ├── Abstractions/
│   │   └── INotionPageLoader.cs
│   └── NotionPageLoader.cs
├── Pipeline/
│   ├── Models/             ← ★ フェーズ1 の追加場所
│   │   ├── ExtractedPage.cs
│   │   └── TransformedPage.cs
│   ├── Abstractions/
│   │   └── INotionExportPipeline.cs
│   └── NotionExportPipeline.cs  ← オーケストレーター
├── Configuration/          ← Application/Configuration から移動
├── Infrastructure/         ← そのまま
└── Program.cs
```