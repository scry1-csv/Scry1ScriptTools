using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Core.Script
{
    public record ScriptAndErrors(IScript? Script, List<string> Errors);
    public enum ScriptType { Vorze, TimeRoter, Funscript, CoyoteScript }

    public partial class ScriptUtil
    {
        #region Public Fields

        public static Dictionary<ScriptType, string> TypeExtentionMap = new()
        {
            { ScriptType.Vorze, ".csv" },
            { ScriptType.TimeRoter, ".csv" },
            { ScriptType.Funscript, ".funscript" },
            { ScriptType.CoyoteScript, ".coyotescript" }
        };

        #endregion

        #region Public Methods
        public static ScriptType GetScriptTypeFromScript(IScript Script)
        {
            return Script switch
            {
                Vorze_SA => ScriptType.Vorze,
                TimeRoter => ScriptType.TimeRoter,
                Funscript => ScriptType.Funscript,
                CoyoteScript => ScriptType.CoyoteScript,
                _ => throw new NotImplementedException()
            };
        }

        public static ScriptAndErrors LoadScript(string path)
        {
            try
            {
                IScript? result;
                if (Path.GetExtension(path) == ".funscript")
                {
                    result = Funscript.LoadScript(path);
                    return new ScriptAndErrors(result, []);
                }
                else if (Path.GetExtension(path) == ".coyotescript")
                    return CoyoteScript.LoadScript(path);

                var results = new List<ScriptAndErrors>
                {
                    UFOTW.LoadScript(path),
                    Vorze_SA.LoadScript(path),
                    TimeRoter.LoadScript(path),
                };

                var nonNull = results.Where(x => x.Script != null).ToList();
                if (nonNull.Count == 1)
                    return nonNull[0];
                else if (nonNull.Count > 1)
                    return new ScriptAndErrors(null, ["スクリプト種別が判別できませんでした"]);
                else
                {
                    // エラー行数が最も少ないフォーマットと判断し、そのエラーを返す
                    return results.MinBy(x => x.Errors.Count) ?? new ScriptAndErrors(null, ["スクリプト種別が判別できませんでした"]);
                }
            }
            catch (Exception e)
            {
                return new ScriptAndErrors(null, [e.ToString()]);
            }
        }

        public static string MillisecondsToHHMMSS(double milliseconds) => TimeSpanToHHMMSS(new TimeSpan(0, 0, 0, 0, (int)milliseconds));
        public static string TimeSpanToHHMMSS(TimeSpan time)
        {
            string h = time.Hours == 0 ? "" : $"{time.Hours}時間";
            string m = time.Minutes == 0 ? "" : $"{time.Minutes}分";
            string s = $"{time.Seconds}秒";
            string milli = time.Milliseconds == 0 ? "" : $"{time.Milliseconds:000}ms";

            return $"{h}{m}{s}{milli}";
        }

        public static List<string> RawCsvToLines(string csv_str)
        {
            var tmp = _newLine().Replace(csv_str, "\n");
            List<string> lines = new(tmp.Split('\n'));
            if (lines.Last() == "")
                lines.RemoveAt(lines.Count - 1);

            return lines;
        }



        [GeneratedRegex(@"^$")]
        public static partial Regex EmptyLineRegex();

        #endregion

        #region Private Methods

        [GeneratedRegex("\r\n|\r")]
        private static partial Regex _newLine();

        #endregion
    }
}
