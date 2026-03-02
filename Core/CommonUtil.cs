using System.Windows;
using System.Collections.Immutable;
using Core.Script;

namespace Core
{
    public enum FileType { Media, Script, Other };
    public enum ScriptType { Vorze, TimeRoter, Funscript, CoyoteScript }

    public static class CommonUtil
    {
        #region Public Fields

        public static readonly ImmutableArray<string> MediaExts = [".mp3", ".m4a", ".wav", ".mp4", ".webm", ".mpg"];
        public static readonly ImmutableArray<string> ScriptExts = [".csv", ".coyotescript", ".funscript"];
        public static readonly ImmutableArray<string> Exts = ImmutableArray.Create(MediaExts.Concat(ScriptExts).ToArray());

        #endregion

        #region Public Properties

        public static string FileDialogFilter { get
            {
                string joined = string.Join(";", Exts.Select((s)=>"*"+s));
                return $"スクリプト,音声,動画 ({joined})|{joined}";
            } }

        #endregion

        #region Public Methods

        public static void ShowMessageBoxTopMost(string message)
        {
            MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

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

        public static FileType DetectFileType(string path)
        {
            var ext = System.IO.Path.GetExtension(path);

            if (MediaExts.Contains(ext))
                return FileType.Media;
            else if (ScriptExts.Contains(ext))
                return FileType.Script;
            else
                return FileType.Other;
        }

        public static double Normalize(double value, double oldMax, double oldMin, double newMax, double newMin)
        {
            if (value == 0) return 0;
            if (value > oldMax) value = oldMax;
            if (value < oldMin) value = oldMin;

            double oldRange = oldMax - oldMin;
            double newRange = newMax - newMin;

            if (oldRange <= 0 || newRange <= 0) return newMax;

            double newValue = (((value - oldMin) * newRange) / oldRange) + newMin;
            return newValue;
        }

        #endregion
    }
}
