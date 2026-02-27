using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SexToyScriptViewer.Controllers
{
    internal class MediaController
    {
        private readonly Controller _parent;
        private string _mediaDuration = "";

        public MediaController(Controller parent)
        {
            _parent = parent;
        }

        public void UpdateMediaElapsedLabel(double milliseconds)
        {
            var elapsed = TimeSpan.FromMilliseconds(milliseconds).ToString(@"hh\:mm\:ss");
            _parent.MainWindow.MediaElapsedLabel.Content = elapsed + " / " + _mediaDuration;
        }

        public void LoadMedia(string path)
        {
            _parent.MainWindow.MediaElem.ScrubbingEnabled = false;
            var uri = new Uri(path);
            Debug.WriteLine("try load: " + uri.ToString());
            _parent.MainWindow.MediaElem.Source = uri;
            _parent.MainWindow.MediaElem.Play();
            _parent.MainWindow.MediaElem.Stop();
        }

        public void PlayMedia()
        {
            _parent.MainWindow.MediaElem.Play();
            _parent.Sync.StartSync();
        }

        public void PauseMedia()
        {
            _parent.MainWindow.MediaElem.Pause();
            _parent.Sync.StopSync();
        }

        public void StopMedia()
        {
            _parent.MainWindow.MediaElem.Stop();
            _parent.Sync.StopSync();
        }

        public void OnMediaProgressDragStarted()
        {
            _parent.IsUserDragging = true;
        }

        public void OnMediaProgressDragCompleted()
        {
            _parent.IsUserDragging = false;
            _parent.MainWindow.MediaElem.Position = TimeSpan.FromMilliseconds(_parent.MainWindow.MediaSeekbarSlider.Value);
            _parent.Chart.MovePlayingAnnotations(_parent.MainWindow.MediaElem.Position.TotalMilliseconds);
        }

        public void OnMediaProgressValueChanged(double milliseconds)
        {
            UpdateMediaElapsedLabel(milliseconds);
            _parent.Chart.MovePlayingAnnotations(milliseconds);
        }

        public void OnVolumeSliderValueChanged(double volume)
        {
            if (_parent.MainWindow.MediaElem != null)
                _parent.MainWindow.MediaElem.Volume = volume;
        }

        public void OnMouseWheel(double delta)
        {
            _parent.MainWindow.MediaElem.Volume += (delta > 0) ? 0.1 : -0.1;
            _parent.MainWindow.VolumeSlider.Value = _parent.MainWindow.MediaElem.Volume;
        }

        public void OnMediaOpened()
        {
            _parent.MainWindow.EnablePlayerElements();
            _mediaDuration = _parent.MainWindow.MediaElem.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss");
            _parent.MainWindow.MediaSeekbarSlider.Minimum = 0;
            _parent.MainWindow.MediaSeekbarSlider.Maximum = _parent.MainWindow.MediaElem.NaturalDuration.TimeSpan.TotalMilliseconds;
            _parent.MainWindow.MediaSeekbarSlider.Value = 0;
            UpdateMediaElapsedLabel(0);

            if (_parent.MainWindow.MediaElem.NaturalVideoHeight > 0)
            {
                Debug.WriteLine("video");
                _parent.MainWindow.MediaElem.ScrubbingEnabled = true;
                Task.Run(() =>
                {
                    while (_parent.MainWindow.MediaElem.ActualHeight == 0)
                        Thread.Sleep(5);
                    
                    _parent.MainWindow.Dispatcher.Invoke(() =>
                    {
                        _parent.MainWindow.Height += (int)_parent.MainWindow.MediaElem.ActualHeight;
                        _parent.MainWindow.Resizer.Visibility = Visibility.Visible;
                    });
                });
            }
            else
                _parent.MainWindow.Resizer.Visibility = Visibility.Hidden;
        }

        public void OnMediaFailed()
        {
            Util.ShowMessageBoxTopMost("メディアの読み込みに失敗しました");
        }

        public void OnPlaybackSpeedValueChanged(double ratio)
        {
            if (_parent.MainWindow.MediaElem is not null)
                _parent.MainWindow.MediaElem.SpeedRatio = ratio;
        }

        public void OnSpeed1xButtonClicked()
        {
            _parent.MainWindow.MediaElem.SpeedRatio = 1;
            _parent.MainWindow.PlaybackSpeedSlider.Value = 10;
        }
    }
}
