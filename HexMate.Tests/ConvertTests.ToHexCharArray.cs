using System;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
        public class ToHexCharArray : ConvertBytesToStringTestBase
        {
            [Theory]
            [InlineData(typeof(ArgumentNullException), null, 0, 1, new char[0], 0)]
            [InlineData(typeof(ArgumentOutOfRangeException), new byte[0], -1, 1, new char[0], 0)]
            [InlineData(typeof(ArgumentOutOfRangeException), new byte[0], 0, -1, new char[0], 0)]
            [InlineData(typeof(ArgumentOutOfRangeException), new byte[0], 0, 1, new char[0], 0)]
            [InlineData(typeof(ArgumentOutOfRangeException),new byte[0], 1, 0, new char[0], 0)]
            [InlineData(typeof(ArgumentNullException),new byte[0], 0, 0, null, 0)]
            [InlineData(typeof(ArgumentOutOfRangeException),new byte[0], 0, 0, new char[0], -1)]
            [InlineData(typeof(ArgumentOutOfRangeException),new byte[0], 0, 0, new char[0], 1)]
            public void InputOutputError(Type exception, byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut)
            {
                Assert.Throws(exception, () => Convert.ToHexCharArray(inArray, offsetIn, length, outArray, offsetOut));
            }

            protected override void AcceptOptions(HexFormattingOptions options)
            {
                Convert.ToHexCharArray(Array.Empty<byte>(), 0, 0, Array.Empty<char>(), 0, options);
            }

            protected override string ProcessBytes(byte[] bytes, int expectedLength, HexFormattingOptions options)
            {
                var resultLength = _random.Next(expectedLength + 3, expectedLength + 10);
                var resultOffset = _random.Next(0, resultLength - expectedLength);

                var result = new char[resultLength];
                var written = Convert.ToHexCharArray(bytes, 0, bytes.Length, result, resultOffset, options);
                Assert.Equal(expectedLength, written);

                return new string(result, resultOffset, written);
            }
        }
    }
}