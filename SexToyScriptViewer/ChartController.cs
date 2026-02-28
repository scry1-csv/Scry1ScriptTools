using Core;
using Core.Control;
using Core.Script;
using System.Windows;

namespace SexToyScriptViewer
{
    internal class ChartController
    {
        #region Private Fields

        private readonly Controller _parent;
        
        private readonly ChartSyncManager _syncManager = new();
        private readonly Dictionary<ChartControl, string> _chartFilePaths = new();

        #endregion

        #region Constructor

        public ChartController(Controller parent)
        {
            _parent = parent;
        }

        #endregion

        #region Public Methods

        public void CloseChart(ChartControl control)
        {
            _syncManager.RemoveChart(control);
            _chartFilePaths.Remove(control);
            RefleshCharts();
        }

        public void SyncChartsRange(ChartControl sender, double min, double max)
        {
            _syncManager.SyncChartsRange(sender, min, max);
        }

        public void MovePlayingAnnotations(double milliseconds)
        {
            _syncManager.MovePlayingAnnotations(milliseconds);
        }

        public void RefleshCharts()
        {
            _parent.MainWindow.ChartsPanel.Children.Clear();
            _parent.MainWindow.ChartsPanel.RowDefinitions.Clear();

            int i = 0;
            foreach (var c in _syncManager.ChartControls)
            {
                System.Windows.Controls.RowDefinition row = new();
                if (c.IsDualChart) row.Height = new GridLength(1.55, GridUnitType.Star);
                _parent.MainWindow.ChartsPanel.RowDefinitions.Add(row);

                System.Windows.Controls.Grid.SetRow(c, i);
                _parent.MainWindow.ChartsPanel.Children.Add(c);
                i++;

                if (_syncManager.ZoomMin > 0 | _syncManager.ZoomMax > 0)
                    c.ZoomTimeAxis(_syncManager.ZoomMin, _syncManager.ZoomMax);

                switch (_parent.TimeAxisMode)
                {
                    case (TimeAxisModeEnum.HHMMSS):
                        SetTimeAxisHHMMSS();
                        break;
                    default:
                        SetTimeAxisInternal();
                        break;
                }
            }
        }

        public void OpenScript(string path)
        {
            var scriptAndErrors = ScriptUtil.LoadScript(path);
            if (scriptAndErrors.Script == null)
            {
                CommonUtil.ShowMessageBoxTopMost($"スクリプトの読み込みに失敗しました。\n\n{string.Join("\n", scriptAndErrors.Errors)}");
                return;
            }
            
            ChartControl control = new();
            control.ReloadChartRequested += (s, e) => ReloadChart(control);
            control.CloseChartRequested += (s, e) => CloseChart(control);
            control.IsUserDraggingChanged += (s, e) => _parent.IsUserDragging = e;
            InitializeChartControlWithScript(control, scriptAndErrors.Script);

            _syncManager.AddChart(control);
            _chartFilePaths[control] = path;

            if (_parent.MainWindow.RadioButton_HHMMSS.IsChecked ?? false)
                control.SetTimeAxisLabelHHMMSS();
            else
                control.SetTimeAxisLabelInternalTime();

            RefleshCharts();
        }

        public void ReloadChart(ChartControl control)
        {
            if (_chartFilePaths.TryGetValue(control, out var path))
            {
                var scriptAndErrors = ScriptUtil.LoadScript(path);
                if (scriptAndErrors.Script == null)
                {
                    CommonUtil.ShowMessageBoxTopMost($"スクリプトの読み込みに失敗しました。\n\n{string.Join("\n", scriptAndErrors.Errors)}");
                    return;
                }

                InitializeChartControlWithScript(control, scriptAndErrors.Script);
                RefleshCharts();                
            }
        }

        public void SetTimeAxisHHMMSS()
        {
            _syncManager.SetTimeAxisHHMMSS();
        }

        public void SetTimeAxisInternal()
        {
            _syncManager.SetTimeAxisInternal();
        }

        #endregion

        #region Private Methods

        private void InitializeChartControlWithScript(ChartControl control, IScript script)
        {
            string fileName = "";
            double plotMin = 0, plotMax = 100;
            string trackerFormat = "";
            System.Collections.IEnumerable? itemsSource = null;
            System.Collections.IEnumerable? itemsSource2 = null;
            Func<double, string>? scriptTimeFormatter = null;
            IEnumerable<(double start, double end)>? differenceRanges = null;

            fileName = script.FileName;
            plotMin = script.PlotMin;
            plotMax = script.PlotMax;
            trackerFormat = script.TrackerFormatString;
            itemsSource = script.ToPlot();
            scriptTimeFormatter = script.LabelFormatter_ScriptTime;
            
            if (script is UFOTW u)
            {
                itemsSource2 = u.ToPlotRight();
                differenceRanges = u.DetectDeference();
            }

            control.InitializeChart(
                fileName,
                plotMin, plotMax,
                trackerFormat,
                itemsSource,
                itemsSource2,
                scriptTimeFormatter,
                differenceRanges
            );
        }

        #endregion
    }
}
