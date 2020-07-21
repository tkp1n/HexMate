using System.Diagnostics;
using System.Runtime.CompilerServices;
using static HexMate.ScalarConstants;

namespace HexMate
{
    internal static partial class Utf8HexParser
    {
        internal static unsafe class Fixed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool TryParse(ref byte* srcBytes, ref byte* destBytes, int destLength)
            {
                var src = srcBytes;
                var dest = destBytes;

                var target = dest + destLength;

                int hi, lo;
                fixed (byte* lut = LookupUpperLower)
                {
                    while (dest != target)
                    {
                        hi = lut[*src++];
                        lo = lut[*src++];

                        if ((hi | lo) >= 0xFE) goto Err;

                        *dest++ = (byte) ((hi << 4) | lo);
                    }
                }

                srcBytes = src;
                destBytes = dest;
                return true;

            Err:
                if (hi >= 0xFE)
                {
                    srcBytes = src - 2;
                    destBytes = dest;
                    return false;
                }
                else
                {
                    Debug.Assert(lo >= 0xFE);
                    srcBytes = src - 1;
                    destBytes = dest;
                    return false;
                }
            }
        }
    }
}