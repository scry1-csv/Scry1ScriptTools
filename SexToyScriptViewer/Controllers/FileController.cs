using Microsoft.Win32;
using System.IO;

namespace SexToyScriptViewer.Controllers
{
    internal class FileController
    {
        private readonly Controller _parent;

        public FileController(Controller parent)
        {
            _parent = parent;
        }

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
                    _parent.Chart.OpenScript(path);
                    break;
                case ".mp3":
                case ".m4a":
                case ".wav":
                case ".mp4":
                case ".webm":
                case ".mpg":
                    _parent.Media.LoadMedia(path);
                    break;
                default:
                    Util.ShowMessageBoxTopMost("対応していないファイル形式です");
                    break;
            }
        }

        public void OnOpenButtonClicked()
        {
            _parent.Media.StopMedia();

            OpenFileDialog dlg = new() { Filter = Util.FileDialogFilter };
            bool? result = dlg.ShowDialog();
            if (result == true)
                OpenFile(dlg.FileName);
        }
    }
}
