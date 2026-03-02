using Core;
using Core.Control;
using System.Windows;

namespace SexToyScriptViewer
{
    internal class ChartController
    {
        #region Private Fields

        private readonly Controller _parent;
        private readonly ChartSyncManager _syncManager = new();

        #endregion

        #region Constructor

        public ChartController(Controller parent)
        {
            _parent = parent;
        }

        #endregion

        #region Public Methods

        public ChartControl CreateChartControl()
        {
            ChartControl control = new();
            control.ReloadChartRequested += (s, e) => _parent.ReloadChart(control);
            control.CloseChartRequested += (s, e) => _parent.CloseChart(control);
            control.IsUserDraggingChanged += (s, e) => _parent.IsUserDragging = e;
            _syncManager.AddChart(control);

            if (_parent.MainWindow.RadioButton_HHMMSS.IsChecked ?? false)
                control.SetTimeAxisLabelHHMMSS();
            else
                control.SetTimeAxisLabelInternalTime();

            return control;
        }

        public void SetChartData(
            ChartControl control,
            string fileName,
            double plotMin, double plotMax,
            string trackerFormat,
            System.Collections.IEnumerable itemsSource,
            System.Collections.IEnumerable? itemsSource2,
            Func<double, string>? scriptTimeFormatter,
            IEnumerable<(double start, double end)>? differenceRanges)
        {
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

        public void CloseChart(ChartControl control)
        {
            _syncManager.RemoveChart(control);
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
