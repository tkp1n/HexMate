using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
#if SPAN
        public class TryFromHexChars : ConvertStringToBytesTestBase
        {
            [Fact]
            public void ProcessEmpty()
            {
                var result =  Convert.TryFromHexChars(Span<char>.Empty, Span<byte>.Empty, out var written);
                Assert.True(result);
                Assert.Equal(0, written);
            }

            [Theory]
            [InlineData(scalarNormal)]
            [InlineData(sseNormal)]
            [InlineData(avxNormal)]
            public void TargetTooSmall(string input)
            {
                Assert.Throws<FormatException>(() => ProcessString(input, CountUseful(input) - 1));
            }
            
            protected override byte[] ProcessString(string s, int expectedLength)
            {
                var result = new byte[expectedLength];
                if (!Convert.TryFromHexChars(s, result, out var written)) throw new FormatException();
                Assert.Equal(expectedLength, written);
                return result;
            }
        }
#endif
    }
}