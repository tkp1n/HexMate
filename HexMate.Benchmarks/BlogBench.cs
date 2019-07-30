using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace HexMate.Benchmarks
{
    [MemoryDiagnoser]
    public unsafe class BlogBench
    {
        private byte[] data;
        
        [Params("LargeText", "SmallText", "")] 
        public string TestFile { get; set; }
        
        [GlobalSetup]
        public void Setup()
        {
            if (TestFile == "")
            {
                data = Encoding.ASCII.GetBytes("put any sample string you want to test here instead of a file");
                return;
            }
            
            var testFileName = TestFile + ".txt";
            
            var testSubjectFile = new FileInfo(testFileName);
            using (var testSubjectStream = testSubjectFile.OpenRead()) {
                using (var testSubjectReader = new StreamReader(testSubjectStream)) {
                    data = Encoding.ASCII.GetBytes(testSubjectReader.ReadToEnd());
                }
            }
        }

        [Benchmark]
        public string HexMate()
        {
            var bytes = data;
            return Convert.ToHexString(bytes, 0, bytes.Length, HexFormattingOptions.None);
        }

        //[Benchmark]
        public string ByteArrayToHexStringViaStringJoinArrayConvertAll() 
            => string.Join(string.Empty, Array.ConvertAll(data, b => b.ToString("X2")));
        
        //[Benchmark]
        public string ByteArrayToHexStringViaStringConcatArrayConvertAll() 
            => string.Concat(Array.ConvertAll(data, b => b.ToString("X2")));
        
        //[Benchmark]
        public string ByteArrayToHexStringViaBitConverter() 
            => BitConverter.ToString(data).Replace("-", "");
        
        //[Benchmark]
        public string ByteArrayToHexStringViaStringBuilderAggregateByteToString()
        {
            var bytes = data;
            return bytes.Aggregate(new StringBuilder(bytes.Length * 2), (sb, b) => sb.Append(b.ToString("X2")))
                .ToString();
        }

        //[Benchmark]
        public string ByteArrayToHexStringViaStringBuilderForEachByteToString()
        {
            var bytes = data;
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                hex.Append(b.ToString("X2"));
            return hex.ToString();
        }
        
        //[Benchmark]
        public string ByteArrayToHexStringViaStringBuilderAggregateAppendFormat()
        {
            var bytes = data;
            return bytes.Aggregate(new StringBuilder(bytes.Length * 2), (sb, b) => sb.AppendFormat("{0:X2}", b))
                .ToString();
        }

        //[Benchmark]
        public string ByteArrayToHexStringViaStringBuilderForEachAppendFormat()
        {
            var bytes = data;
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }
        
        //[Benchmark]
        public string ByteArrayToHexViaByteManipulation()
        {
            var bytes = data;
            var c = new char[bytes.Length * 2];
            byte b;
            for (var i = 0; i < bytes.Length; i++) {
                b = ((byte)(bytes[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(bytes[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new string(c);
        }
        
        //[Benchmark]
        public string ByteArrayToHexViaByteManipulation2()
        {
            var bytes = data;
            var c = new char[bytes.Length * 2];
            int b;
            for (var i = 0; i < bytes.Length; i++) {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }
        
#if !NETCOREAPP
        [Benchmark]
        public string ByteArrayToHexViaSoapHexBinary(byte[] bytes) {
            SoapHexBinary soapHexBinary = new SoapHexBinary(bytes);
            return soapHexBinary.ToString();
        }
#endif

        const string hexAlphabet = "0123456789ABCDEF";
        //[Benchmark]
        public string ByteArrayToHexViaLookupAndShift()
        {
            var bytes = data;
            var result = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) {
                result.Append(hexAlphabet[(int)(b >> 4)]);
                result.Append(hexAlphabet[(int)(b & 0xF)]);
            }
            return result.ToString();
        }
        
        static uint[] _Lookup32 = Enumerable.Range(0, 256).Select(i => {
            string s = i.ToString("X2");
            return ((uint)s[0]) + ((uint)s[1] << 16);
        }).ToArray();
        //[Benchmark]
        public string ByteArrayToHexViaLookupPerByte()
        {
            var bytes = data;
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++) {
                var val = _Lookup32[bytes[i]];
                result[2*i] = (char)val;
                result[2*i + 1] = (char) (val >> 16);
            }
            return new string(result);
        }
        
        // { "00", "01", ..., "0E", "0F", "10", "11", ..., "FE", "FF" }
        static readonly string[] hexStringTable = hexAlphabet.SelectMany(n1 => hexAlphabet.Select(n2 => new string(new[] { n1, n2 }))).ToArray();
        //[Benchmark]
        public string ByteArrayToHexViaLookup()
        {
            var bytes = data;
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes) {
                result.Append(hexStringTable[b]);
            }
            return result.ToString();
        }
        
        static readonly uint* _lookup32UnsafeP = (uint*)GCHandle.Alloc(_Lookup32, GCHandleType.Pinned).AddrOfPinnedObject();
        [Benchmark]
        public string ByteArrayToHexViaLookup32UnsafeDirect()
        {
            var bytes = data;
            var lookupP = _lookup32UnsafeP;
            var result = new string((char)0, bytes.Length * 2);
            fixed (byte* bytesP = bytes)
            fixed (char* resultP = result) {
                uint* resultP2 = (uint*)resultP;
                for (int i = 0; i < bytes.Length; i++) {
                    resultP2[i] = lookupP[bytesP[i]];
                }
            }

            return result;
        }
    }
}