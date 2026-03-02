using Core;
using Core.Script;
using System.Collections.Immutable;

namespace SexToyScriptConverter.Converter
{
    public static class VorzeToCoyote
    {
        public record ConverterData(Func<Vorze_SA, CoyoteScript> Method, string Name);

        public static readonly ImmutableArray<ConverterData> Converters = [
            new(ConvertDirectReverseWeak, "逆回転時弱"),
            new(ConvertReverseStop, "逆回転時一時停止"),
            new(ConvertUp, "上昇"),
            new(ConvertDown, "下降"),
            ];

        public static CoyoteScript ConvertUp(Vorze_SA vorze)
        {
            var origin = vorze.ScriptData.ToArray();

            int oldmax = vorze.ScriptData.Max(r => r.Power);
            var nonZero = vorze.ScriptData.Where(r => r.Power > 0).ToArray();
            int oldmin = nonZero.Length > 0 ? nonZero.Min(r => r.Power) : 0;

            int freq = 10;

            List<CoyoteScript.ScriptRow> result = [];
            if (vorze.ScriptData.Count > 0 && vorze.ScriptData[0].Power != 0)
                result.Add(new() { InternalTime = 0, Frequency = freq, Strength = 0 });

            var prev = origin[0];
            foreach (var row in vorze.ScriptData)
            {
                if (prev.Power == 0)
                {
                    result.Add(new() { InternalTime = (int)prev.Milliseconds, Frequency = freq, Strength = 0 });
                    prev = row;
                    continue;
                }

                int targetStr = row.Power;
                int cursor = (int)prev.Milliseconds;
                double strength = 0;
                double stepCount = (row.Milliseconds - prev.Milliseconds) / 25.0;

                if (stepCount <= 0)
                {
                    prev = row;
                    continue;
                }

                double step = (strength - targetStr) / stepCount;
                while (cursor < row.Milliseconds)
                {
                    result.Add(new CoyoteScript.ScriptRow { InternalTime = cursor, Frequency = freq, Strength = (int)strength });
                    cursor += 25;
                    strength -= step;

                    if (strength > 100)
                    {
                        strength = 100;
                    }
                    else if (strength < 0)
                    {
                        strength = 0;
                    }
                }

                prev = row;
            }

            return new CoyoteScript { ScriptData = result, FileName = "", FilePath = "" };
        }

        public static CoyoteScript ConvertDown(Vorze_SA vorze)
        {
            var origin = vorze.ScriptData.ToArray();

            int oldmax = vorze.ScriptData.Max(r => r.Power);
            var nonZero = vorze.ScriptData.Where(r => r.Power > 0).ToArray();
            int oldmin = nonZero.Length > 0 ? nonZero.Min(r => r.Power) : 0;

            int freq = 10;

            List<CoyoteScript.ScriptRow> result = [];
            if (vorze.ScriptData.Count > 0 && vorze.ScriptData[0].Power != 0)
                result.Add(new() { InternalTime = 0, Frequency = freq, Strength = 0 });

            var prev = origin[0];
            foreach (var row in vorze.ScriptData)
            {
                if (prev.Power == 0)
                {
                    result.Add(new() { InternalTime = (int)prev.Milliseconds, Frequency = freq, Strength = 0 });
                    prev = row;
                    continue;
                }


                int targetStr = (row.Direction == prev.Direction && prev.Power != 0)
                    ? row.Power
                    : Math.Max(row.Power - 40, 0);

                int cursor = (int)prev.Milliseconds;

                double strength = prev.Power;
                double stepCount = (row.Milliseconds - prev.Milliseconds) / 25.0;

                if (stepCount <= 0)
                {
                    prev = row;
                    continue;
                }

                // 前のパワーから目標強度へ向かう 1 ステップあたりの変化量
                double step = (prev.Power - targetStr) / stepCount;
                while (cursor < row.Milliseconds)
                {
                    result.Add(new CoyoteScript.ScriptRow { InternalTime = cursor, Frequency = freq, Strength = (int)strength });
                    cursor += 25;
                    strength -= step;

                    if (strength > 100)
                        throw new Exception("strength > 100");
                }

                prev = row;
            }

            return new CoyoteScript { ScriptData = result, FileName = "", FilePath = "" };
        }

        public static CoyoteScript ConvertReverseStop(Vorze_SA vorze)
        {
            const int freq = 10;

            var result = new List<CoyoteScript.ScriptRow>();

            if (vorze.ScriptData.Count > 0 && vorze.ScriptData[0].Power != 0)
                result.Add(new() { InternalTime = 0, Frequency = freq, Strength = 0 });

            var prev = vorze.ScriptData[0];
            foreach (var row in vorze.ScriptData)
            {
                double diff = row.Milliseconds - prev.Milliseconds;

                if (row.Direction != prev.Direction && row.Power != 0)
                {
                    double delay = diff < 400 ? diff / 2 : 200;
                    result.Add(new() { InternalTime = (int)(row.Milliseconds - delay), Frequency = freq, Strength = 0 });
                }

                result.Add(new() { InternalTime = (int)row.Milliseconds, Frequency = freq, Strength = row.Power });

                prev = row;
            }

            return new CoyoteScript { ScriptData = result, FileName = "", FilePath = "" };
        }

        public static CoyoteScript ConvertDirectReverseWeak(Vorze_SA vorze)
        {
            const int freqUp = 10;
            const int freqDown = 20;
            const double factorDown = 0.5;

            var result = new List<CoyoteScript.ScriptRow>();

            if (vorze.ScriptData.Count > 0 && vorze.ScriptData[0].Power != 0)
                result.Add(new() { InternalTime = 0, Frequency = freqUp, Strength = 0 });

            foreach (var row in vorze.ScriptData)
            {
                int freq;
                int power;

                if (row.Direction)
                {
                    freq = freqUp;
                    power = row.Power;
                }
                else
                {
                    freq = freqDown;
                    power = (int)(row.Power * factorDown);
                }

                result.Add(new() { InternalTime = (int)row.Milliseconds, Frequency = freq, Strength = power });
            }

            return new CoyoteScript { ScriptData = result, FileName = "", FilePath = "" };
        }
    }
}
