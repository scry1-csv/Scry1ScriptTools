using Core;
using Core.Control;
using System.Windows;

namespace SexToyScriptConverter
{
    internal class ChartController
    {

        #region Private Fields and Properties

        private readonly ChartSyncManager _syncManager = new();

        private readonly ChartControl originChart;
        private readonly ChartControl convertedChart;

        #endregion

        #region Constructor

        internal ChartController(MainWindow mainWindow)
        {
            originChart = mainWindow.OriginChart;
            convertedChart = mainWindow.ConvertedChart;

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
            ApplyChartData(originChart, fileName, plotMin, plotMax, trackerFormat, itemsSource, itemsSource2, scriptTimeFormatter, differenceRanges);
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
            ApplyChartData(convertedChart, fileName, plotMin, plotMax, trackerFormat, itemsSource, itemsSource2, scriptTimeFormatter, differenceRanges);

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

        private static void ApplyChartData(
            ChartControl chart,
            string fileName,
            double plotMin, double plotMax,
            string trackerFormat,
            System.Collections.IEnumerable itemsSource,
            System.Collections.IEnumerable? itemsSource2,
            Func<double, string>? scriptTimeFormatter,
            IEnumerable<(double start, double end)>? differenceRanges)
        {
            chart.InitializeChart(
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
