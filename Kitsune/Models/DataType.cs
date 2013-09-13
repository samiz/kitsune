using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kitsune
{
    [Serializable]
    public enum DataType
    {
        Number, Text, Script, Object, Invalid,
        Boolean
    }
    public static class DataTypeNames
    {
        public static string NameOf(DataType dt)
        {
            return dt.ToString();
        }

        public static DataType TypeOf(string name)
        {
            return (DataType) Enum.Parse(typeof(DataType), name);
        }
        public static string TypeFingerprint(IEnumerable<DataType> types)
        {
            return types.Select(t => DataTypeNames.NameOf(t)).Combine(",");
        }

        internal static DataType[] DecodeFingerprint(string typeFingerPrint)
        {
            if (typeFingerPrint == "")
                return new DataType[] { };
            return typeFingerPrint.Split(",".ToCharArray()).Select(t => TypeOf(t)).ToArray();
        }
    }
}
