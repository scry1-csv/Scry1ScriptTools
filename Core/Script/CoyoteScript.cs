using OxyPlot;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Script
{
    partial class CoyoteScript : IScript
    {
        #region Public Properties

        public int PlotMax { get { return 100; } }
        public int PlotMin { get { return 0; } }
        public required string FileName { get; init; } = "";
        public required string FilePath { get; init; }
        public string TrackerFormatString { get { return "{1}: {HHMMSS} ({ScriptTime})\n{3}: {4}"; } }

        #endregion

        #region Private Fields

        // Dataに不適正な内容を直接加えることを防ぐため、隠蔽してメソッドで操作を提供する
        private List<ScriptLine> _scriptData = new() { };

        #endregion

        #region Public Methods
        public int MillisecondsToInternalTime(double milliseconds) => (int)milliseconds;

        public string LabelFormatter_ScriptTime(double milliseconds) => milliseconds.ToString();


        public static ScriptAndErrors LoadScript(string path)
        {
            using var f = new StreamReader(path);
            var csv_str = f.ReadToEnd();

            var rows = ScriptUtil.RawCsvToLines(csv_str);

            List<ScriptLine> script = [];
            List<string> errors = [];
            var emptyline = ScriptUtil.EmptyLineRegex();
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
                    Frequency = int.Parse(splitted[1]),
                    Strength = int.Parse(splitted[2]),
                });
            }

            return new ScriptAndErrors(errors.Count == 0 ? new CoyoteScript
            {
                _scriptData = script,
                FileName = Path.GetFileName(path),
                FilePath = path
            } : null, errors);
        }

        public IDataPointProvider[] ToPlot()
        {
            List<CustomDataPoint> result = new() { new CustomDataPoint(0, 0) };

            int prevPower = 0;

            foreach (var line in _scriptData)
            {
                result.Add(new CustomDataPoint(line.Milliseconds, prevPower));
                result.Add(new CustomDataPoint(line.Milliseconds, line.Strength));
                prevPower = line.Strength;
            }

            return result.ToArray();
        }

        #endregion

        #region Private Methods

        [GeneratedRegex("^([0-9]+),(100|[0-9]{1,2}),(100|[0-9]{1,2})$")]
        private static partial Regex SyntaxRegex();

        #endregion

        #region Inner Types

        /// <summary>
        /// csvの行のデータを保持する構造体
        /// </summary>
        public struct ScriptLine
        {
            /// <summary>ミリ秒単位</summary>
            public int InternalTime;
            /// <summary>0～100まで</summary>
            public int Frequency;
            /// <summary>0～100まで</summary>
            public int Strength;
            public readonly double Milliseconds { get => InternalTime; }

            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.Append($"InternalTime:{InternalTime}, ");
                builder.Append($"Frequency:{Frequency}, ");
                builder.Append($"Strength:{Strength}");
                return builder.ToString();
            }
        }

        public class CustomDataPoint : IDataPointProvider, IScriptDataPoint
        {
            public double X { get; }
            public double Y { get; }
            public int Frequency { get; }
            public string HHMMSS { get; }
            public string ScriptTime { get; }
            public DataPoint GetDataPoint()
            {
                return new DataPoint(X, Y);
            }

            public CustomDataPoint(double x, double y)
            {
                X = x;
                Y = y;
                HHMMSS = ScriptUtil.MillisecondsToHHMMSS(x);
                ScriptTime = x.ToString();
            }
        }

        #endregion
    }
}
