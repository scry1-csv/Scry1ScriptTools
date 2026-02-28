using Core;
using Core.Control;
using Core.Script;
using System.Windows;

namespace SexToyScriptConverter.Controllers
{
    internal class ChartController(Controller parent)
    {

        #region Private Fields

        private readonly List<ChartControl> _chartControls = new();
        private readonly Dictionary<ChartControl, string> _chartFilePaths = new();
        private double zoomMin = 0;
        private double zoomMax = 0;

        public bool IsUserDragging { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion
        #region Constructor

        #endregion

        #region Public Methods

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

        public void OpenScript(string path)
        {
            var scriptAndErrors = ScriptUtil.LoadScript(path);
            if (scriptAndErrors.Script == null)
            {
                CommonUtil.ShowMessageBoxTopMost($"スクリプトの読み込みに失敗しました。\n\n{string.Join("\n", scriptAndErrors.Errors)}");
                return;
            }
            
            ChartControl origin = new(parent);
            origin.HideButtons();
            InitializeChartControlWithScript(origin, scriptAndErrors.Script);

            ChartControl converted = new(parent);
            converted.HideFileInfoPanel();
            
            InitializeChartControlWithScript(converted, scriptAndErrors.Script);


            parent.MainWindow?.OriginChart.Children.Clear();
            parent.MainWindow?.OriginChart.Children.Add(origin);
            parent.MainWindow?.ConvertedChart.Children.Add(converted);

            if (parent.MainWindow?.RadioButton_HHMMSS.IsChecked ?? false)
                origin.SetTimeAxisLabelHHMMSS();
            else
                origin.SetTimeAxisLabelScriptTime();

        }

        public void SetTimeAxisHHMMSS()
        {
            foreach (var c in _chartControls)
                c.SetTimeAxisLabelHHMMSS();
        }

        public void SetTimeAxisInternal()
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
