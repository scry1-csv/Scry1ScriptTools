using Core;
using Core.Script;
using System.Collections.Immutable;

namespace SexToyScriptConverter.Converter
{
    public static class VorzeToCoyote
    {
        public record ConverterData(Func<Vorze_SA, CoyoteScript> Converter, string Name);

        public static readonly ImmutableArray<ConverterData> Converters = [
            new(ConvertUp, "上昇")
            ];

        public static CoyoteScript ConvertUp(Vorze_SA vorze)
        {
            var origin = vorze.ScriptData.ToArray();

            int oldmax = vorze.ScriptData.Max(r => r.Power);
            var nonZero = vorze.ScriptData.Where(r => r.Power > 0).ToArray();
            int oldmin = nonZero.Length > 0 ? nonZero.Min(r => r.Power) : 0;

            int newmax = 100;
            int newmin = 20;

            foreach (var row in origin)
                if (row.Power != 0)
                    if (oldmax - oldmin != 0)
                        row.Power = (int)CommonUtil.Normalize(row.Power, oldmax, oldmin, newmax, newmin);
                    else
                        row.Power = newmin;

            int freq = 10;

            var result = new List<CoyoteScript.ScriptRow>
            {
                new() { InternalTime = 0, Frequency = freq, Strength = 0 }
            };

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
    }
}
