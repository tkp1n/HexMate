using System;
using System.Linq;
using HexMate.Tests.TestHelpers;
using Xunit;

namespace HexMate.Tests
{
    public unsafe class Utf16HexParserTests_Invalid
    {
        private readonly byte[] _binary;
        private readonly char[] _hexMixed;
        private readonly char[] _invalids;
        private readonly Random _random;

        public Utf16HexParserTests_Invalid()
        {
            _binary = TestData.Binary();
            _hexMixed = TestData.HexMixedUtf16(_binary);
            _invalids = Enumerable.Range(0, 512)
                .Where(x => x != 0x09 && x != 0x0A && x != 0x0D && x != 0x20) // whitespace
                .Where(x => x > 0x39 || x < 0x30) // digits
                .Where(x => x > 0x46 || x < 0x41) // a-f upper
                .Where(x => x > 0x66 || x < 0x61) // a-f lower
                .Select(x => (char) x)
                .ToArray();
            _random = new Random();
        }

#if INTRINSICS
        [Avx2Fact]
        public void TestInvalidAvx2()
        {
            var invalid = new char[_hexMixed.Length];
            var actual = new byte[_binary.Length];
            for (var i = 0; i < _invalids.Length; i++)
            {
                _hexMixed.CopyTo(invalid.AsSpan());
                var expectedPos = _random.Next(0, _hexMixed.Length);
                invalid[expectedPos] = _invalids[i];

                Pinner.Pin(invalid, actual, (input, output) =>
                {
                    var inPtr = input;
                    var outPtr = output;
                    var res = Utf16HexParser.Avx2.TryParse(ref inPtr, ref outPtr, _binary.Length);
                    Assert.False(res, $"Dig: {_invalids[i]:X}");
                    Assert.Equal(FastMath.RoundDownTo32(expectedPos), (int) (inPtr - input));
                    Assert.Equal(FastMath.RoundDownTo32(expectedPos / 2), (int) (outPtr - output));
                });
            }
        }

        [Sse41Fact]
        public void TestInvalidSse41()
        {
            var invalid = new char[_hexMixed.Length];
            var actual = new byte[_binary.Length];
            for (var i = 0; i < _invalids.Length; i++)
            {
                _hexMixed.CopyTo(invalid.AsSpan());
                var expectedPos = _random.Next(0, _hexMixed.Length);
                invalid[expectedPos] = _invalids[i];

                Pinner.Pin(invalid, actual, (input, output) =>
                {
                    var inPtr = input;
                    var outPtr = output;
                    var res = Utf16HexParser.Sse41.TryParse(ref inPtr, ref outPtr, _binary.Length);
                    Assert.False(res, $"Dig: {_invalids[i]:X}");
                    Assert.Equal(FastMath.RoundDownTo16(expectedPos), (int) (inPtr - input));
                    Assert.Equal(FastMath.RoundDownTo16(expectedPos / 2), (int) (outPtr - output));
                });
            }
        }

        [Ssse3Fact]
        public void TestInvalidSsse3()
        {
            var invalid = new char[_hexMixed.Length];
            var actual = new byte[_binary.Length];
            for (var i = 0; i < _invalids.Length; i++)
            {
                _hexMixed.CopyTo(invalid.AsSpan());
                var expectedPos = _random.Next(0, _hexMixed.Length);
                invalid[expectedPos] = _invalids[i];

                Pinner.Pin(invalid, actual, (input, output) =>
                {
                    var inPtr = input;
                    var outPtr = output;
                    var res = Utf16HexParser.Ssse3.TryParse(ref inPtr, ref outPtr, _binary.Length);
                    Assert.False(res, $"Dig: {_invalids[i]:X}");
                    Assert.Equal(FastMath.RoundDownTo16(expectedPos), (int) (inPtr - input));
                    Assert.Equal(FastMath.RoundDownTo16(expectedPos / 2), (int) (outPtr - output));
                });
            }
        }

        [Sse2Fact]
        public void TestInvalidSse2()
        {
            var invalid = new char[_hexMixed.Length];
            var actual = new byte[_binary.Length];
            for (var i = 0; i < _invalids.Length; i++)
            {
                _hexMixed.CopyTo(invalid.AsSpan());
                var expectedPos = _random.Next(0, _hexMixed.Length);
                invalid[expectedPos] = _invalids[i];

                Pinner.Pin(invalid, actual, (input, output) =>
                {
                    var inPtr = input;
                    var outPtr = output;
                    var res = Utf16HexParser.Sse2.TryParse(ref inPtr, ref outPtr, _binary.Length);
                    Assert.False(res, $"Dig: {_invalids[i]:X}");
                    Assert.Equal(FastMath.RoundDownTo16(expectedPos), (int) (inPtr - input));
                    Assert.Equal(FastMath.RoundDownTo16(expectedPos / 2), (int) (outPtr - output));
                });
            }
        }
#endif
#if SPAN
        [Fact]
        public void TestInvalidSpan()
        {
            var invalid = new char[_hexMixed.Length];
            var actual = new byte[_binary.Length];
            for (var i = 0; i < _invalids.Length; i++)
            {
                _hexMixed.CopyTo(invalid.AsSpan());
                var expectedPos = _random.Next(0, _hexMixed.Length);
                invalid[expectedPos] = _invalids[i];

                Pinner.Pin(invalid, actual, (input, output) =>
                {
                    var inPtr = input;
                    var outPtr = output;
                    var res = Utf16HexParser.Span.TryParse(ref inPtr, _hexMixed.Length,  ref outPtr, _binary.Length);
                    Assert.False(res, $"Dig: {_invalids[i]:X}");
                    Assert.Equal(expectedPos, (int) (inPtr - input));
                    Assert.Equal(expectedPos / 2, (int) (outPtr - output));
                });
            }
        }
#endif
        [Fact]
        public void TestInvalidFixed()
        {
            var invalid = new char[_hexMixed.Length];
            var actual = new byte[_binary.Length];
            for (var i = 0; i < _invalids.Length; i++)
            {
                _hexMixed.CopyTo(invalid, 0);
                var expectedPos = _random.Next(0, _hexMixed.Length);
                invalid[expectedPos] = _invalids[i];

                Pinner.Pin(invalid, actual, (input, output) =>
                {
                    var inPtr = input;
                    var outPtr = output;
                    var res = Utf16HexParser.Fixed.TryParse(ref inPtr, _hexMixed.Length, ref outPtr, _binary.Length);
                    Assert.False(res, $"Dig: {_invalids[i].ToString()}");
                    Assert.Equal(expectedPos, (int) (inPtr - input));
                    Assert.Equal(expectedPos / 2, (int) (outPtr - output));
                });
            }
        }
    }
}
