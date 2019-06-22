using System;
using System.Linq;
using Xunit;

namespace HexMate.Tests
{
    public static partial class ConvertTests
    {
        public abstract class ConvertStringToBytesTestBase
        {
            [Theory]
            [InlineData("0")]
            [InlineData(" 9")]
            [InlineData("   A")]
            [InlineData("F ")]
            [InlineData("0   ")]
            public void SingleCharacter(string input)
            {
                Assert.Throws<FormatException>(() =>
                {
                    ProcessString(input, CountUseful(input));
                });
            }

            [Theory]
            [InlineData("010203040")]
            [InlineData(" 010203040")]
            [InlineData("0 10203040")]
            [InlineData("0102 03040")]
            [InlineData("0102   03040")]
            [InlineData("01020   3040")]
            [InlineData("01020304 0")]
            [InlineData("010203040 ")]
            public void OddNofCharacters(string input)
            {
                Assert.Throws<FormatException>(() =>
                {
                    ProcessString(input, CountUseful(input));
                });
            }

            protected const string scalarNormal = "01020304";
            protected const string sseNormal = "01020304010203040102030401020304";
            protected const string avxNormal = "0102030401020304010203040102030401020304010203040102030401020304";

            [Theory]
            // Scalar sized cases
            [InlineData("")]
            [InlineData("01020304")]
            [InlineData(" 01020304")]
            [InlineData("   01020304")]
            [InlineData("0102 0304")]
            [InlineData("010  20304")]
            [InlineData("0102  0304")]
            [InlineData("01020304 ")]
            [InlineData("01020304   ")]
            [InlineData("0 1\t0\r2\n0 3 0 4")]

            // SSE sized cases
            [InlineData("01020304010203040102030401020304")]
            [InlineData(" 01020304010203040102030401020304")]
            [InlineData("   01020304010203040102030401020304")]
            [InlineData("0102 0304010203040102030401020304")]
            [InlineData("0102   0304010203040102030401020304")]
            [InlineData("01020   304010203040102030401020304")]
            [InlineData("01020304010203040102030401020304 ")]
            [InlineData("01020304010203040102030401020304   ")]
            [InlineData("0 1\t0\r2\n0304010203040102030401020304")]

            // AVX sized cases
            [InlineData("0102030401020304010203040102030401020304010203040102030401020304")]
            [InlineData(" 0102030401020304010203040102030401020304010203040102030401020304")]
            [InlineData("   0102030401020304010203040102030401020304010203040102030401020304")]
            [InlineData("0102 030401020304010203040102030401020304010203040102030401020304")]
            [InlineData("0102   030401020304010203040102030401020304010203040102030401020304")]
            [InlineData("01020   30401020304010203040102030401020304010203040102030401020304")]
            [InlineData("0102030401020304010203040102030401020304010203040102030401020304 ")]
            [InlineData("0102030401020304010203040102030401020304010203040102030401020304   ")]
            [InlineData("0 1\t0\r2\n030401020304010203040102030401020304010203040102030401020304")]

            // SSE prefix
            [InlineData(sseNormal + "01020304")]
            [InlineData(sseNormal + " 01020304")]
            [InlineData(sseNormal + "   01020304")]
            [InlineData(sseNormal + "0102 0304")]
            [InlineData(sseNormal + "010  20304")]
            [InlineData(sseNormal + "0102  0304")]
            [InlineData(sseNormal + "01020304 ")]
            [InlineData(sseNormal + "01020304   ")]
            [InlineData(sseNormal + "0 1\t0\r2\n0 3 0 4")]

            // AVX prefix
            [InlineData(avxNormal + "01020304")]
            [InlineData(avxNormal + " 01020304")]
            [InlineData(avxNormal + "   01020304")]
            [InlineData(avxNormal + "0102 0304")]
            [InlineData(avxNormal + "010  20304")]
            [InlineData(avxNormal + "0102  0304")]
            [InlineData(avxNormal + "01020304 ")]
            [InlineData(avxNormal + "01020304   ")]
            [InlineData(avxNormal + "0 1\t0\r2\n0 3 0 4")]

            // SSE sized cases + scalar suffix
            [InlineData(" 01020304010203040102030401020304" + scalarNormal)]
            [InlineData("   01020304010203040102030401020304" + scalarNormal)]
            [InlineData("0102 0304010203040102030401020304" + scalarNormal)]
            [InlineData("0102   0304010203040102030401020304" + scalarNormal)]
            [InlineData("01020   304010203040102030401020304" + scalarNormal)]
            [InlineData("0 1\t0\r2\n0304010203040102030401020304" + scalarNormal)]

            // AVX sized cases + scalar suffix
            [InlineData(" 0102030401020304010203040102030401020304010203040102030401020304" + scalarNormal)]
            [InlineData("   0102030401020304010203040102030401020304010203040102030401020304" + scalarNormal)]
            [InlineData("0102 030401020304010203040102030401020304010203040102030401020304" + scalarNormal)]
            [InlineData("0102   030401020304010203040102030401020304010203040102030401020304" + scalarNormal)]
            [InlineData("01020   30401020304010203040102030401020304010203040102030401020304" + scalarNormal)]
            [InlineData("0 1\t0\r2\n030401020304010203040102030401020304010203040102030401020304" + scalarNormal)]
            public void NormalCases(string input)
            {
                var expected = ParseSlow(input);
                var result = ProcessString(input, CountUseful(input));

                Assert.Equal(expected, result);
            }

            [Theory]
            [InlineData("010203xy")]
            [InlineData(sseNormal + "010203xy")]
            [InlineData(avxNormal + "010203xy")]
            [InlineData("010203040102030401020304010203xy")]
            [InlineData("01020304010203040102030401020304010203040102030401020304010203xy")]
            public void ErrorCases(string input)
            {
                Assert.Throws<FormatException>(() => ProcessString(input, CountUseful(input)));
            }

            protected abstract byte[] ProcessString(string s, int resultingBytes);

            protected static int CountUseful(string s)
                => s.Count(x => (x >= '0' && x <= '9') || (x >= 'A' && x <= 'F') || (x >= 'a' && x <= 'f')) / 2;

            private static byte[] ParseSlow(string s)
            {
                var noWhitespace =  s.Replace(" ", string.Empty)
                    .Replace("\t", string.Empty)
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty);

                var result = new byte[noWhitespace.Length / 2];
                int pos = 0;
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = System.Convert.ToByte(noWhitespace.Substring(pos, 2), 16);
                    pos += 2;
                }

                return result;
            }
        }
    }
}