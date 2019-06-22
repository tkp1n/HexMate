#if SPAN
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static HexMate.ScalarConstants;

namespace HexMate
{
    internal static partial class Utf8HexParser
    {
        internal static unsafe class Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool TryParse(ref byte* srcBytes, ref byte* destBytes, int destLength)
            {
                var src = srcBytes;
                var dest = destBytes;

                var target = dest + destLength;
                while (dest != target)
                {
                    var hi = Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(LookupUpperLowerHi), (IntPtr)(*src++));
                    var lo = Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(LookupUpperLowerLo), (IntPtr)(*src++));

                    if (hi >= 0xFE) goto HiErr;
                    if (lo >= 0xFE) goto LoErr;

                    *dest++ = (byte) (hi | lo);
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
#endif