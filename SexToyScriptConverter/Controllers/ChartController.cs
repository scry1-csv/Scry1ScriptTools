using Core;
using Core.Control;
using Core.Script;
using System.Windows;

namespace SexToyScriptConverter.Controllers
{
    public class ChartController
    {

        #region Private Fields and Properties

        private readonly Controller _parent;
        private readonly List<ChartControl> _chartControls = [];
        private double zoomMin = 0;
        private double zoomMax = 0;

        private readonly ChartControl originChart;
        private readonly ChartControl convertedChart;

        #endregion

        #region Constructor

        internal ChartController(Controller parent)
        {
            _parent = parent;
            originChart = _parent.MainWindow.OriginChart;
            convertedChart = _parent.MainWindow.ConvertedChart;

            originChart.HideButtons();
            convertedChart.HideFileInfoPanel();

            _chartControls.Add(originChart);
            _chartControls.Add(convertedChart);
        }

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

            originChart.SyncChartsRangeRequested += (s, e) => SyncChartsRange(originChart, e.Min, e.Max);
            InitializeChartControlWithScript(originChart, scriptAndErrors.Script);
            originChart.Visibility = Visibility.Visible;

            convertedChart.SyncChartsRangeRequested += (s, e) => SyncChartsRange(convertedChart, e.Min, e.Max);            
            InitializeChartControlWithScript(convertedChart, scriptAndErrors.Script);
            convertedChart.Visibility = Visibility.Visible;

            switch(_parent.TimeAxisMode)
            {
                case (TimeAxisModeEnum.HHMMSS):
                    SetTimeAxisHHMMSS();
                    break;

                default:
                    SetTimeAxisInternal();
                    break;
            }

        }

        public void SetTimeAxisHHMMSS()
        {
            foreach (var c in _chartControls)
                c.SetTimeAxisLabelHHMMSS();
        }

        public void SetTimeAxisInternal()
        {
            foreach (var c in _chartControls)
                c.SetTimeAxisLabelInternalTime();
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
