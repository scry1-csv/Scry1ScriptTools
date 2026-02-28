using System.Collections.Generic;

namespace Core.Control
{
    public class ChartSyncManager
    {
        #region Private Fields

        private readonly List<ChartControl> _chartControls = [];

        #endregion

        #region Public Properties

        public IReadOnlyList<ChartControl> ChartControls => _chartControls;
        public double ZoomMin { get; private set; }
        public double ZoomMax { get; private set; }

        #endregion

        #region Public Methods

        public void AddChart(ChartControl control)
        {
            if (!_chartControls.Contains(control))
            {
                _chartControls.Add(control);
                control.SyncChartsRangeRequested += Control_SyncChartsRangeRequested;
                
                if (ZoomMin > 0 || ZoomMax > 0)
                {
                    control.ZoomTimeAxis(ZoomMin, ZoomMax);
                }
            }
        }

        public void RemoveChart(ChartControl control)
        {
            if (_chartControls.Contains(control))
            {
                control.SyncChartsRangeRequested -= Control_SyncChartsRangeRequested;
                _chartControls.Remove(control);
            }
        }

        public void ClearCharts()
        {
            foreach (var c in _chartControls)
            {
                c.SyncChartsRangeRequested -= Control_SyncChartsRangeRequested;
            }
            _chartControls.Clear();
        }

        public void SyncChartsRange(ChartControl sender, double min, double max)
        {
            ZoomMin = min;
            ZoomMax = max;
            foreach (var item in _chartControls)
            {
                if (item != sender)
                {
                    item.ZoomTimeAxis(min, max);
                }
            }
        }

        public void MovePlayingAnnotations(double milliseconds)
        {
            foreach (var c in _chartControls)
            {
                c.MovePlayingAnnotation(milliseconds);
            }
        }

        public void SetTimeAxisHHMMSS()
        {
            foreach (var c in _chartControls)
            {
                c.SetTimeAxisLabelHHMMSS();
            }
        }

        public void SetTimeAxisInternal()
        {
            foreach (var c in _chartControls)
            {
                c.SetTimeAxisLabelInternalTime();
            }
        }

        #endregion

        #region Private Methods

        private void Control_SyncChartsRangeRequested(object? sender, (double Min, double Max) e)
        {
            if (sender is ChartControl control)
            {
                SyncChartsRange(control, e.Min, e.Max);
            }
        }

        #endregion
    }
}
