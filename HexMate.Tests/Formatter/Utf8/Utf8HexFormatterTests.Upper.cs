using System;
using System.Text;
using HexMate.Tests.TestHelpers;
using Xunit;

namespace HexMate.Tests
{
    public unsafe class Utf8HexFormatterTests_Upper
    {
        private readonly byte[] input;
        private readonly byte[] expected;

        public Utf8HexFormatterTests_Upper()
        {
            input = new byte[4096];
            var r = new Random();
            r.NextBytes(input);

            expected = Encoding.ASCII.GetBytes(BitConverter.ToString(input).Replace("-", string.Empty));
        }

        [Fact]
        public void TestFixed()
        {
            var output = new byte[input.Length * 2];
            var written = 0;
            Pinner.Pin(input, output, (i, o) =>
            {
                i += input.Length;
                o += output.Length;
                written = Utf8HexFormatter.Fixed.Format(ref i, ref o, input.Length);
            });

            Assert.Equal(expected, output);
            Assert.Equal(input.Length, written);
        }

#if INTRINSICS
        [Sse2Fact]
        public void TestSse2()
        {
            var output = new byte[input.Length * 2];
            var written = 0;
            Pinner.Pin(input, output, (i, o) =>
            {
                i += input.Length;
                o += output.Length;
                written = Utf8HexFormatter.Sse2.Format(ref i, ref o, input.Length);
            });

            Assert.Equal(expected, output);
            Assert.Equal(input.Length, written);
        }

        [Ssse3Fact]
        public void TestSsse3()
        {
            var output = new byte[input.Length * 2];
            var written = 0;
            Pinner.Pin(input, output, (i, o) =>
            {
                i += input.Length;
                o += output.Length;
                written = Utf8HexFormatter.Ssse3.Format(ref i, ref o, input.Length);
            });

            Assert.Equal(expected, output);
            Assert.Equal(input.Length, written);
        }

        [Avx2Fact]
        public void TestAvx2()
        {
            var output = new byte[input.Length * 2];
            var written = 0;
            Pinner.Pin(input, output, (i, o) =>
            {
                i += input.Length;
                o += output.Length;
                written = Utf8HexFormatter.Avx2.Format(ref i, ref o, input.Length);
            });

            Assert.Equal(expected, output);
            Assert.Equal(input.Length, written);
        }
#endif
    }
}