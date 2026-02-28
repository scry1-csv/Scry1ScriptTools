using Core.Script;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace SexToyScriptConverter.Converter
{
    internal class FunscriptToCoyote
    {
        public record ConverterData(Func<Funscript, CoyoteScript> Converter, string Name);

        public static readonly ImmutableArray<ConverterData> Converters = [
            new(ConvertReproduceSpeed, "上昇")
            ];

        public static CoyoteScript ConvertReproduceSpeed(Funscript funscript)
        {
            throw new NotImplementedException();
        }
    }
}
