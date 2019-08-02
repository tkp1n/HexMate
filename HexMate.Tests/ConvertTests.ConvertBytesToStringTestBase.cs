using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
        public abstract class ConvertBytesToStringTestBase
        {
            protected readonly Random _random = new Random();
            public static IEnumerable<object[]> GetEncodingInput()
            {
                var lengths = new[] { 0, 1, 31, 32, 34, 63, 64, 65, 71, 72, 73 };
                var options = new[] { 0, 1, 2, 3 };

                foreach (var length in lengths)
                {
                    foreach (var option in options)
                    {
                        yield return new object[]
                        {
                            length,
                            (HexFormattingOptions) option
                        };
                    }
                }
            }

            [Theory]
            [InlineData(-1)]
            [InlineData(4)]
            public void TestOptionValidation(int options)
            {
                Assert.Throws<ArgumentException>(nameof(options), () => AcceptOptions((HexFormattingOptions)options));
            }

            [Theory]
            [MemberData(nameof(GetEncodingInput))]
            public void Encode(int length, HexFormattingOptions options)
            {
                var inArray = new byte[length];
                _random.NextBytes(inArray);

                var expected = EncodeSlow(inArray, options);
                var result = ProcessBytes(inArray, expected.Length, options);

                Assert.Equal(expected, result);
            }

            protected abstract void AcceptOptions(HexFormattingOptions options);

            protected abstract string ProcessBytes(byte[] bytes, int expectedLength, HexFormattingOptions options);

            private static string EncodeSlow(byte[] input, HexFormattingOptions options)
            {
                const int insertLineBreaksEvery = 72;
                var result = BitConverter.ToString(input).Replace("-", string.Empty);

                if (options.HasFlag(HexFormattingOptions.InsertLineBreaks) && result.Length > insertLineBreaksEvery)
                {
                    var newResult = new StringBuilder();
                    for (var i = 0; i < result.Length; i += insertLineBreaksEvery)
                    {
                        newResult.Append(result.Substring(i, Math.Min(insertLineBreaksEvery, result.Length - i)));
                        newResult.Append("\r\n"); // Convert does not add platform-specific new-lines
                    }

                    result = newResult.ToString().TrimEnd();
                }

                if (options.HasFlag(HexFormattingOptions.Lowercase))
                {
                    return result.ToLower();
                }

                return result;
            }
        }
    }
}