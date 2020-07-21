#if INTRINSICS
using System.Diagnostics;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Avx;
using static System.Runtime.Intrinsics.X86.Avx2;
using static HexMate.VectorConstants;
using static HexMate.VectorUtils;

namespace HexMate
{
    internal static partial class Utf8HexFormatter
    {
        internal static unsafe class Avx2
        {
            internal static int Format(ref byte* srcBytes, ref byte* destBytes, int srcLength, bool toLower = false)
            {
                Debug.Assert(System.Runtime.Intrinsics.X86.Avx2.IsSupported);
                Debug.Assert(srcLength >= 32);

                var x0F = Vector256.Create((byte) 0x0F);
                var lowerHexLookupTable = ReadVector256(s_lowerHexLookupTable);
                var upperHexLookupTable = ReadVector256(s_upperHexLookupTable);
                var hexLookupTable = toLower ? lowerHexLookupTable : upperHexLookupTable;

                var src = srcBytes;
                var dest = destBytes;

                var bytesToRead = FastMath.RoundDownTo32(srcLength);
                var bytesToWrite = bytesToRead << 1;

                var start = dest - bytesToWrite;

                do
                {
                    src -= 32;
                    var value = LoadVector256(src);

                    var hiShift = ShiftRightLogical(value.AsInt16(), 4).AsByte();
                    var hiHalf = And(hiShift, x0F);
                    var loHalf = And(value, x0F);
                    var hi13 = UnpackHigh(hiHalf, loHalf);
                    var lo02 = UnpackLow(hiHalf, loHalf);

                    var hi = Permute2x128(lo02, hi13, 0b0011_0001);
                    var lo = Permute2x128(lo02, hi13, 0b0010_0000);

                    var resHi = Shuffle(hexLookupTable, hi);
                    var resLo = Shuffle(hexLookupTable, lo);

                    dest -= 32;
                    Store(dest, resHi);
                    dest -= 32;
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