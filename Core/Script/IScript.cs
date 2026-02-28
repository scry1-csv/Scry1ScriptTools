using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Script
{
    public interface IScript
    {

        public abstract int PlotMax { get; }
        public abstract int PlotMin { get; }
        public abstract string FileName { get; init; }
        public abstract string FilePath { get; init; }
        public abstract string TrackerFormatString { get; }

        public abstract string LabelFormatter_ScriptTime(double seconds);
        
        public abstract int MillisecondsToInternalTime(double milliseconds);
        public abstract IDataPointProvider[] ToPlot();

    }
}
