using System;
using System.Windows.Threading;

namespace SexToyScriptViewer.Controllers
{
    internal class SyncController
    {
        private readonly Controller _parent;
        private readonly DispatcherTimer _annotationsSyncTimer;
        private readonly DispatcherTimer _seekbarTimer;

        public SyncController(Controller parent)
        {
            _parent = parent;

            _annotationsSyncTimer = new() { Interval = TimeSpan.FromMilliseconds(10) };
            _annotationsSyncTimer.Tick += AnnotationsSyncTimer_Tick;

            _seekbarTimer = new() { Interval = TimeSpan.FromSeconds(1) };
            _seekbarTimer.Tick += SeekbarTimer_Tick;
        }

        public void StartSync()
        {
            _annotationsSyncTimer.Start();
            _seekbarTimer.Start();
        }

        public void StopSync()
        {
            _annotationsSyncTimer.Stop();
            _seekbarTimer.Stop();
        }

        private void AnnotationsSyncTimer_Tick(object? sender, EventArgs e)
        {
            if (!_parent.IsUserDragging)
                _parent.Chart.MovePlayingAnnotations(_parent.MainWindow.MediaElem.Position.TotalMilliseconds);
        }

        private void SeekbarTimer_Tick(object? sender, EventArgs e)
        {
            if ((_parent.MainWindow.MediaElem.Source != null) && _parent.MainWindow.MediaElem.NaturalDuration.HasTimeSpan && !_parent.IsUserDragging)
            {
                double position = _parent.MainWindow.MediaElem.Position.TotalMilliseconds;
                _parent.MainWindow.MediaSeekbarSlider.Value = position;
                _parent.Media.UpdateMediaElapsedLabel(position);
            }
        }
    }
}
