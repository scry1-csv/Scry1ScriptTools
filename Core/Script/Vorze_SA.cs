using OxyPlot;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Script
{
    /// <summary>
    /// 「Vorze」形式のスクリプト。
    /// 内部時間単位: <b>100ms 単位</b>。InternalTime の 1 = 100ms。
    /// Milliseconds への変換式: InternalTime * 100。
    /// </summary>
    public partial class Vorze_SA : IScript
    {
        #region Public Properties
        public required List<ScriptRow> ScriptData { get; init; }
        public int PlotMax { get { return 100; } }
        public int PlotMin { get { return -100; } }
        public required string FileName { get; init; } = "";
        public required string FilePath { get; init; }
        public string TrackerFormatString { get { return "{1}: {HHMMSS} ({ScriptTime})\n{3}: {4}"; } }

        #endregion

        #region Public Methods

        // 1 InternalTime = 100ms なので ms を 100 で割る。小数点以下は四捨五入。
        public int MillisecondsToInternalTime(double milliseconds)
        {
            return Convert.ToInt32(milliseconds / 100);
        }

        public string LabelFormatter_ScriptTime(double milliseconds) => (milliseconds / 100).ToString();


        public static ScriptAndErrors LoadScript(string path)
        {
            using var f = new StreamReader(path);
            var rows = ScriptUtil.RawCsvToLines(f.ReadToEnd());
            List<ScriptRow> script = [];
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

                int time = int.Parse(splitted[0]);

                script.Add(new()
                {
                    InternalTime = time,
                    Direction = splitted[1] == "1",
                    Power = int.Parse(splitted[2])
                });
            }

            if (script.Count == 0)
            {
                return new ScriptAndErrors(null, errors);
            }

            return new ScriptAndErrors(errors.Count == 0 ? new Vorze_SA
            {
                ScriptData = script,
                FileName = Path.GetFileName(path),
                FilePath = path
            } : null, errors);
        }

        public void SaveScript(string path)
        {
            StringBuilder sb = new();

            foreach (var row in ScriptData)
                sb.AppendLine($"{row.InternalTime},{row.CsvDirection},{row.Power}");

            File.WriteAllText(path, sb.ToString());
        }

        public IDataPointProvider[] ToPlot()
        {
            List<CustomDataPoint> result = new() { new CustomDataPoint(0, 0, 0) };

            int prevPower = 0;

            foreach (var row in ScriptData)
            {
                int power;
                if (row.Direction == true)
                    power = row.Power;
                else
                    power = -row.Power;

                result.Add(new CustomDataPoint(row.Milliseconds, prevPower, row.InternalTime));
                result.Add(new CustomDataPoint(row.Milliseconds, power, row.InternalTime));
                prevPower = power;
            }

            return result.ToArray();
        }

        #endregion

        #region Private Methods

        [GeneratedRegex("^([0-9]+),([01]),(100|[0-9]{1,2})$")]
        private static partial Regex SyntaxRegex();

        #endregion

        #region Inner Types

        /// <summary>
        /// csvの行のデータを保持する構造体
        /// </summary>
        public record ScriptRow
        {
            /// <summary>100ms 単位 (1 = 100ms)。Milliseconds への変換は * 100。</summary>
            public int InternalTime;
            public bool Direction;
            public int Power;
            public string CsvDirection { get => Direction ? "1" : "0"; }
            // InternalTime * 100 で ms に変換する。
            public double Milliseconds => InternalTime * 100;
            public override string ToString()
            {
                var builder = new StringBuilder();
                builder.Append($"InternalTime:{InternalTime}, ");
                builder.Append($"Direction:{Direction}, ");
                builder.Append($"Power:{Power}");
                return builder.ToString();
            }
        }

        public class CustomDataPoint : IDataPointProvider, IScriptDataPoint
        {
            public double X { get; }
            public double Y { get; }
            public string HHMMSS { get; }
            public string ScriptTime { get; }

            public CustomDataPoint(double x, double y, int internalTime)
            {
                X = x;
                Y = y;
                HHMMSS = MillisecondsToHHMMSS(x);
                ScriptTime = internalTime.ToString();
            }

            private static string MillisecondsToHHMMSS(double milliseconds) =>
                ScriptUtil.TimeSpanToHHMMSS(new TimeSpan(0, 0, 0, 0, (int)milliseconds));

            public DataPoint GetDataPoint() => new(X, Y);
        }

        #endregion
    }
}
