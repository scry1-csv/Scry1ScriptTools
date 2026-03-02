using Core;
using Core.Control;
using Core.Script;
using Microsoft.Win32;

namespace SexToyScriptViewer
{
    internal enum TimeAxisModeEnum { HHMMSS, Internal }

    internal class Controller
    {
        #region Public Properties

        public MainWindow MainWindow { get; }
        public bool IsUserDragging { get; set; } = false;
        public TimeAxisModeEnum TimeAxisMode { get; set; } = TimeAxisModeEnum.HHMMSS;

        #endregion

        #region Private Fields

        private readonly ChartController _chartController;
        private readonly Dictionary<ChartControl, (IScript script, string filePath)> _chartScripts = [];

        #endregion

        #region Constructor

        public Controller(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            _chartController = new ChartController(this);
        }

        #endregion

        #region Public Methods

        public void CloseChart(ChartControl control)
        {
            UnregisterChart(control);
            _chartController.CloseChart(control);
        }

        public void SyncChartsRange(ChartControl sender, double min, double max) => _chartController.SyncChartsRange(sender, min, max);
        public void MovePlayingAnnotations(double milliseconds) => _chartController.MovePlayingAnnotations(milliseconds);
        public void RefleshCharts() => _chartController.RefleshCharts();

        public void ReloadChart(ChartControl control)
        {
            if (!_chartScripts.TryGetValue(control, out var entry)) return;

            var scriptAndErrors = ScriptUtil.LoadScript(entry.filePath);
            if (scriptAndErrors.Script == null)
            {
                CommonUtil.ShowMessageBoxTopMost($"スクリプトの読み込みに失敗しました。\n\n{string.Join("\n", scriptAndErrors.Errors)}");
                return;
            }

            _chartScripts[control] = (scriptAndErrors.Script, entry.filePath);
            ApplyScriptToChart(control, scriptAndErrors.Script);
            _chartController.RefleshCharts();
        }

        public void OpenScript(string path)
        {
            var scriptAndErrors = ScriptUtil.LoadScript(path);
            if (scriptAndErrors.Script == null)
            {
                CommonUtil.ShowMessageBoxTopMost($"スクリプトの読み込みに失敗しました。\n\n{string.Join("\n", scriptAndErrors.Errors)}");
                return;
            }

            ChartControl control = _chartController.CreateChartControl();
            ApplyScriptToChart(control, scriptAndErrors.Script);
            RegisterChart(control, scriptAndErrors.Script, path);
            _chartController.RefleshCharts();
        }

        public void LoadMedia(string path) => MainWindow.MediaPlayer.LoadMedia(path);

        public void OnOpenButtonClicked()
        {
            MainWindow.MediaPlayer.Stop();

            OpenFileDialog dlg = new() { Filter = CommonUtil.FileDialogFilter };
            bool? result = dlg.ShowDialog();
            if (result == true)
                OpenFile(dlg.FileName);
        }

        public void OnFileDropped(string[] dropped)
        {
            foreach (string item in dropped)
                OpenFile(item);
        }

        public void OnRadioButtonHHMMSSChecked()
        {
            TimeAxisMode = TimeAxisModeEnum.HHMMSS;
            _chartController.SetTimeAxisHHMMSS();
        }

        public void OnRadioButtonInternalTimeChecked()
        {
            TimeAxisMode = TimeAxisModeEnum.Internal;
            _chartController.SetTimeAxisInternal();
        }

        #endregion

        #region Private Methods

        private void OpenFile(string path)
        {
            switch (CommonUtil.DetectFileType(path))
            {
                case FileType.Media:
                    LoadMedia(path);
                    break;
                case FileType.Script:
                    OpenScript(path);
                    break;
                default:
                    CommonUtil.ShowMessageBoxTopMost($"対応していないファイル形式です:\n{path}");
                    break;
            }
        }

        private void RegisterChart(ChartControl control, IScript script, string filePath)
        {
            _chartScripts[control] = (script, filePath);
        }

        private void UnregisterChart(ChartControl control)
        {
            _chartScripts.Remove(control);
        }

        private void ApplyScriptToChart(ChartControl control, IScript script)
        {
            System.Collections.IEnumerable? itemsSource2 = null;
            IEnumerable<(double start, double end)>? differenceRanges = null;

            if (script is UFOTW u)
            {
                itemsSource2 = u.ToPlotRight();
                differenceRanges = u.DetectDeference();
            }

            _chartController.SetChartData(
                control,
                script.FileName,
                script.PlotMin, script.PlotMax,
                script.TrackerFormatString,
                script.ToPlot(),
                itemsSource2,
                script.LabelFormatter_ScriptTime,
                differenceRanges
            );
        }

        #endregion
    }
}
