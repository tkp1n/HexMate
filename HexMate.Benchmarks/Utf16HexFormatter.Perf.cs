using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace HexMate.Benchmarks
{
    public unsafe class Utf16HexFormatterPerf
    {
        private byte[] _input;
        private char[] _output;
        private byte* _inPtr;
        private char* _outPtr;
        private GCHandle _inHandle;
        private GCHandle _outHandle;

        [Params(32, 2048)]
        public int DataSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _input = new byte[DataSize];
            new Random().NextBytes(_input);
            _output = new char[DataSize * 2];

            _inHandle = GCHandle.Alloc(_input, GCHandleType.Pinned);
            _inPtr = (byte*) _inHandle.AddrOfPinnedObject() + DataSize;
            _outHandle = GCHandle.Alloc(_output, GCHandleType.Pinned);
            _outPtr = (char*) _outHandle.AddrOfPinnedObject() + DataSize * 2;
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _inHandle.Free();
            _outHandle.Free();
        }

#if INTRINSICS
        [Benchmark]
        public void FormatAvx2()
        {
            var inBytes = _inPtr;
            var outBytes = _outPtr;
            Utf16HexFormatter.Avx2.Format(ref inBytes, ref outBytes, DataSize);
        }

        [Benchmark]
        public void FormatSsse3()
        {
            var inBytes = _inPtr;
            var outBytes = _outPtr;
            Utf16HexFormatter.Ssse3.Format(ref inBytes, ref outBytes, DataSize);
        }

        [Benchmark]
        public void FormatSse2()
        {
            var inBytes = _inPtr;
            var outBytes = _outPtr;
            Utf16HexFormatter.Sse2.Format(ref inBytes, ref outBytes, DataSize);
        }
#endif
        [Benchmark]
        public void FormatFixed()
        {
            var inBytes = _inPtr;
            var outBytes = _outPtr;
            Utf16HexFormatter.Fixed.Format(ref inBytes, ref outBytes, DataSize);
        }
    }
}