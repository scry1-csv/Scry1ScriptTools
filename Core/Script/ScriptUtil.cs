using System.IO;
using System.Text.RegularExpressions;

namespace Core.Script
{
    public record ScriptAndErrors(IScript? Script, List<string> Errors);

    public partial class ScriptUtil
    {
        #region Public Methods

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
                {
                    result = CoyoteScript.LoadScript(path);
                    return new ScriptAndErrors(result, []);
                }

                var ufotw = UFOTW.LoadScript(path);
                if (ufotw.Script != null) return ufotw;

                var vorze = Vorze_SA.LoadScript(path);
                if (vorze.Script != null) return vorze;

                var timeRoter = TimeRoter.LoadScript(path);
                if (timeRoter.Script != null) return timeRoter;

                ScriptAndErrors best = ufotw;
                if (vorze.Errors.Count < best.Errors.Count) best = vorze;
                if (timeRoter.Errors.Count < best.Errors.Count) best = timeRoter;

                return best;
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
