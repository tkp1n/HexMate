using HexMate.Tests.TestHelpers;
using Xunit;

namespace HexMate.Tests
{
    public unsafe class Utf8HexParserTests_Valid
    {
        private readonly byte[] _binary;
        private readonly byte[] _hexMixed;

        public Utf8HexParserTests_Valid()
        {
            _binary = TestData.Binary();
            _hexMixed = TestData.HexMixedUtf8(_binary);
        }

#if INTRINSICS
        [Avx2Fact]
        public void TestValidAvx2()
        {
            var actual = new byte[_binary.Length];
            Pinner.Pin(_hexMixed, actual, (input, output) =>
            {
                var inPtr = input;
                var outPtr = output;
                var res = Utf8HexParser.Avx2.TryParse(ref inPtr, ref outPtr, _binary.Length);
                Assert.True(res);
                Assert.Equal(_hexMixed.Length, (int) (inPtr - input));
                Assert.Equal(_binary.Length, (int) (outPtr - output));
            });

            Assert.Equal(_binary, actual);
        }

        [Sse41Fact]
        public void TestValidSse41()
        {
            var actual = new byte[_binary.Length];
            Pinner.Pin(_hexMixed, actual, (input, output) =>
            {
                var inPtr = input;
                var outPtr = output;
                var res = Utf8HexParser.Sse41.TryParse(ref inPtr, ref outPtr, _binary.Length);
                Assert.True(res);
                Assert.Equal(_hexMixed.Length, (int) (inPtr - input));
                Assert.Equal(_binary.Length, (int) (outPtr - output));
            });

            Assert.Equal(_binary, actual);
        }

        [Ssse3Fact]
        public void TestValidSsse3()
        {
            var actual = new byte[_binary.Length];
            Pinner.Pin(_hexMixed, actual, (input, output) =>
            {
                var inPtr = input;
                var outPtr = output;
                var res = Utf8HexParser.Ssse3.TryParse(ref inPtr, ref outPtr, _binary.Length);
                Assert.True(res);
                Assert.Equal(_hexMixed.Length, (int) (inPtr - input));
                Assert.Equal(_binary.Length, (int) (outPtr - output));
            });

            Assert.Equal(_binary, actual);
        }

        [Sse2Fact]
        public void TestValidSse2()
        {
            var actual = new byte[_binary.Length];
            Pinner.Pin(_hexMixed, actual, (input, output) =>
            {
                var inPtr = input;
                var outPtr = output;
                var res = Utf8HexParser.Sse2.TryParse(ref inPtr, ref outPtr, _binary.Length);
                Assert.True(res);
                Assert.Equal(_hexMixed.Length, (int) (inPtr - input));
                Assert.Equal(_binary.Length, (int) (outPtr - output));
            });

            Assert.Equal(_binary, actual);
        }
#endif
#if SPAN
        [Fact]
        public void TestValidSpan()
        {
            var actual = new byte[_binary.Length];
            Pinner.Pin(_hexMixed, actual, (input, output) =>
            {
                var inPtr = input;
                var outPtr = output;
                var res = Utf8HexParser.Span.TryParse(ref inPtr, ref outPtr, _binary.Length);
                Assert.True(res);
                Assert.Equal(_hexMixed.Length, (int) (inPtr - input));
                Assert.Equal(_binary.Length, (int) (outPtr - output));
            });

            Assert.Equal(_binary, actual);
        }
#endif
        [Fact]
        public void TestValidFixed()
        {
            var actual = new byte[_binary.Length];
            Pinner.Pin(_hexMixed, actual, (input, output) =>
            {
                var inPtr = input;
                var outPtr = output;
                var res = Utf8HexParser.Fixed.TryParse(ref inPtr, ref outPtr, _binary.Length);
                Assert.True(res);
                Assert.Equal(_hexMixed.Length, (int) (inPtr - input));
                Assert.Equal(_binary.Length, (int) (outPtr - output));
            });

            Assert.Equal(_binary, actual);
        }
    }
}
