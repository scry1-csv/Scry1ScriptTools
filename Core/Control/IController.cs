using Scry1ScriptTools.Core.Control;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Control
{
    public interface IController
    {
        public void ReloadChart(ChartControl control);
        public void CloseChart(ChartControl control);
        public void SyncChartsRange(ChartControl sender, double min, double max);
        public bool IsUserDragging { get; set; }
    }
}
