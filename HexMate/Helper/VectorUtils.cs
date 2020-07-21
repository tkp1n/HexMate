#if INTRINSICS
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HexMate
{
    internal class VectorUtils
    {
        internal static Vector128<byte> ReadVector128(ReadOnlySpan<byte> data)
            => Unsafe.ReadUnaligned<Vector128<byte>>(ref MemoryMarshal.GetReference(data));

        internal static Vector256<byte> ReadVector256(ReadOnlySpan<byte> data)
            => Unsafe.ReadUnaligned<Vector256<byte>>(ref MemoryMarshal.GetReference(data));
    }
}
#endif