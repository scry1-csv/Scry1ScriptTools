using Core;
using Microsoft.Win32;
using System.IO;

namespace SexToyScriptViewer.Controllers
{
    internal class FileController(Controller parent)
    {
        #region Public Methods

        public void OnFileDropped(string[] dropped)
        {
            foreach (string item in dropped) 
                OpenFile(item);

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

        #endregion
    }
}
