using System;
using System.Diagnostics;
using static HexMate.ScalarConstants;

namespace HexMate
{
    internal static partial class Utf8HexFormatter
    {
        internal static unsafe class Fixed
        {
            internal static int Format(ref byte* srcBytes, ref byte* destBytes, int srcLength, bool toLower = false)
            {
                Debug.Assert(srcLength > 0);

                var src = srcBytes;
                var dest = destBytes;

                var bytesToRead = srcLength;
                var bytesToWrite = bytesToRead << 1;

                var start = dest - bytesToWrite;

                var destShort = (ushort*) dest;

                fixed (ushort* lut =
                    toLower
                        ? (BitConverter.IsLittleEndian ? s_utf8LookupHexLowerLE : s_utf8LookupHexLowerBE)
                        : (BitConverter.IsLittleEndian ? s_utf8LookupHexUpperLE : s_utf8LookupHexUpperBE))
                {
                    do
                    {
                        *(--destShort) = lut[*(--src)];
                    } while (destShort != start);
                }

                srcBytes = src;
                destBytes = (byte*) destShort;

                return bytesToRead;
            }
        }
    }
}
