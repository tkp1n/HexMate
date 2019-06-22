using System;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
        public static partial class ToHexString
        {
            public class ByteArray : ConvertBytesToStringTestBase
            {
                [Theory]
                [InlineData(typeof(ArgumentNullException), null)]
                public void InputOutputError(Type exception, byte[] inArray)
                {
                    Assert.Throws(exception, () => Convert.ToHexString(inArray));
                }

                [Fact]
                public void ProcessEmptyBytes()
                {
                    var result = Convert.ToHexString(Array.Empty<byte>());
                    Assert.Same(string.Empty, result);
                }

                protected override void AcceptOptions(HexFormattingOptions options)
                {
                    Convert.ToHexString(Array.Empty<byte>(), options);
                }

                protected override string ProcessBytes(byte[] bytes, int expectedLength, HexFormattingOptions options)
                {
                    var result = Convert.ToHexString(bytes, options);
                    Assert.Equal(expectedLength, result.Length);
                    return result;
                }
            }
        }
    }
}