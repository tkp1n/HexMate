#if INTRINSICS
using System.Diagnostics;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Ssse3;
using static System.Runtime.Intrinsics.X86.Sse2;
using static HexMate.VectorConstants;
using static HexMate.VectorUtils;

namespace HexMate
{
    internal static partial class Utf8HexFormatter
    {
        internal static unsafe class Ssse3
        {
            internal static int Format(ref byte* srcBytes, ref byte* destBytes, int srcLength, bool toLower = false)
            {
                Debug.Assert(System.Runtime.Intrinsics.X86.Ssse3.IsSupported);
                Debug.Assert(srcLength >= 16);

                var x0F = Vector128.Create((byte) 0x0F);
                var lowerHexLookupTable = ReadVector128(s_lowerHexLookupTable);
                var upperHexLookupTable = ReadVector128(s_upperHexLookupTable);
                var hexLookupTable = toLower ? lowerHexLookupTable : upperHexLookupTable;

                var src = srcBytes;
                var dest = destBytes;

                var bytesToRead = FastMath.RoundDownTo16(srcLength);
                var bytesToWrite = bytesToRead << 1;

                var start = dest - bytesToWrite;

                do
                {
                    src -= 16;
                    var value = LoadVector128(src);

                    var hiShift = ShiftRightLogical(value.AsInt16(), 4).AsByte();
                    var hiHalf = And(hiShift, x0F);
                    var loHalf = And(value, x0F);
                    var hi = UnpackHigh(hiHalf, loHalf);
                    var lo = UnpackLow(hiHalf, loHalf);

                    var resHi = Shuffle(hexLookupTable, hi);
                    var resLo = Shuffle(hexLookupTable, lo);

                    dest -= 16;
                    Store(dest, resHi);
                    dest -= 16;
                    Store(dest, resLo);
                } while (dest != start);

                srcBytes = src;
                destBytes = dest;

                return bytesToRead;
            }
        }
    }
}
#endif