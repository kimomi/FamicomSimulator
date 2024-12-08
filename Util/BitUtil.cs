using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator.Util
{
    public class BitUtil
    {
        public static bool IsBitSet(byte b, int index)
        {
            return (b & (1 << index)) != 0;
        }
        private static byte SetBit(byte b, int index)
        {
            return (byte)(b | (1 << index));
        }
        private static byte ClearBit(byte b, int index)
        {
            return (byte)(b & ~(1 << index));
        }
        private static byte ToggleBit(byte b, int index)
        {
            return (byte)(b ^ (1 << index));
        }

        public static byte SetBitValue(byte b, int index, byte value)
        {
            return value == 0 ? ClearBit(b, index) : SetBit(b, index);
        }

        public static byte GetBitValue(byte b, int index)
        {
            return (b & (1 << index)) != 0 ? (byte)1 : (byte)0;
        }
    }
}
