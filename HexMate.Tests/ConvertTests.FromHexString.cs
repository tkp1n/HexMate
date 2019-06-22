using System;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
        public class FromHexString : ConvertStringToBytesTestBase
        {
            [Theory]
            [InlineData(typeof(ArgumentNullException), null)]
            public void InputOutputError(Type exception, string s)
            {
                Assert.Throws(exception, () => Convert.FromHexString(s));
            }

            [Fact]
            public void CachedEmptyArray()
            {
                var result = Convert.FromHexString(string.Empty);
                Assert.Same(Array.Empty<byte>(), result);
            }

            protected override byte[] ProcessString(string s, int _)
                => Convert.FromHexString(s);
        }
    }
}