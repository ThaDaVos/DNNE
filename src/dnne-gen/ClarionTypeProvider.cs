using System.Collections.Generic;

namespace DNNE
{
    internal class ClarionTypeProvider
    {
        internal struct MapStruct
        {
            public string ClarionType { get; init; }
            public string DefaultValue { get; init; }
        };

        internal static Dictionary<string, MapStruct> MAP = new()
        {
            { "double", new MapStruct { ClarionType = "REAL", DefaultValue = "0" } },
            { "float", new MapStruct { ClarionType = "REAL", DefaultValue = "0" } },
            { "intptr_t", new MapStruct { ClarionType = "LONG", DefaultValue = "0" } },
            { "int32_t", new MapStruct { ClarionType = "LONG", DefaultValue = "0" } },
        };

        internal static string MapTypeToClarion(string type)
        {
            return MAP.ContainsKey(type.ToLower())
                ? MAP[type.ToLower()].ClarionType
                : type.ToUpper();
        }
    
        internal static string ReplaceTypesWithDefaults(string argTypesString)
        {
            foreach (var mapping in MAP.Values)
            {
                argTypesString = argTypesString.Replace(mapping.ClarionType, mapping.DefaultValue);
            }

            return argTypesString;
        }
    }
}