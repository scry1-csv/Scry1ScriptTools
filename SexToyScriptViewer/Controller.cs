using Scry1ScriptTools.Core.Control;
using SexToyScriptViewer.Controllers;

namespace SexToyScriptViewer
{
    internal class Controller : IController
    {
        public MainWindow MainWindow { get; }
        public MediaController Media { get; }
        public ChartController Chart { get; }
        public FileController File { get; }
        public SyncController Sync { get; }

        public bool IsUserDragging { get; set; } = false;

        public Controller(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            Chart = new ChartController(this);
            Sync = new SyncController(this);
            Media = new MediaController(this);
            File = new FileController(this);
        }

        public void CloseChart(ChartControl control) => Chart.CloseChart(control);
        public void SyncChartsRange(ChartControl sender, double min, double max) => Chart.SyncChartsRange(sender, min, max);
        public void MovePlayingAnnotations(double milliseconds) => Chart.MovePlayingAnnotations(milliseconds);
        public void RefleshCharts() => Chart.RefleshCharts();
        public void OnFileDropped(string[] dropped) => File.OnFileDropped(dropped);
        public void OpenFile(string path) => File.OpenFile(path);
        public void OpenScript(string path) => Chart.OpenScript(path);
        public void ReloadChart(ChartControl control) => Chart.ReloadChart(control);
        public void LoadMedia(string path) => Media.LoadMedia(path);
        public void OnOpenButtonClicked() => File.OnOpenButtonClicked();
        public void OnRadioButtonHHMMSSChecked() => Chart.OnRadioButtonHHMMSSChecked();
        public void OnRadioButtonInternalTimeChecked() => Chart.OnRadioButtonInternalTimeChecked();
        public void PlayMedia() => Media.PlayMedia();
        public void PauseMedia() => Media.PauseMedia();
        public void StopMedia() => Media.StopMedia();
        public void OnMediaProgressDragStarted() => Media.OnMediaProgressDragStarted();
        public void OnMediaProgressDragCompleted() => Media.OnMediaProgressDragCompleted();
        public void OnMediaProgressValueChanged(double milliseconds) => Media.OnMediaProgressValueChanged(milliseconds);
        public void OnVolumeSliderValueChanged(double volume) => Media.OnVolumeSliderValueChanged(volume);
        public void OnMouseWheel(double delta) => Media.OnMouseWheel(delta);
        public void OnMediaOpened() => Media.OnMediaOpened();
        public void OnMediaFailed() => Media.OnMediaFailed();
        public void OnPlaybackSpeedValueChanged(double ratio) => Media.OnPlaybackSpeedValueChanged(ratio);
        public void OnSpeed1xButtonClicked() => Media.OnSpeed1xButtonClicked();
    }
}
