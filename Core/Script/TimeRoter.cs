using OxyPlot;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Script
{
    partial class TimeRoter(List<TimeRoter.ScriptLine> scriptData, string fileName, string filePath) : IScript
    {
        #region Static Fields

        public static TimeRoter EmptyScript = new([], "", "");

        #endregion

        #region Public Properties

        public int PlotMax { get { return 1000; } }
        public int PlotMin { get { return 0; } }
        public string FileName { get; init; } = fileName;
        public string FilePath { get; init; } = filePath;
        public string TrackerFormatString { get { return "{1}: {HHMMSS} ({ScriptTime})\n{3}: {4}"; } }

        #endregion
        #region Constructor

        #endregion

        #region Public Methods

        public int MillisecondsToInternalTime(double milliseconds)
        {
            return Convert.ToInt32(milliseconds / 10);
        }

        public string LabelFormatter_ScriptTime(double milliseconds) => ((decimal)milliseconds / 10).ToString();

        public static ScriptAndErrors LoadScript(string path)
        {
            using var f = new StreamReader(path);
            var csv_str = f.ReadToEnd();

            var rows = ScriptUtil.RawCsvToLines(csv_str);

            List<ScriptLine> script = [];
            List<string> errors = [];
            var emptyline = ScriptUtil. EmptyLineRegex();
            var syntax = SyntaxRegex();

            for (int i = 0; i < rows.Count; i++)
            {
                if (emptyline.IsMatch(rows[i]))
                {
                    errors.Add($"{i + 1}行目: 空行です！");
                    continue;
                }

                if (!syntax.IsMatch(rows[i]))
                { 
                    errors.Add($"{i + 1}行目: 構文エラー");
                    continue;
                }

                var splitted = rows[i].Split(',');
                var d = decimal.Parse(splitted[0]);
                int time = decimal.ToInt32(d * 100);

                script.Add(new ScriptLine()
                {
                    InternalTime = time,
                    Power = int.Parse(splitted[1])
                });
            }

            return new ScriptAndErrors(errors.Count == 0 ? new TimeRoter(
                script,
                Path.GetFileName(path),
                path
            ) : null, errors);
        }

        public IDataPointProvider[] ToPlot()
        {
            List<CustomDataPoint> result = [new CustomDataPoint(0, 0)];

            int prevPower = 0;

            foreach (var line in scriptData)
            {
                result.Add(new CustomDataPoint(line.Milliseconds, prevPower));
                result.Add(new CustomDataPoint(line.Milliseconds, line.Power));
                prevPower = line.Power;
            }

            return result.ToArray();
        }

        #endregion

        #region Private Methods

        [GeneratedRegex("^([0-9]+)(\\.[0-9]{1,2})?,(1000|[0-9]{1,3})$")]
        private static partial Regex SyntaxRegex();

        #endregion

        #region Inner Types

        /// <summary>
        /// csvの行のデータを保持する構造体
        /// </summary>
        public struct ScriptLine
        {
            /// <summary>1/100秒単位</summary>
            public int InternalTime;
            /// <summary>0～1000まで</summary>
            public int Power;
            public readonly double Milliseconds { get => (double)InternalTime * 10; }

            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.Append($"InternalTime:{InternalTime}, ");
                builder.Append($"Power:{Power}");
                return builder.ToString();
            }
        }

        public class CustomDataPoint(double x, double y) : IDataPointProvider, IScriptDataPoint
        {
            public double X { get; } = x;
            public double Y { get; } = y;
            public string HHMMSS { get; } = ScriptUtil.MillisecondsToHHMMSS(x);
            public string ScriptTime { get; } = $"{x / 1000:F2}";
            public DataPoint GetDataPoint()
            {
                return new DataPoint(X, Y);
            }
        }

        #endregion
    }
}
