using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace HexMate.Benchmarks
{
    public unsafe class Utf8HexFormatterPerf
    {
        private byte[] _input;
        private byte[] _output;
        private byte* _inPtr;
        private byte* _outPtr;
        private GCHandle _inHandle;
        private GCHandle _outHandle;

        [Params(32, 2048)]
        public int DataSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _input = new byte[DataSize];
            new Random().NextBytes(_input);
            _output = new byte[DataSize * 2];

            _inHandle = GCHandle.Alloc(_input, GCHandleType.Pinned);
            _inPtr = (byte*) _inHandle.AddrOfPinnedObject() + DataSize;
            _outHandle = GCHandle.Alloc(_output, GCHandleType.Pinned);
            _outPtr = (byte*) _outHandle.AddrOfPinnedObject() + DataSize * 2;
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
            Utf8HexFormatter.Avx2.Format(ref inBytes, ref outBytes, DataSize);
        }

        [Benchmark]
        public void FormatSsse3()
        {
            var inBytes = _inPtr;
            var outBytes = _outPtr;
            Utf8HexFormatter.Ssse3.Format(ref inBytes, ref outBytes, DataSize);
        }

        [Benchmark]
        public void FormatSse2()
        {
            var inBytes = _inPtr;
            var outBytes = _outPtr;
            Utf8HexFormatter.Sse2.Format(ref inBytes, ref outBytes, DataSize);
        }
#endif
        [Benchmark]
        public void FormatFixed()
        {
            var inBytes = _inPtr;
            var outBytes = _outPtr;
            Utf8HexFormatter.Fixed.Format(ref inBytes, ref outBytes, DataSize);
        }
    }
}