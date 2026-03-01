using Core;
using Core.Control;
using Core.Script;
using System.Windows;

namespace SexToyScriptConverter
{
    public class ChartController
    {

        #region Private Fields and Properties

        private readonly Controller _parent;
        private readonly ChartSyncManager _syncManager = new();

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

            _syncManager.AddChart(originChart);
            _syncManager.AddChart(convertedChart);
        }

        #endregion

        #region Public Methods

        public void SyncChartsRange(ChartControl sender, double min, double max)
        {
            _syncManager.SyncChartsRange(sender, min, max);
        }

        public void MovePlayingAnnotations(double milliseconds)
        {
            _syncManager.MovePlayingAnnotations(milliseconds);
        }

        public void SetOrigin(IScript script)
        {
            InitializeChartControlWithScript(originChart, script);
            
            switch(_parent.TimeAxisMode)
            {
                case (TimeAxisModeEnum.HHMMSS):
                    SetTimeAxisHHMMSS();
                    break;

                default:
                    SetTimeAxisInternal();
                    break;
            }

            originChart.Visibility = Visibility.Visible;
        }

        public void SetConverted(IScript script)
        {
            InitializeChartControlWithScript(convertedChart, script);

            switch (_parent.TimeAxisMode)
            {
                case (TimeAxisModeEnum.HHMMSS):
                    SetTimeAxisHHMMSS();
                    break;

                default:
                    SetTimeAxisInternal();
                    break;
            }

            if (_syncManager.ZoomMin > 0 | _syncManager.ZoomMax > 0)
                convertedChart.ZoomTimeAxis(_syncManager.ZoomMin, _syncManager.ZoomMax);

            convertedChart.Visibility = Visibility.Visible;
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
