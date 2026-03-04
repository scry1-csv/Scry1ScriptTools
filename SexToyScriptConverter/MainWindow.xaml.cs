using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SexToyScriptConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
                for (int i = 1; i < args.Length; i++)
                    _controller.OpenFile(args[i]);
        }

        #endregion

        #region XAML UI Event Handlers

        private void OpenButton_Click(object sender, RoutedEventArgs e) => _controller.OnOpenButtonClicked();
        private void SaveButton_Click(object sender, RoutedEventArgs e) => _controller.OnSaveButtonClicked();
        private void RadioButton_HHMMSS_Checked(object sender, RoutedEventArgs e) => _controller?.OnRadioButtonHHMMSSChecked();
        private void RadioButton_InternalTime_Checked(object sender, RoutedEventArgs e) => _controller?.OnRadioButtonInternalTimeChecked();

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

        private void TargetScriptTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combobox = sender as ComboBox;
            var selectedItem = combobox?.SelectedValue as string;
            _controller?.OnTargetScriptTypeComboBoxSelectionChanged(selectedItem);
        }


        private void ConvertMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _controller?.OnConvertMethodComboBoxSelectionChanged();
        }

        #endregion
    }
}