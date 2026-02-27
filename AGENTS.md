# Scry1ScriptTools - AI Coding Agent Guidelines

このドキュメントは、AIコーディングエージェントがコードベースの一貫性を維持し、効率的に開発を進めるためのガイドラインです。

## 🏛️ アーキテクチャと全体像 (Big Picture Architecture)
プロジェクトは主に2つの要素で構成されています：
1. **Core**: スクリプトの解析やデータ構造を担うクラスライブラリ（`IScript`, `ScriptUtil`など）。
2. **SexToyScriptViewer**: UI（WPF）および全体制御。

### Pure MVCアーキテクチャ
当WPFプロジェクトは一般的なMVVMではなく、**完全なMVCパターン**を採用しています。
- **データバインディングは禁止**: XAMLでの`{Binding ...}`などは使用せず、名前付きコントロール（`x:Name`）をコードビハインドから直接操作します。
- **ViewからControllerへの委譲**: ビューは、UIイベントをすべて`Controller.cs`のメソッドに委譲します。
  ```csharp
  // MainWindow.xaml.cs の例
  private void OpenButton_Click(object sender, RoutedEventArgs e)
  {
      _controller.OnOpenButtonClicked();
  }
  ```
- **ファサードとしてのControllerと直接的なUI操作**: `Controller.cs` はビューからの入力を受け付ける窓口（Facade）として機能し、実際の処理は機能ごとに分割された各サブコントローラー（`MediaController`, `ChartController`, `FileController`, `SyncController`）に処理を委譲します。各コントローラーは親のControllerを経由してUIの参照にアクセスし、状態を手動で更新します。
  ```csharp
  // MediaController.cs 内での制御の例
  _parent.MainWindow.MediaElem.Play();
  ```

## 🛠️ プロジェクト固有の規約とパターン (Project Conventions)
- **手動UI同期処理 (イベント駆動)**: OxyPlotチャートのズーム等が発生した際、リアクティブプロパティに頼らず、`ChartController` がリストを反復処理して直接別UIを更新します。
  ```csharp
  // ChartController.cs 内の SyncChartsRange の例
  public void SyncChartsRange(ChartControl sender, double min, double max)
  {
      zoomMin = min;
      zoomMax = max;
      foreach (var item in _chartControls)
          if (item != sender) item.ZoomTimeAxis(min, max);
  }
  ```
- **タイマーベースの描画同期 (ポーリング)**: メディアの再生位置とグラフ同期は、`SyncController.cs` 内の `DispatcherTimer`（10ms間隔）を使用して定期的にメディア位置をポーリングし、更新を行っています。

## 🔌 統合と拡張性 (Integration & Extensibility)
- **新規スクリプト形式の追加**:
  1. `Core\Script\IScript.cs` を実装した新しいパーサクラスを作成。
  2. OxyPlot向け描画データ出力（`ToPlot()`）を実装。
  3. `Core\Script\ScriptUtil.cs`の`LoadScript()`にある拡張子判定分岐に追加。
- **コンポーネント間連携**: チャートの操作は `Core\Control\ChartControl.xaml.cs` に集約されており、`OxyPlot` のプロパティ操作（SeriesやAnnotationの追加）はここで行います。複数のチャートが同期して動くため、個別の独立した更新は`Controller`通して全体に波及させる（SyncChartsRange等）設計になっています。

## 🏃 開発ワークフロー
- プロジェクトは`.NET 10.0-windows`です。
- アプリの挙動をテスト・デバッグする際は、Visual Studio等で`SexToyScriptViewer`プロジェクトをスタートアップ・プロジェクトとして直接実行（F5等）してください。特殊なビルドスクリプトやCLIコマンドは不要です。
