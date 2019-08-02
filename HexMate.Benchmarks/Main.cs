using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace HexMate.Benchmarks
{
    [Config(typeof(ConfigWithCustomEnvVars))]
    public class Main
    {
        private byte[] _inBytes;
        private byte[] _outBytes;
        private char[] _outChars;

        private class ConfigWithCustomEnvVars : ManualConfig
        {
            private const string EnableAVX2 = "COMPlus_EnableAVX2";
            private const string EnableSSE41 = "COMPlus_EnableSSE41";
            private const string EnableSSSE3 = "COMPlus_EnableSSSE3";
            private const string EnableSSE2 = "COMPlus_EnableSSE2";

            private static readonly EnvironmentVariable[] AllEnabled =
            {
                new EnvironmentVariable(EnableAVX2, "1"),
                new EnvironmentVariable(EnableSSE41, "1"),
                new EnvironmentVariable(EnableSSSE3, "1"),
                new EnvironmentVariable(EnableSSE2, "1"),
            };

            private static readonly EnvironmentVariable[] NoAvx2 =
            {
                new EnvironmentVariable(EnableAVX2, "0"),
                new EnvironmentVariable(EnableSSE41, "1"),
                new EnvironmentVariable(EnableSSSE3, "1"),
                new EnvironmentVariable(EnableSSE2, "1"),
            };

            private static readonly EnvironmentVariable[] NoSse41 =
            {
                new EnvironmentVariable(EnableAVX2, "0"),
                new EnvironmentVariable(EnableSSE41, "0"),
                new EnvironmentVariable(EnableSSSE3, "1"),
                new EnvironmentVariable(EnableSSE2, "1"),
            };

            private static readonly EnvironmentVariable[] NoSsse3 =
            {
                new EnvironmentVariable(EnableAVX2, "0"),
                new EnvironmentVariable(EnableSSE41, "0"),
                new EnvironmentVariable(EnableSSSE3, "0"),
                new EnvironmentVariable(EnableSSE2, "1"),
            };

            private static readonly EnvironmentVariable[] NoSse2 =
            {
                new EnvironmentVariable(EnableAVX2, "0"),
                new EnvironmentVariable(EnableSSE41, "0"),
                new EnvironmentVariable(EnableSSSE3, "0"),
                new EnvironmentVariable(EnableSSE2, "0"),
            };

            public ConfigWithCustomEnvVars()
            {
                Add(Job.Core
                    .With(AllEnabled)
                    .WithId("AVX2"));
                Add(Job.Core
                    .With(NoAvx2)
                    .WithId("SSE4.1"));
                Add(Job.Core
                    .With(NoSse41)
                    .WithId("SSSE3"));
                Add(Job.Core
                    .With(NoSsse3)
                    .WithId("SSE2"));
                Add(Job.Core
                    .With(NoSse2)
                    .WithId("Scalar"));
            }
        }

        [Params(32, 2048)]
        public int DataSize { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _inBytes = new byte[DataSize];
            new Random().NextBytes(_inBytes);
            _outBytes = Encoding.ASCII.GetBytes(BitConverter.ToString(_inBytes).Replace("-", string.Empty).Substring(0, DataSize));

            _inBytes = new byte[DataSize];
            new Random().NextBytes(_inBytes);
            _outChars = BitConverter.ToString(_inBytes).Replace("-", string.Empty).Substring(0, DataSize).ToCharArray();
        }

#if SPAN
        [Benchmark]
        public void EncodeUtf8()
            => Hex.EncodeToUtf8(_inBytes, _outBytes , out _, out _);

        [Benchmark]
        public void DecodeUtf8()
            => Hex.DecodeFromUtf8(_outBytes, _inBytes , out _, out _);

        [Benchmark]
        public void EncodeUtf16()
            => Convert.TryToHexChars(_inBytes, _outChars, out _);

        [Benchmark]
        public void DecodeUtf16()
            => Convert.TryFromHexChars(_outChars, _inBytes, out _);
#endif
    }
}