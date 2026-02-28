using Core.Control;
using SexToyScriptConverter.Controllers;

namespace SexToyScriptConverter
{
    internal enum TimeAxisModeEnum { HHMMSS, Internal }

    internal class Controller : IController
    {
        #region Public Properties

        public MainWindow MainWindow { get; }
        public ChartController Chart { get; }
        public FileController File { get; }

        public bool IsUserDragging { get; set; } = false;
        public TimeAxisModeEnum TimeAxisMode { get; set; } = TimeAxisModeEnum.HHMMSS;


        #endregion

        #region Constructor

        public Controller(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            Chart = new ChartController(this);
            File = new FileController(this);
        }

        #endregion

        #region Public Methods

        public void SyncChartsRange(ChartControl sender, double min, double max) => Chart.SyncChartsRange(sender, min, max);
        public void MovePlayingAnnotations(double milliseconds) => Chart.MovePlayingAnnotations(milliseconds);
        public void OnFileDropped(string[] dropped) => File.OnFileDropped(dropped);
        public void OpenFile(string path) => File.OpenFile(path);
        public void OpenScript(string path) => Chart.OpenScript(path);
        public void ReloadChart(ChartControl control) => throw new NotImplementedException();
        public void LoadMedia(string path) => MainWindow.MediaPlayer.LoadMedia(path);
        public void OnOpenButtonClicked() => File.OnOpenButtonClicked();
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

        public void CloseChart(ChartControl control)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
