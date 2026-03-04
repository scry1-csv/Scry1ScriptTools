using OxyPlot;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Script
{
    /// <summary>
    /// 「TimeRoter」形式のスクリプト。
    /// 内部時間単位: <b>10ms 単位</b>。InternalTime の 1 = 10ms。
    /// Milliseconds への変換式: InternalTime * 10。
    /// ※ CSV 保存形式と InternalTime の差異に注意:
    ///   CSV 上の時刻は「秒の小数点表記」(例: 1.23)であり、
    ///   読み込み時に「小数表記秒 * 100」して InternalTime に変換するため、
    ///   1.23秒 → InternalTime=123 (= 1230ms ・ 10 = 123 tick)。
    ///   保存時は InternalTime を 100 で割り「秒」表記に戻す (<see cref="ScriptRow.CsvTime"/> 参照)。
    /// </summary>
    public partial class TimeRoter(List<TimeRoter.ScriptRow> scriptData, string fileName, string filePath) : IScript
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

        // 1 InternalTime = 10ms なので ms を 10 で割る。小数点以下は四捨五入。
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
                var d = decimal.Parse(splitted[0]);
                // CSV上の山山表記(秒)を * 100 して InternalTime(単位: 10ms) に変換。
                // 例: CSV値 1.23秒 -> d=1.23 -> time = 123  (123 * 10ms = 1230ms ≡ 1.23秒)
                int time = decimal.ToInt32(d * 100);

                script.Add(new ScriptRow()
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

        public void SaveScript(string path)
        {
            StringBuilder sb = new();

            foreach (var row in scriptData)
                sb.AppendLine($"{row.CsvTime},{row.Power}");

            File.WriteAllText(path, sb.ToString());
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
        public struct ScriptRow
        {
            /// <summary>10ms 単位 (1 = 10ms)。Milliseconds への変換は * 10。
            /// CSV値(秒小数点表記) からの変換式: int(CSV値 * 100)。</summary>
            public int InternalTime;
            /// <summary>0～1000まで</summary>
            public int Power;
            // InternalTime * 10 で ms に変換 (1 InternalTime = 10ms)。
            public readonly double Milliseconds { get => (double)InternalTime * 10; }
            public readonly string CsvTime { get => $"{(InternalTime / 100):F2}"; }

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
