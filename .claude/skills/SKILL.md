---
name: csharp-impl-skill
description: "C#での実装に関するスキル"
---

# C# 実装スキル

## 基本ルール

- 最新のC#言語機能を積極的に使用する。
- プライマリコンストラクタが使用できる場合は、プライマリコンストラクタを使用する。
- 実装スタイルは[C# 実装スタイルガイド](references/impl-style.md)に従う。
- IAsyncDisposable インターフェースを実装している場合は、非同期Disposeパターンを使用する。
 - ex. await using 文を使用してリソースを解放する。