using System;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
        public class FromHexCharArray : ConvertStringToBytesTestBase
        {
            [Theory]
            [InlineData(typeof(ArgumentNullException), null, 0, 1)]
            [InlineData(typeof(ArgumentOutOfRangeException), new char[0], -1, 1)]
            [InlineData(typeof(ArgumentOutOfRangeException), new char[0], 0, -1)]
            [InlineData(typeof(ArgumentOutOfRangeException), new char[0], 0, 1)]
            [InlineData(typeof(ArgumentOutOfRangeException),new char[0], 1, 0)]
            public void InputOutputError(Type exception, char[] inArray, int offset, int length)
            {
                Assert.Throws(exception, () => Convert.FromHexCharArray(inArray, offset, length));
            }

            [Fact]
            public void CachedEmptyArray()
            {
                var result = Convert.FromHexCharArray(Array.Empty<char>(), 0, 0);
                Assert.Same(Array.Empty<byte>(), result);
            }

            protected override byte[] ProcessString(string s, int _)
                => Convert.FromHexCharArray(s.ToCharArray(), 0, s.Length);
        }
    }
}