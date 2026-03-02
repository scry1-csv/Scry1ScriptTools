using System.Windows;

namespace SexToyScriptViewer
{
    public partial class MainWindow : Window
    {
        #region Private Fields

        private readonly Controller _controller;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            RadioButton_HHMMSS.IsChecked = true;

            _controller = new Controller(this);

            // MediaPlayerControlのイベントを購読してChartControllerへブリッジする
            MediaPlayer.PositionChanged += ms => _controller.MovePlayingAnnotations(ms);
        }

        #endregion

        #region XAML UI Event Handlers

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            _controller.OnOpenButtonClicked();
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            // ドラッグされているのがファイルなら許容、それ以外なら不許可
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] dropped)
                _controller.OnFileDropped(dropped);
        }

        private void RadioButton_HHMMSS_Checked(object sender, RoutedEventArgs e)
        {
            _controller?.OnRadioButtonHHMMSSChecked();
        }

        private void RadioButton_InternalTime_Checked(object sender, RoutedEventArgs e)
        {
            _controller?.OnRadioButtonInternalTimeChecked();
        }

        #endregion
    }
}
