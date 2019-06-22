#if SPAN
using System;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
        public class FromHexString_Span : ConvertStringToBytesTestBase
        {
            [Fact]
            public void CachedEmptyArray()
            {
                var result = Convert.FromHexString(Span<char>.Empty);
                Assert.Same(Array.Empty<byte>(), result);
            }

            protected override byte[] ProcessString(string s, int _)
                => Convert.FromHexString(s.AsSpan());
        }
    }
}
#endif