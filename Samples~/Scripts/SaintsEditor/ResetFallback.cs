using System.Collections.Generic;

namespace SaintsField.Samples.Scripts.SaintsEditor
{
    public class ResetFallback : SaintsMonoBehaviour
    {
        public List<string> lis = new List<string>
        {
            "I'll", "follow", "you", "into", "the", "dark",
        };

        public long longV = 2L;
        public ulong ulongV = 2UL;
        public int intV = 2;
        public uint uintV = 2;
        public sbyte sbyteV = 2;
        public byte byteV = 2;
        public short shortV = 2;
        public ushort ushortV = 2;
    }
}
