# CreateSymlink

## 概要

CreateSymlinkは、Windows環境でシンボリックリンクを簡単に作成するためのツールです。<br>
右クリックのドラッグアンドドロップで表示されるコンテキストメニューから、複数のファイルやフォルダーのシンボリックリンクを、ドロップしたディレクトリに一括で作成できます。

![](.github_files/rec1.gif)

## 構成

- **CSCSharp**
    - `CreateSymlinkCSharp`<br>
    シンボリックリンク作成処理を行うC#コンソール/WinFormsアプリケーション。<br>
    実行ファイルはコンテキストメニュー拡張から呼び出されます。

- **CSShell**
    - `ContextMenu`<br>
    SharpShellを利用したWindowsエクスプローラーの右クリックメニュー拡張。<br>
    選択したファイル/フォルダーのパスとリンク先ディレクトリをCreateSymlinkCSharp.exeに渡して実行します。

## 主な機能

- 複数ファイル・フォルダーのシンボリックリンクを一括作成
- 既存リンクの上書き/リネーム/キャンセル選択
- 管理者権限が必要な場合は自動で昇格
- 独自のメッセージボックスUI（日本語対応）

## インストール方法

### インストーラーを使用する場合

Releaseからzipファイルをダウンロードして、setup.exeを実行してください。

### ビルドする場合

1. CreateSymlinkCSharpプロジェクトをビルドし、CreateSymlinkCSharp.exeを取得します。
2. ContextMenuプロジェクトをビルドし、SharpShellの手順に従いシェル拡張DLLを登録します。
3. エクスプローラーで右クリックし、「シンボリックリンクを作成」メニューが表示されれば成功です。

## 依存関係

- [.NET 8.0 (Windows)](https://dotnet.microsoft.com/)
- [SharpShell](https://github.com/dwmkerr/sharpshell)（CSShell/ContextMenu）

## ライセンス

Copyright © 2025 Yu-gene