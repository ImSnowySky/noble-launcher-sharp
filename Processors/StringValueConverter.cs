using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobleLauncher.Processors
{
    static class StringValueConverter
    {
        public static bool IsStrictlyTrue(string Value) {
            return Value == "true";
        }

        public static bool IsStrictlyTrue(IDictionary<string, string> Dict, string Key) {
            if (!Dict.ContainsKey(Key))
                return false;

            return IsStrictlyTrue(Dict[Key]);
        }

        public static List<string> ToList(string Value, char Separator) {
            if (Value.Length == 0 || Value == null)
                return new List<string>();

            var splittedString = Value.Split(Separator);
            return new List<string>(splittedString);
        }

        public static List<string> ToList(IDictionary<string, string> Dict, string Key, char Separator) {
            if (!Dict.ContainsKey(Key))
                return new List<string>();

            return ToList(Dict[Key], Separator);
        }
    }
}
