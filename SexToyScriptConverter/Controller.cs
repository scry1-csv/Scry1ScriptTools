using Core;
using Core.Control;
using Microsoft.Win32;

namespace SexToyScriptConverter
{
    internal enum TimeAxisModeEnum { HHMMSS, Internal }

    internal class Controller
    {
        #region Public Properties

        public MainWindow MainWindow { get; }
        public ChartController Chart { get; }

        public bool IsUserDragging { get; set; } = false;
        public TimeAxisModeEnum TimeAxisMode { get; set; } = TimeAxisModeEnum.HHMMSS;


        #endregion

        #region Constructor

        public Controller(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            Chart = new ChartController(this);
        }

        #endregion

        #region Public Methods

        public void SyncChartsRange(ChartControl sender, double min, double max) => Chart.SyncChartsRange(sender, min, max);
        public void MovePlayingAnnotations(double milliseconds) => Chart.MovePlayingAnnotations(milliseconds);
        public void OpenScript(string path) => Chart.OpenScript(path);
        public void LoadMedia(string path) => MainWindow.MediaPlayer.LoadMedia(path);

        public void OnOpenButtonClicked()
        {
            MainWindow.MediaPlayer.Stop();

            OpenFileDialog dlg = new() { Filter = CommonUtil.FileDialogFilter };
            bool? result = dlg.ShowDialog();
            if (result == true)
                OpenFile(dlg.FileName);
        }

        public void OnFileDropped(string[] dropped)
        {
            foreach (string item in dropped)
                OpenFile(item);
        }

        public void OnRadioButtonHHMMSSChecked()
        {
            TimeAxisMode = TimeAxisModeEnum.HHMMSS;
            Chart.SetTimeAxisHHMMSS();
        }
        public void OnRadioButtonInternalTimeChecked()
        {
            TimeAxisMode = TimeAxisModeEnum.Internal;
            Chart.SetTimeAxisInternal();
        }

        #endregion

        #region Private Methods

        private void OpenFile(string path)
        {
            switch (CommonUtil.DetectFileType(path))
            {
                case FileType.Media:
                    LoadMedia(path);
                    break;
                case FileType.Script:
                    Chart.OpenScript(path);
                    break;
                default:
                    CommonUtil.ShowMessageBoxTopMost($"対応していないファイル形式です:\n{path}");
                    break;
            }
        }

        #endregion
    }
}
