using System;
using HexMate.Tests.TestHelpers;
using Xunit;

namespace HexMate.Tests
{
    public unsafe class Utf16HexFormatterTests_Lower
    {
        private readonly byte[] input;
        private readonly char[] expected;

        public Utf16HexFormatterTests_Lower()
        {
            input = new byte[4096];
            var r = new Random();
            r.NextBytes(input);

            expected = BitConverter.ToString(input).Replace("-", string.Empty).ToLowerInvariant().ToCharArray();
        }

        [Fact]
        public void TestFixed()
        {
            var output = new char[input.Length * 2];
            var written = 0;
            Pinner.Pin(input, output, (i, o) =>
            {
                i += input.Length;
                o += output.Length;
                written = Utf16HexFormatter.Fixed.Format(ref i, ref o, input.Length, toLower: true);
            });

            Assert.Equal(expected, output);
            Assert.Equal(input.Length, written);
        }

#if INTRINSICS
        [Sse2Fact]
        public void TestSse2()
        {
            var output = new char[input.Length * 2];
            var written = 0;
            Pinner.Pin(input, output, (i, o) =>
            {
                i += input.Length;
                o += output.Length;
                written = Utf16HexFormatter.Sse2.Format(ref i, ref o, input.Length, toLower: true);
            });

            Assert.Equal(expected, output);
            Assert.Equal(input.Length, written);
        }

        [Ssse3Fact]
        public void TestSsse3()
        {
            var output = new char[input.Length * 2];
            var written = 0;
            Pinner.Pin(input, output, (i, o) =>
            {
                i += input.Length;
                o += output.Length;
                written = Utf16HexFormatter.Ssse3.Format(ref i, ref o, input.Length, toLower: true);
            });

            Assert.Equal(expected, output);
            Assert.Equal(input.Length, written);
        }

        [Avx2Fact]
        public void TestAvx2()
        {
            var output = new char[input.Length * 2];
            var written = 0;
            Pinner.Pin(input, output, (i, o) =>
            {
                i += input.Length;
                o += output.Length;
                written = Utf16HexFormatter.Avx2.Format(ref i, ref o, input.Length, toLower: true);
            });

            Assert.Equal(expected, output);
            Assert.Equal(input.Length, written);
        }
#endif
    }
}