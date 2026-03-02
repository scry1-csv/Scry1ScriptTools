using Core;
using Core.Control;
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

        public void SetOrigin(
            string fileName,
            double plotMin, double plotMax,
            string trackerFormat,
            System.Collections.IEnumerable itemsSource,
            System.Collections.IEnumerable? itemsSource2,
            Func<double, string>? scriptTimeFormatter,
            IEnumerable<(double start, double end)>? differenceRanges)
        {
            originChart.InitializeChart(
                fileName,
                plotMin, plotMax,
                trackerFormat,
                itemsSource,
                itemsSource2,
                scriptTimeFormatter,
                differenceRanges
            );

            switch (_parent.TimeAxisMode)
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

        public void SetConverted(
            string fileName,
            double plotMin, double plotMax,
            string trackerFormat,
            System.Collections.IEnumerable itemsSource,
            System.Collections.IEnumerable? itemsSource2,
            Func<double, string>? scriptTimeFormatter,
            IEnumerable<(double start, double end)>? differenceRanges)
        {
            convertedChart.InitializeChart(
                fileName,
                plotMin, plotMax,
                trackerFormat,
                itemsSource,
                itemsSource2,
                scriptTimeFormatter,
                differenceRanges
            );

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
    }
}
