using Core;
using Core.Script;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SexToyScriptConverter.Converter
{
    public static class FunscriptToCoyote
    {
        #region Inner Class
        private enum ReproduceSpeedType { Normal, DownWeak, UpWeak }
        public record ConverterData(Func<Funscript, CoyoteScript> Method, string Name);
        #endregion

        #region Private Field
        private static readonly int FREQ = 10;
        #endregion

        #region Public Field
        public static readonly ImmutableArray<ConverterData> Converters = [
            new(ConvertReproduceSpeedNormal, "速度再現"),
            new(ConvertReproduceSpeedUpWeak, "速度再現（上昇時弱）"),
            new(ConvertReproduceSpeedDownWeak, "速度再現（下降時弱）")
            ];
        #endregion

        #region Public Methods
        public static CoyoteScript ConvertReproduceSpeedNormal(Funscript funscript) => ConvertReproduceSpeedBase(funscript, ReproduceSpeedType.Normal);
        public static CoyoteScript ConvertReproduceSpeedUpWeak(Funscript funscript) => ConvertReproduceSpeedBase(funscript, ReproduceSpeedType.UpWeak);
        public static CoyoteScript ConvertReproduceSpeedDownWeak(Funscript funscript) => ConvertReproduceSpeedBase(funscript, ReproduceSpeedType.DownWeak);
        #endregion

        #region Private Methods

        private static int CalcSpeed(FunscriptJson.Action prev, FunscriptJson.Action now)
        {
            double duration = now.At - prev.At;
            if (duration == 0) return 0;
            double distance = Math.Abs(now.Pos - prev.Pos);
            if (distance == 0) return 0;
            double s = 25000.0 * Math.Pow((duration * 90.0 / distance), -1.05);

            return (int)Math.Round(CommonUtil.Normalize(s, 100, 0, 100, 5));
        }


        private static CoyoteScript ConvertReproduceSpeedBase(Funscript funscript, ReproduceSpeedType convertType)
        {
            var fs = funscript.Data.Actions;
            List<CoyoteScript.ScriptRow> tmp = [new() { InternalTime = 0, Frequency = 10, Strength = 0 }];

            bool start = false;
            var prevAct = new FunscriptJson.Action { At = 0, Pos = fs.Count > 0 ? fs[0].Pos : 0 };

            double factorDown = 0.333;

            foreach (var act in fs)
            {
                if (act.Pos == 0 && !start)
                {
                    prevAct = act;
                    continue;
                }
                else
                {
                    start = true;
                    int prevAt = prevAct.At;

                    int speed = CalcSpeed(prevAct, act);

                    switch (convertType)
                    {
                        case ReproduceSpeedType.UpWeak:
                            if (act.Pos < prevAct.Pos)
                                speed = (int)(speed * factorDown);
                            break;
                        case ReproduceSpeedType.DownWeak:
                            if (act.Pos > prevAct.Pos)
                                speed = (int)(speed * factorDown);
                            break;
                        case ReproduceSpeedType.Normal:
                        default:
                            break;
                    }


                    tmp.Add(new CoyoteScript.ScriptRow { InternalTime = prevAt, Frequency = FREQ, Strength = speed });
                    prevAct = act;
                }
            }

            int maxSpeed = tmp.Count > 0 ? tmp.Max(r => r.Strength) : 0;
            var nonzeroSpeeds = tmp.Where(r => r.Strength != 0).ToList();
            int minSpeed = nonzeroSpeeds.Count > 0 ? nonzeroSpeeds.Min(r => r.Strength) : 0;

            var result = new List<CoyoteScript.ScriptRow>();
            foreach (var row in tmp)
            {
                int normalizedStr = (int)CommonUtil.Normalize(row.Strength, maxSpeed, minSpeed, 100, 10);
                result.Add(new CoyoteScript.ScriptRow
                {
                    InternalTime = row.InternalTime,
                    Frequency = row.Frequency,
                    Strength = normalizedStr
                });
            }

            return new CoyoteScript { ScriptData = result, FileName = "", FilePath = "" };
        }
        #endregion
    }
}
