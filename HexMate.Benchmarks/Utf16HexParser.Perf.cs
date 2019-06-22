using System;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace HexMate.Benchmarks
{
    public unsafe class Utf16HexParser_Perf
    {
        private char[] _input;
        private byte[] _output;
        private char* _inPtr;
        private byte* _outPtr;
        private GCHandle _inHandle;
        private GCHandle _outHandle;

        [Params(32, 2048)]
        public int DataSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _output = new byte[DataSize];
            new Random().NextBytes(_output);
            _input = BitConverter.ToString(_output).Replace("-", string.Empty).ToCharArray();

            _inHandle = GCHandle.Alloc(_input, GCHandleType.Pinned);
            _inPtr = (char*) _inHandle.AddrOfPinnedObject();
            _outHandle = GCHandle.Alloc(_output, GCHandleType.Pinned);
            _outPtr = (byte*) _outHandle.AddrOfPinnedObject();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _inHandle.Free();
            _outHandle.Free();
        }

#if INTRINSICS
        [Benchmark]
        public void ParseAvx2()
        {
            var srcBytes = _inPtr;
            var destBytes = _outPtr;
            Utf16HexParser.Avx2.TryParse(ref srcBytes, ref destBytes, DataSize);
        }

        [Benchmark]
        public void ParseSse41()
        {
            var srcBytes = _inPtr;
            var destBytes = _outPtr;
            Utf16HexParser.Sse41.TryParse(ref srcBytes, ref destBytes, DataSize);
        }

        [Benchmark]
        public void ParseSsse3()
        {
            var srcBytes = _inPtr;
            var destBytes = _outPtr;
            Utf16HexParser.Ssse3.TryParse(ref srcBytes, ref destBytes, DataSize);
        }

        [Benchmark]
        public void ParseSse2()
        {
            var srcBytes = _inPtr;
            var destBytes = _outPtr;
            Utf16HexParser.Sse2.TryParse(ref srcBytes, ref destBytes, DataSize);
        }
#endif
#if SPAN
        [Benchmark]
        public void ParseSpan()
        {
            var srcBytes = _inPtr;
            var destBytes = _outPtr;
            Utf16HexParser.Span.TryParse(ref srcBytes, DataSize, ref destBytes, DataSize);
        }
#endif
        [Benchmark]
        public void ParseFixed()
        {
            var srcBytes = _inPtr;
            var destBytes = _outPtr;
            Utf16HexParser.Fixed.TryParse(ref srcBytes, DataSize, ref destBytes, DataSize);
        }
    }
}