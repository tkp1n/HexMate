using System;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
        public static partial class ToHexString
        {
            public class ByteArrayOffsetLength : ConvertBytesToStringTestBase
            {
                [Theory]
                [InlineData(typeof(ArgumentNullException), null, 0, 1)]
                [InlineData(typeof(ArgumentOutOfRangeException), new byte[0], -1, 1)]
                [InlineData(typeof(ArgumentOutOfRangeException), new byte[0], 0, -1)]
                [InlineData(typeof(ArgumentOutOfRangeException), new byte[0], 0, 1)]
                [InlineData(typeof(ArgumentOutOfRangeException),new byte[0], 1, 0)]
                public void InputOutputError(Type exception, byte[] inArray, int offset, int length)
                {
                    Assert.Throws(exception, () => Convert.ToHexString(inArray, offset, length));
                }

                [Fact]
                public void ProcessEmptyBytes()
                {
                    var result = Convert.ToHexString(Array.Empty<byte>(), 0, 0);
                    Assert.Same(string.Empty, result);
                }

                protected override void AcceptOptions(HexFormattingOptions options)
                {
                    Convert.ToHexString(Array.Empty<byte>(), 0, 0, options);
                }

                protected override string ProcessBytes(byte[] bytes, int expectedLength, HexFormattingOptions options)
                {
                    var result = Convert.ToHexString(bytes, 0, bytes.Length, options);
                    Assert.Equal(expectedLength, result.Length);
                    return result;
                }
            }
        }
    }
}