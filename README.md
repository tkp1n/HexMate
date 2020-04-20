# Hex <--> Binary 

## Implementation of API Proposal for [dotnet/runtime#17837](https://github.com/dotnet/runtime/issues/17837)

### API Proposal

```csharp
namespace System
{
    // NEW type
    [Flags]
    public enum HexFormattingOptions
    {
        None = 0,
        InsertLineBreaks = 1,
        Lowercase = 2
    }

    // NEW methods on EXISTING type
    public static class Convert
    {
        // Decode from chars
        public static byte[] FromHexCharArray(char[] inArray, int offset, int length) => throw null;
        public static bool TryFromHexChars(ReadOnlySpan<char> chars, Span<byte> bytes, out int bytesWritten) => throw null;

        // Decode from strings
        public static byte[] FromHexString(string s) => throw null;
        public static byte[] FromHexString(ReadOnlySpan<char> chars) => throw null;
        public static bool TryFromHexString(string s, Span<byte> bytes, out int bytesWritten) => throw null;

        // Encode to chars
        public static int ToHexCharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut, HexFormattingOptions options = default) => throw null;
        public static bool TryToHexChars(ReadOnlySpan<byte> bytes, Span<char> chars, out int charsWritten, HexFormattingOptions options = default) => throw null;

        // Encode to strings
        public static string ToHexString(byte[] inArray, HexFormattingOptions options = default) => throw null;
        public static string ToHexString(byte[] inArray, int offset, int length, HexFormattingOptions options = default) => throw null;
        public static string ToHexString(ReadOnlySpan<byte> bytes, HexFormattingOptions options = default) => throw null;
    }
}

namespace System.Buffers.Text
{
    // NEW type
    public static class Hex
    {
        // Decode
        public static OperationStatus DecodeFromUtf8(ReadOnlySpan<byte> utf8, Span<byte> bytes, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true) => throw null;
        public static OperationStatus DecodeFromUtf8InPlace(Span<byte> buffer, out int bytesWritten) => throw null;
        
        // Encode
        public static OperationStatus EncodeToUtf8(ReadOnlySpan<byte> bytes, Span<byte> utf8, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true) => throw null;
        public static OperationStatus EncodeToUtf8InPlace(Span<byte> buffer, int dataLength, out int bytesWritten) => throw null;
    }
}
```

### Performance characteristics

```
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19041.207 (2004/?/20H1)
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.100-preview.2.20176.6
```

| Method      | Job    | DataSize | Mean         | Error      | StdDev     | 
|-------------|--------|----------|--------------|------------|------------| 
| DecodeUtf16 | AVX2   | 32       | 18.535 ns    | 0.0736 ns  | 0.0615 ns  | 
| DecodeUtf16 | SSE2   | 32       | 28.495 ns    | 0.5772 ns  | 0.6647 ns  | 
| DecodeUtf16 | SSE4.1 | 32       | 22.267 ns    | 0.4771 ns  | 0.7568 ns  | 
| DecodeUtf16 | SSSE3  | 32       | 22.629 ns    | 0.1030 ns  | 0.0860 ns  | 
| DecodeUtf16 | Scalar | 32       | 36.456 ns    | 0.2476 ns  | 0.2316 ns  | 
|             |        |          |              |            |            | 
| DecodeUtf16 | AVX2   | 2048     | 147.228 ns   | 0.7159 ns  | 0.5978 ns  | 
| DecodeUtf16 | SSE2   | 2048     | 413.381 ns   | 1.5861 ns  | 1.4836 ns  | 
| DecodeUtf16 | SSE4.1 | 2048     | 295.134 ns   | 5.8257 ns  | 5.1644 ns  | 
| DecodeUtf16 | SSSE3  | 2048     | 319.053 ns   | 2.6398 ns  | 2.3401 ns  | 
| DecodeUtf16 | Scalar | 2048     | 1,913.189 ns | 11.4772 ns | 10.7358 ns |
|             |        |          |              |            |            |  
| DecodeUtf8  | AVX2   | 32       | 14.601 ns    | 0.1075 ns  | 0.0897 ns  | 
| DecodeUtf8  | SSE2   | 32       | 17.503 ns    | 0.3222 ns  | 0.3013 ns  | 
| DecodeUtf8  | SSE4.1 | 32       | 13.611 ns    | 0.0723 ns  | 0.0641 ns  | 
| DecodeUtf8  | SSSE3  | 32       | 13.762 ns    | 0.2694 ns  | 0.2520 ns  | 
| DecodeUtf8  | Scalar | 32       | 22.227 ns    | 0.1324 ns  | 0.1239 ns  |
|             |        |          |              |            |            |  
| DecodeUtf8  | AVX2   | 2048     | 112.673 ns   | 0.5130 ns  | 0.4799 ns  | 
| DecodeUtf8  | SSE2   | 2048     | 371.773 ns   | 1.4891 ns  | 1.2434 ns  | 
| DecodeUtf8  | SSE4.1 | 2048     | 247.388 ns   | 2.9746 ns  | 2.4839 ns  | 
| DecodeUtf8  | SSSE3  | 2048     | 278.068 ns   | 1.3503 ns  | 1.1276 ns  | 
| DecodeUtf8  | Scalar | 2048     | 1,197.137 ns | 4.7857 ns  | 4.4765 ns  |
|             |        |          |              |            |            |  
| EncodeUtf16 | AVX2   | 32       | 5.391 ns     | 0.0482 ns  | 0.0403 ns  | 
| EncodeUtf16 | SSE2   | 32       | 5.656 ns     | 0.0372 ns  | 0.0348 ns  | 
| EncodeUtf16 | SSE4.1 | 32       | 5.553 ns     | 0.0556 ns  | 0.0464 ns  | 
| EncodeUtf16 | SSSE3  | 32       | 5.608 ns     | 0.1093 ns  | 0.1301 ns  | 
| EncodeUtf16 | Scalar | 32       | 5.675 ns     | 0.0409 ns  | 0.0363 ns  |
|             |        |          |              |            |            |  
| EncodeUtf16 | AVX2   | 2048     | 5.403 ns     | 0.0431 ns  | 0.0337 ns  | 
| EncodeUtf16 | SSE2   | 2048     | 5.500 ns     | 0.0299 ns  | 0.0279 ns  | 
| EncodeUtf16 | SSE4.1 | 2048     | 5.300 ns     | 0.0326 ns  | 0.0305 ns  | 
| EncodeUtf16 | SSSE3  | 2048     | 5.313 ns     | 0.0171 ns  | 0.0151 ns  | 
| EncodeUtf16 | Scalar | 2048     | 5.372 ns     | 0.0287 ns  | 0.0254 ns  |
|             |        |          |              |            |            |  
| EncodeUtf8  | AVX2   | 32       | 8.222 ns     | 0.0639 ns  | 0.0598 ns  | 
| EncodeUtf8  | SSE2   | 32       | 9.779 ns     | 0.0476 ns  | 0.0397 ns  | 
| EncodeUtf8  | SSE4.1 | 32       | 7.681 ns     | 0.0730 ns  | 0.0683 ns  | 
| EncodeUtf8  | SSSE3  | 32       | 7.763 ns     | 0.0984 ns  | 0.0821 ns  | 
| EncodeUtf8  | Scalar | 32       | 23.286 ns    | 0.1478 ns  | 0.1234 ns  |
|             |        |          |              |            |            |  
| EncodeUtf8  | AVX2   | 2048     | 50.853 ns    | 0.4674 ns  | 0.3903 ns  | 
| EncodeUtf8  | SSE2   | 2048     | 98.637 ns    | 0.5082 ns  | 0.4505 ns  | 
| EncodeUtf8  | SSE4.1 | 2048     | 65.998 ns    | 0.3181 ns  | 0.2820 ns  | 
| EncodeUtf8  | SSSE3  | 2048     | 73.602 ns    | 0.4883 ns  | 0.4329 ns  | 
| EncodeUtf8  | Scalar | 2048     | 487.154 ns   | 1.9383 ns  | 1.8131 ns  | 
