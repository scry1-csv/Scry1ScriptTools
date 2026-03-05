using OxyPlot;

namespace Core.Script
{
    public class CustomDataPoint(double x, double y, string scriptTime) : IDataPointProvider
    {
        public double X { get; } = x;
        public double Y { get; } = y;
        public string HHMMSS { get; } = ScriptUtil.MillisecondsToHHMMSS(x);
        public string ScriptTime { get; } = scriptTime;
        public DataPoint GetDataPoint() => new(X, Y);
    }
}
