using Microsoft.Win32;
using Scry1ScriptTools.Core.Control;
using Scry1ScriptTools.Core.Script;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SexToyScriptViewer
{
    public partial class MainWindow : Window
    {
        private readonly Controller _controller;

        public MainWindow()
        {
            InitializeComponent();
            RadioButton_HHMMSS.IsChecked = true;

            _controller = new Controller(this);

            // MediaPlayerControlのイベントを購読してChartControllerへブリッジする
            MediaPlayer.PositionChanged += ms => _controller.MovePlayingAnnotations(ms);
        }

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
    }
}
