#if SPAN
using System;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
        public static partial class ToHexString
        {
            public class SpanBased : ConvertBytesToStringTestBase
            {
                [Fact]
                public void ProcessEmptyBytes()
                {
                    var result = Convert.ToHexString(ReadOnlySpan<byte>.Empty);
                    Assert.Same(string.Empty, result);
                }

                protected override void AcceptOptions(HexFormattingOptions options)
                {
                    Convert.ToHexString(ReadOnlySpan<byte>.Empty, options);
                }

                protected override string ProcessBytes(byte[] bytes, int expectedLength, HexFormattingOptions options)
                {
                    var result = Convert.ToHexString(bytes.AsSpan(), options);
                    Assert.Equal(expectedLength, result.Length);
                    return result;
                }
            }
        }
    }
}
#endif