using Core;
using Core.Control;
using Core.Script;
using System.Windows;

namespace SexToyScriptViewer.Controllers
{
    internal class ChartController
    {
        #region Private Fields

        private readonly Controller _parent;
        
        private readonly List<ChartControl> _chartControls = new();
        private readonly Dictionary<ChartControl, string> _chartFilePaths = new();
        private double zoomMin = 0;
        private double zoomMax = 0;

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
            _chartControls.Remove(control);
            _chartFilePaths.Remove(control);
            RefleshCharts();
        }

        public void SyncChartsRange(ChartControl sender, double min, double max)
        {
            zoomMin = min;
            zoomMax = max;
            foreach (var item in _chartControls)
                if (item != sender)
                    item.ZoomTimeAxis(min, max);
        }

        public void MovePlayingAnnotations(double milliseconds)
        {
            foreach (var c in _chartControls)
                c.MovePlayingAnnotation(milliseconds);
        }

        public void RefleshCharts()
        {
            _parent.MainWindow.ChartsPanel.Children.Clear();
            _parent.MainWindow.ChartsPanel.RowDefinitions.Clear();

            int i = 0;
            foreach (var c in _chartControls)
            {
                System.Windows.Controls.RowDefinition row = new();
                if (c.IsDualChart) row.Height = new GridLength(1.55, GridUnitType.Star);
                _parent.MainWindow.ChartsPanel.RowDefinitions.Add(row);

                System.Windows.Controls.Grid.SetRow(_chartControls[i], i);
                _parent.MainWindow.ChartsPanel.Children.Add(_chartControls[i]);
                i++;

                if (zoomMin > 0 | zoomMax > 0)
                    c.ZoomTimeAxis(zoomMin, zoomMax);
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
            
            ChartControl control = new(_parent);
            InitializeChartControlWithScript(control, scriptAndErrors.Script);

            _chartControls.Add(control);
            _chartFilePaths[control] = path;

            if (_parent.MainWindow.RadioButton_HHMMSS.IsChecked ?? false)
                control.SetTimeAxisLabelHHMMSS();
            else
                control.SetTimeAxisLabelScriptTime();

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

        public void OnRadioButtonHHMMSSChecked()
        {
            foreach (var c in _chartControls)
                c.SetTimeAxisLabelHHMMSS();
        }

        public void OnRadioButtonInternalTimeChecked()
        {
            foreach (var c in _chartControls)
                c.SetTimeAxisLabelScriptTime();
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
