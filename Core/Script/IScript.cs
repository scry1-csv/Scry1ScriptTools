using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Script
{
    /// <summary>
    /// 各スクリプト形式の内部時間単位（InternalTime）まとめ:
    /// <list type="table">
    ///   <listheader><term>クラス</term><description>単位 / Milliseconds への変換式</description></listheader>
    ///   <item><term>CoyoteScript</term><description>ミリ秒 (1 = 1ms) … InternalTime * 1</description></item>
    ///   <item><term>Funscript</term><description>ミリ秒 (1 = 1ms) … At フィールドそのまま</description></item>
    ///   <item><term>Vorze_SA</term><description>100ms 単位 (1 = 100ms) … InternalTime * 100</description></item>
    ///   <item><term>UFOTW</term><description>100ms 単位 (1 = 100ms) … InternalTime * 100</description></item>
    ///   <item><term>TimeRoter</term><description>10ms 単位 (1 = 10ms) … InternalTime * 10
    ///     ※ CSV 上は秒の小数点表記（例: 1.23）で保存され、読み込み時に「秒 * 100」して InternalTime に格納する</description></item>
    /// </list>
    /// 変換ロジックは各クラスの <see cref="MillisecondsToInternalTime"/> および ScriptRow.Milliseconds プロパティを参照。
    /// </summary>
    public interface IScript
    {

        public abstract int PlotMax { get; }
        public abstract int PlotMin { get; }
        public abstract string FileName { get; init; }
        public abstract string FilePath { get; init; }
        public abstract string TrackerFormatString { get; }

        public abstract string LabelFormatter_ScriptTime(double seconds);

        /// <summary>
        /// ミリ秒を各フォーマット固有の InternalTime に変換する。
        /// 実装ごとの変換係数は上記クラスコメントを参照。
        /// </summary>
        public abstract int MillisecondsToInternalTime(double milliseconds);
        public abstract IDataPointProvider[] ToPlot();
        public abstract void SaveScript(string path);


        public ScriptType ScriptType => ScriptUtil.GetScriptTypeFromScript(this);
    }
}
