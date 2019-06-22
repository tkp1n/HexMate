#if SPAN
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HexMate
{
    internal class VectorUtils
    {
        internal static TVector ReadVector<TVector>(ReadOnlySpan<byte> data)
            => Unsafe.As<byte, TVector>(ref MemoryMarshal.GetReference(data));
    }
}
#endif