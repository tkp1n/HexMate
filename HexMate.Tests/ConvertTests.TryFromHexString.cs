using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
#if SPAN
        public class TryFromHexString : ConvertStringToBytesTestBase
        {
            [Theory]
            [InlineData(typeof(ArgumentNullException), null)]
            public void InputOutputError(Type exception, string s)
            {
                Assert.Throws(exception, () => Convert.TryFromHexString(s, Array.Empty<byte>(), out _));
            }

            [Fact]
            public void ProcessEmpty()
            {
                var result =  Convert.TryFromHexString(string.Empty, Span<byte>.Empty, out var written);
                Assert.True(result);
                Assert.Equal(0, written);
            }

            protected override byte[] ProcessString(string s, int expectedLength)
            {
                var result = new byte[expectedLength];
                if (!Convert.TryFromHexString(s, result, out var written)) throw new FormatException();
                Assert.Equal(expectedLength, written);
                return result;
            }
        }
#endif
    }
}