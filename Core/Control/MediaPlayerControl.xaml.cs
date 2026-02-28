using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Scry1ScriptTools.Core.Control
{
    /// <summary>
    /// メディア再生機能とシークバー同期を担うUserControl。
    /// 外部への通知はイベント経由で行い、Chart等と疎結合を保つ。
    /// </summary>
    public partial class MediaPlayerControl : UserControl
    {
        #region Events

        public event Action<double>? PositionChanged;
        public event Action? PlaybackStarted;
        public event Action? PlaybackStopped;

        #endregion

        #region Private Fields

        private readonly DispatcherTimer _annotationsSyncTimer;
        private readonly DispatcherTimer _seekbarSyncTimer;

        private bool _isUserDragging = false;
        private string _mediaDuration = "";

        #endregion

        #region Constructor

        public MediaPlayerControl()
        {
            InitializeComponent();

            _annotationsSyncTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            _annotationsSyncTimer.Tick += AnnotationsSyncTimer_Tick;

            _seekbarSyncTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _seekbarSyncTimer.Tick += SeekbarTimer_Tick;
        }

        #endregion

        #region Public Methods

        public void LoadMedia(string path)
        {
            MediaElem.ScrubbingEnabled = false;
            var uri = new Uri(path);
            Debug.WriteLine("try load: " + uri.ToString());
            MediaElem.Source = uri;
            // いったん再生してすぐ停止することでメディアを開く
            MediaElem.Play();
            MediaElem.Stop();
        }

        public void Play()
        {
            MediaElem.Play();
            StartSync();
            PlaybackStarted?.Invoke();
        }

        public void Pause()
        {
            MediaElem.Pause();
            StopSync();
            PlaybackStopped?.Invoke();
        }
        public void Stop()
        {
            MediaElem.Stop();
            StopSync();
            PlaybackStopped?.Invoke();
        }
        public double CurrentPositionMs => MediaElem.Position.TotalMilliseconds;
        #endregion

        #region Private Methods

        private void EnablePlayerElements()
        {
            PlayButton.IsEnabled = true;
            PauseButton.IsEnabled = true;
            VolumeSlider.IsEnabled = true;
            VolumeLabel.IsEnabled = true;
            MediaSeekbarSlider.IsEnabled = true;
        }

        private void UpdateMediaElapsedLabel(double milliseconds)
        {
            var elapsed = TimeSpan.FromMilliseconds(milliseconds).ToString(@"hh\:mm\:ss");
            MediaElapsedLabel.Content = elapsed + " / " + _mediaDuration;
        }

        private void StartSync()
        {
            _annotationsSyncTimer.Start();
            _seekbarSyncTimer.Start();
        }

        private void StopSync()
        {
            _annotationsSyncTimer.Stop();
            _seekbarSyncTimer.Stop();
        }

        private void AnnotationsSyncTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isUserDragging)
                PositionChanged?.Invoke(MediaElem.Position.TotalMilliseconds);
        }
        private void SeekbarTimer_Tick(object? sender, EventArgs e)
        {
            if (MediaElem.Source != null
                && MediaElem.NaturalDuration.HasTimeSpan
                && !_isUserDragging)
            {
                double position = MediaElem.Position.TotalMilliseconds;
                MediaSeekbarSlider.Value = position;
                UpdateMediaElapsedLabel(position);
            }
        }

        #endregion

        #region XAML UI Event Handlers

        private void Play_Click(object sender, RoutedEventArgs e) => Play();

        private void Pause_Click(object sender, RoutedEventArgs e) => Pause();

        private void MediaElem_MediaOpened(object sender, RoutedEventArgs e)
        {
            EnablePlayerElements();
            _mediaDuration = MediaElem.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss");
            MediaSeekbarSlider.Minimum = 0;
            MediaSeekbarSlider.Maximum = MediaElem.NaturalDuration.TimeSpan.TotalMilliseconds;
            MediaSeekbarSlider.Value = 0;
            UpdateMediaElapsedLabel(0);

            if (MediaElem.NaturalVideoHeight > 0)
            {
                // 動画ファイルの場合はスクラビングを有効化し、ウィンドウを縦に拡張する
                Debug.WriteLine("video");
                MediaElem.ScrubbingEnabled = true;
                Task.Run(() =>
                {
                    while (MediaElem.ActualHeight == 0)
                        Thread.Sleep(5);

                    Dispatcher.Invoke(() =>
                    {
                        // ホストウィンドウの高さをMediaElementの実際の高さ分だけ拡張する
                        if (Window.GetWindow(this) is Window window)
                            window.Height += (int)MediaElem.ActualHeight;
                    });
                });
            }
        }

        private void MediaElem_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(
                "メディアの読み込みに失敗しました",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error,
                MessageBoxResult.OK,
                MessageBoxOptions.DefaultDesktopOnly);
        }

        private void MediaProgressSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _isUserDragging = true;
        }

        private void MediaProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _isUserDragging = false;
            MediaElem.Position = TimeSpan.FromMilliseconds(MediaSeekbarSlider.Value);
            // シーク後に位置をChartへ通知する
            PositionChanged?.Invoke(MediaSeekbarSlider.Value);
        }

        private void MediaProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateMediaElapsedLabel(MediaSeekbarSlider.Value);
            PositionChanged?.Invoke(MediaSeekbarSlider.Value);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MediaElem != null && VolumeSlider.IsLoaded)
                MediaElem.Volume = VolumeSlider.Value;
        }

        private void PlaybackSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MediaElem is not null)
                MediaElem.SpeedRatio = PlaybackSpeedSlider.Value / 10;
        }

        private void Speed1xButton_Click(object sender, RoutedEventArgs e)
        {
            MediaElem.SpeedRatio = 1;
            PlaybackSpeedSlider.Value = 10;
        }

        private void StackPanel_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // マウスホイールで音量を調整する
            MediaElem.Volume += (e.Delta > 0) ? 0.1 : -0.1;
            VolumeSlider.Value = MediaElem.Volume;
        }

        #endregion
    }
}
