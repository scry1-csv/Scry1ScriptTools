using Core.Control;
using SexToyScriptViewer.Controllers;

namespace SexToyScriptViewer
{
    internal class Controller : IController
    {
        #region Public Properties

        public MainWindow MainWindow { get; }
        public ChartController Chart { get; }
        public FileController File { get; }

        public bool IsUserDragging { get; set; } = false;

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

        public void CloseChart(ChartControl control) => Chart.CloseChart(control);
        public void SyncChartsRange(ChartControl sender, double min, double max) => Chart.SyncChartsRange(sender, min, max);
        public void MovePlayingAnnotations(double milliseconds) => Chart.MovePlayingAnnotations(milliseconds);
        public void RefleshCharts() => Chart.RefleshCharts();
        public void OnFileDropped(string[] dropped) => File.OnFileDropped(dropped);
        public void OpenFile(string path) => File.OpenFile(path);
        public void OpenScript(string path) => Chart.OpenScript(path);
        public void ReloadChart(ChartControl control) => Chart.ReloadChart(control);
        public void LoadMedia(string path) => MainWindow.MediaPlayer.LoadMedia(path);
        public void OnOpenButtonClicked() => File.OnOpenButtonClicked();
        public void OnRadioButtonHHMMSSChecked() => Chart.OnRadioButtonHHMMSSChecked();
        public void OnRadioButtonInternalTimeChecked() => Chart.OnRadioButtonInternalTimeChecked();

        #endregion
    }
}
