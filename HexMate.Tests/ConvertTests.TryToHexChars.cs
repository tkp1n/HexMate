#if SPAN
using System;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
        public static partial class ToHexString
        {
            public class TryToHexChars : ConvertBytesToStringTestBase
            {
                [Fact]
                public void ProcessEmptyBytes()
                {
                    var result = Convert.TryToHexChars(ReadOnlySpan<byte>.Empty, Span<char>.Empty, out var written);
                    Assert.True(result);
                    Assert.Equal(0, written);
                }

                [Fact]
                public void TargetTooSmall()
                {
                    var result = Convert.TryToHexChars(new byte[2], Span<char>.Empty, out var written);
                    Assert.False(result);
                    Assert.Equal(0, written);
                }

                protected override void AcceptOptions(HexFormattingOptions options)
                {
                    Convert.TryToHexChars(ReadOnlySpan<byte>.Empty, Span<char>.Empty, out _, options);
                }

                protected override string ProcessBytes(byte[] bytes, int expectedLength, HexFormattingOptions options)
                {
                    var buffer = new char[expectedLength];

                    var result = Convert.TryToHexChars(bytes.AsSpan(), buffer.AsSpan(), out var written, options);
                    Assert.Equal(expectedLength, written);
                    Assert.True(result);

                    return new string(buffer);
                }
            }
        }
    }
}
#endif