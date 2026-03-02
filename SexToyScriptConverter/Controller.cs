using Core;
using Core.Control;
using Core.Script;
using Microsoft.Win32;
using OxyPlot;
using SexToyScriptConverter.Converter;
using SexToyScriptConverter.Properties;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace SexToyScriptConverter
{
    internal enum TimeAxisModeEnum { HHMMSS, Internal }

    internal class Controller
    {
        #region Public Properties

        public MainWindow MainWindow { get; }
        public bool IsUserDragging { get; set; } = false;
        public TimeAxisModeEnum TimeAxisMode { get; set; } = TimeAxisModeEnum.HHMMSS;
        public IScript? OriginScript { get; private set; }
        public IScript? ConvertedScript { get; private set; }

        #endregion

        #region Private Field

        private readonly ChartController _chartController;

        #endregion

        #region Constructor

        public Controller(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            _chartController = new ChartController(MainWindow);
        }

        #endregion

        #region Public Methods

        public void SyncChartsRange(ChartControl sender, double min, double max) => _chartController.SyncChartsRange(sender, min, max);
        public void MovePlayingAnnotations(double milliseconds) => _chartController.MovePlayingAnnotations(milliseconds);
        public void LoadMedia(string path) => MainWindow.MediaPlayer.LoadMedia(path);

        private ScriptType GetTargetScriptType()
        {
            // 現在はCoyoteScript以外に対応していないためコメントアウト
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

            MainWindow.ConvertMethodComboBox.SelectedIndex = 0;
        }


        public void OnOpenButtonClicked()
        {
            MainWindow.MediaPlayer.Stop();

            OpenFileDialog dlg = new() { Filter = CommonUtil.FileDialogFilter };
            bool? result = dlg.ShowDialog();
            if (result == true)
                OpenFile(dlg.FileName);
        }

        public void OnSaveButtonClicked()
        {
            if (ConvertedScript is null || OriginScript is null) return;
            if (MainWindow.ConvertMethodComboBox.SelectedValue is not string methodStr)
                return;

            MainWindow.MediaPlayer.Stop();

            var dir = Path.GetDirectoryName(OriginScript.FilePath);
            var ext = CommonUtil.TypeExtentionMap[ConvertedScript.ScriptType];
            var name = $"{Path.GetFileNameWithoutExtension(OriginScript.FilePath)}_{methodStr}";
            var path = $"{dir}\\{name}{ext}"; ;

            int i = 1 ;
            while (File.Exists(path))
                path = $"{dir}\\{name}({i++}){ext}"; ;
           
            try
            {
                ConvertedScript.SaveScript(path);
            }
            catch (Exception e)
            {
                CommonUtil.ShowMessageBoxTopMost("ファイルの書き込みに失敗しました:\n\n" + e.ToString());
            }
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
            if (targetType == null)
                return;

            IScript? converted = null;

            if (OriginScript is Funscript origin)
                if (targetType == ScriptType.CoyoteScript)
                    converted = FunscriptToCoyote.Converters.Where(x => x.Name == methodStr).First().Method(origin);

            if (converted is not null)
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
 
            IDataPointProvider[]? itemsSource2 = null;
            List<(double start, double end)>? differenceRanges = null;
            if (OriginScript is UFOTW u)
            {
                itemsSource2 = u.ToPlotRight();
                differenceRanges = u.DetectDeference();
            }

            _chartController.SetConverted(script.FileName, script.PlotMin, script.PlotMax, script.TrackerFormatString,
                script.ToPlot(), itemsSource2, script.LabelFormatter_ScriptTime, differenceRanges);
            ApplyTimeAxisMode();
        }

        private void OpenScript(string path)
        {
            var scriptAndErrors = ScriptUtil.LoadScript(path);
            if (scriptAndErrors.Script == null)
            {
                CommonUtil.ShowMessageBoxTopMost($"スクリプトの読み込みに失敗しました。\n\n{string.Join("\n", scriptAndErrors.Errors)}");
                return;
            }

            var script = scriptAndErrors.Script;
            OriginScript = script;


            IDataPointProvider[]? itemsSource2 = null;
            List<(double start, double end)>? differenceRanges = null;
            if (OriginScript is UFOTW u)
            {
                itemsSource2 = u.ToPlotRight();
                differenceRanges = u.DetectDeference();
            }

            _chartController.SetOrigin(script.FileName, script.PlotMin, script.PlotMax, script.TrackerFormatString,
                script.ToPlot(), itemsSource2, script.LabelFormatter_ScriptTime, differenceRanges);
            ApplyTimeAxisMode();
            DisplayConverters(script.ScriptType);
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

        private void ApplyTimeAxisMode()
        {
            if (TimeAxisMode == TimeAxisModeEnum.HHMMSS)
                _chartController.SetTimeAxisHHMMSS();
            else
                _chartController.SetTimeAxisInternal();
        }

        #endregion
    }
}
