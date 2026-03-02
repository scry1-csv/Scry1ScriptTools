using Core;
using Core.Control;
using Core.Script;
using Microsoft.Win32;
using OxyPlot;
using SexToyScriptConverter.Converter;
using SexToyScriptConverter.Properties;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SexToyScriptConverter
{
    internal enum TimeAxisModeEnum { HHMMSS, Internal }

    internal class Controller
    {
        #region Public Properties

        public MainWindow MainWindow { get; }
        public ChartController _chartController { get; }

        public bool IsUserDragging { get; set; } = false;
        public TimeAxisModeEnum TimeAxisMode { get; set; } = TimeAxisModeEnum.HHMMSS;
        public IScript? OriginScript { get; private set; }
        public IScript? ConvertedScript { get; private set; }


        #endregion

        #region Constructor

        public Controller(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            _chartController = new ChartController(this);
        }

        #endregion

        #region Public Methods

        public void SyncChartsRange(ChartControl sender, double min, double max) => _chartController.SyncChartsRange(sender, min, max);
        public void MovePlayingAnnotations(double milliseconds) => _chartController.MovePlayingAnnotations(milliseconds);
        public void LoadMedia(string path) => MainWindow.MediaPlayer.LoadMedia(path);

        private ScriptType GetTargetScriptType()
        {
            //if (MainWindow.TargetScriptTypeComboBox.SelectedItem == MainWindow.VorzeComboBoxItem)
            //    return ScriptType.Vorze;
            //else if (MainWindow.TargetScriptTypeComboBox.SelectedItem == MainWindow.TimeRoterComboBoxItem)
            //    return ScriptType.TimeRoter;
            //else if (MainWindow.TargetScriptTypeComboBox.SelectedItem == MainWindow.FunscriptComboBoxItem)
            //    return ScriptType.Funscript;
            //else (MainWindow.TargetScriptTypeComboBox.SelectedItem == MainWindow.CoyoteScriptComboBoxItem)
            //    return ScriptType.CoyoteScript;

            return ScriptType.CoyoteScript;
        }

        private void DisplayConverters(ScriptType? originScriptType)
        {
            if (originScriptType is null) return;

            MainWindow.ConvertMethodComboBox.Items.Clear();

            var target = GetTargetScriptType();

            if (originScriptType == ScriptType.Vorze)
                if (target == ScriptType.CoyoteScript)
                    foreach (var item in VorzeToCoyote.Converters)
                        MainWindow.ConvertMethodComboBox.Items.Add(item.Name);

            //if (Chart.OriginScriptType == ScriptType.TimeRoter)
            //    if (target == ScriptType.CoyoteScript)
            //        foreach (var item in VorzeToCoyote.Converters)
            //            MainWindow.ConvertMethodComboBox.Items.Add(item.Name);


            if (originScriptType == ScriptType.Funscript)
                if (target == ScriptType.CoyoteScript)
                    foreach (var item in FunscriptToCoyote.Converters)
                        MainWindow.ConvertMethodComboBox.Items.Add(item.Name);

            //if (Chart.OriginScriptType == ScriptType.CoyoteScript)
            //    if (target == ScriptType.CoyoteScript)
            //        foreach (var item in VorzeToCoyote.Converters)
            //            MainWindow.ConvertMethodComboBox.Items.Add(item.Name);

            MainWindow.ConvertMethodComboBox.SelectedIndex= 0;
        }


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
            _chartController.SetTimeAxisHHMMSS();
        }
        public void OnRadioButtonInternalTimeChecked()
        {
            TimeAxisMode = TimeAxisModeEnum.Internal;
            _chartController.SetTimeAxisInternal();
        }

        public void OnTargetScriptTypeComboBoxSelectionChanged(string? selected)
        {

        }

        public void OnConvertMethodComboBoxSelectionChanged()
        {
            var t = MainWindow.TargetScriptTypeComboBox.SelectedValue as ComboBoxItem;
            var targetStr = t?.Content.ToString();
            var methodStr = MainWindow.ConvertMethodComboBox.SelectedValue as string;

            if (targetStr is null || methodStr is null)
                return;
            
            ScriptType? targetType = GetScriptTypeFromResourceString(targetStr);
            if (targetStr == null)
                return;

            IScript? converted = null;

            if (OriginScript is Funscript origin)
                if(targetType == ScriptType.CoyoteScript)
                    converted = FunscriptToCoyote.Converters.Where(x => x.Name == methodStr).First().Method(origin);

            if(converted is not null)
                ApplyConverted(converted);
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
                    OpenScript(path);
                    break;
                default:
                    CommonUtil.ShowMessageBoxTopMost($"対応していないファイル形式です:\n{path}");
                    break;
            }
        }

        private void ApplyConverted(IScript script)
        {
            ConvertedScript = script;
            _chartController.SetConverted(script);
        }

        private void OpenScript(string path)
        {
            var scriptAndErrors = ScriptUtil.LoadScript(path);
            if (scriptAndErrors.Script == null)
            {
                CommonUtil.ShowMessageBoxTopMost($"スクリプトの読み込みに失敗しました。\n\n{string.Join("\n", scriptAndErrors.Errors)}");
                return;
            }

            OriginScript = scriptAndErrors.Script;
            _chartController.SetOrigin(scriptAndErrors.Script);
            DisplayConverters(scriptAndErrors.Script.ScriptType);
            MainWindow.ConvertSettingPanel.Visibility = Visibility.Visible;
        }

        private ScriptType? GetScriptTypeFromResourceString(string? str)
        {
            if (str == Resources.VorzeComboBoxItem)
                return ScriptType.Vorze;
            else if (str == Resources.TimeRoterComboBoxItem)
                return ScriptType.TimeRoter;
            else if (str == Resources.FunscriptComboBoxItem)
                return ScriptType.Funscript;
            else if (str == Resources.CoyoteScriptComboBoxItem)
                return ScriptType.CoyoteScript;
            else
                return null;
        }

        #endregion
    }
}
