using OxyPlot;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Script
{
    /// <summary>
    /// 「Coyote」形式のスクリプト。
    /// 内部時間単位: <b>ミリ秒 (ms)</b>。InternalTime の 1 = 1ms。
    /// Milliseconds への変換式: InternalTime * 1 (题差なし)。
    /// </summary>
    public partial class CoyoteScript : IScript
    {
        #region Public Properties
        public required List<ScriptRow> ScriptData { get; init; }
        public int PlotMax { get { return 100; } }
        public int PlotMin { get { return 0; } }
        public required string FileName { get; init; } = "";
        public required string FilePath { get; init; }
        public string TrackerFormatString { get { return "{1}: {HHMMSS} ({ScriptTime})\n{3}: {4}"; } }

        #endregion

        #region Public Methods
        // 内部時間単位 = ms なので、ms をそのまま int にキャストするだけ
        public int MillisecondsToInternalTime(double milliseconds) => (int)milliseconds;

        public string LabelFormatter_ScriptTime(double milliseconds) => milliseconds.ToString();


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
                script.Add(new ScriptRow()
                {
                    InternalTime = int.Parse(splitted[0]),
                    Frequency = int.Parse(splitted[1]),
                    Strength = int.Parse(splitted[2]),
                });
            }

            return new ScriptAndErrors(errors.Count == 0 ? new CoyoteScript
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
                sb.AppendLine($"{row.InternalTime},{row.Frequency},{row.Strength}");

            File.WriteAllText(path, sb.ToString());
        }

        public IDataPointProvider[] ToPlot()
        {
            List<CustomDataPoint> result = [new CustomDataPoint(0, 0)];

            int prevPower = 0;

            foreach (var row in ScriptData)
            {
                result.Add(new CustomDataPoint(row.Milliseconds, prevPower));
                result.Add(new CustomDataPoint(row.Milliseconds, row.Strength));
                prevPower = row.Strength;
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
        public struct ScriptRow
        {
            /// <summary>ミリ秒単位 (1 = 1ms)。Milliseconds プロパティへの変換は * 1（そのまま）。</summary>
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
