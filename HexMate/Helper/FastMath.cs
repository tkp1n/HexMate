using System.Diagnostics;

namespace HexMate
{
    internal class FastMath
    {
        internal static int RoundDownTo32(int x)
        {
            Debug.Assert(x >= 0);
            return x & 0x7FFFFFE0;
        }

        internal static int RoundDownTo16(int x)
        {
            Debug.Assert(x >= 0);
            return x & 0x7FFFFFF0;
        }

        internal static int RoundDownTo2(int x)
        {
            Debug.Assert(x >= 0);
            return x & 0x7FFFFFFE;
        }
    }
}