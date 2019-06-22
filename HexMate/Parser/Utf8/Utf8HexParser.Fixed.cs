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

                fixed (byte* lutHi = LookupUpperLowerHi)
                fixed (byte* lutLo = LookupUpperLowerLo)
                {
                    while (dest != target)
                    {
                        var hi = lutHi[*src++];
                        var lo = lutLo[*src++];

                        if (hi >= 0xFE) goto HiErr;
                        if (lo >= 0xFE) goto LoErr;

                        *dest++ = (byte) (hi | lo);
                    }
                }

                srcBytes = src;
                destBytes = dest;
                return true;

            HiErr:
                srcBytes = src - 2;
                destBytes = dest;
                return false;

            LoErr:
                srcBytes = src - 1;
                destBytes = dest;
                return false;
            }
        }
    }
}