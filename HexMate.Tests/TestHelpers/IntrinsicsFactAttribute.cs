#if INTRINSICS
using System.Runtime.Intrinsics.X86;
using Xunit;

namespace HexMate.Tests.TestHelpers
{
    public sealed class Avx2FactAttribute : FactAttribute
    {
        public Avx2FactAttribute()
        {
            if (!Avx2.IsSupported)
            {
                Skip = "AVX2 is not supported on this platform";
            }
        }
    }

    public sealed class Sse41FactAttribute : FactAttribute
    {
        public Sse41FactAttribute()
        {
            if (!Sse41.IsSupported)
            {
                Skip = "SSE4.1 is not supported on this platform";
            }
        }
    }

    public sealed class Ssse3FactAttribute : FactAttribute
    {
        public Ssse3FactAttribute()
        {
            if (!Ssse3.IsSupported)
            {
                Skip = "SSSE3 is not supported on this platform";
            }
        }
    }

    public sealed class Sse2FactAttribute : FactAttribute
    {
        public Sse2FactAttribute()
        {
            if (!Sse2.IsSupported)
            {
                Skip = "SSE2 is not supported on this platform";
            }
        }
    }
}
#endif