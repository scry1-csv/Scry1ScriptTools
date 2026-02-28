using Microsoft.Win32;
using System.IO;

namespace SexToyScriptViewer.Controllers
{
    internal class FileController(Controller parent)
    {
        public void OnFileDropped(string[] dropped)
        {
            if (dropped.Length == 1)
                OpenFile(dropped[0]);
            else
                Util.ShowMessageBoxTopMost("開けるのは同時に一つのファイルだけです！");
        }

        public void OpenFile(string path)
        {
            switch (Path.GetExtension(path))
            {
                case ".csv":
                case ".funscript":
                case ".coyotescript":
                    parent.Chart.OpenScript(path);
                    break;
                case ".mp3":
                case ".m4a":
                case ".wav":
                case ".mp4":
                case ".webm":
                case ".mpg":
                    parent.LoadMedia(path);
                    break;
                default:
                    Util.ShowMessageBoxTopMost("対応していないファイル形式です");
                    break;
            }
        }

        public void OnOpenButtonClicked()
        {
            parent.MainWindow.MediaPlayer.Stop();

            OpenFileDialog dlg = new() { Filter = Util.FileDialogFilter };
            bool? result = dlg.ShowDialog();
            if (result == true)
                OpenFile(dlg.FileName);
        }
    }
}
