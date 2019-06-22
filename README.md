# Hex <--> Binary 
## Implementation of API Proposal for [dotnet/corefx#10013](https://github.com/dotnet/corefx/issues/10013)

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

| Method      | Job    | DataSize | Mean        | Error      | StdDev     | 
|-------------|--------|----------|-------------|------------|------------| 
| DecodeUtf16 | AVX2   | 32       | 33.13 ns    | 0.6630 ns  | 0.6511 ns  | 
| DecodeUtf16 | SSE4.1 | 32       | 34.02 ns    | 0.7083 ns  | 0.6625 ns  | 
| DecodeUtf16 | SSSE3  | 32       | 37.03 ns    | 0.7644 ns  | 0.7850 ns  | 
| DecodeUtf16 | SSE2   | 32       | 43.49 ns    | 0.8232 ns  | 0.7701 ns  | 
| DecodeUtf16 | Scalar | 32       | 46.71 ns    | 0.8503 ns  | 0.7954 ns  | 
|             |        |          |             |            |            | 
| DecodeUtf16 | AVX2   | 2048     | 219.62 ns   | 3.4391 ns  | 3.2169 ns  | 
| DecodeUtf16 | SSE4.1 | 2048     | 396.82 ns   | 2.0316 ns  | 1.5861 ns  | 
| DecodeUtf16 | SSSE3  | 2048     | 438.41 ns   | 8.6944 ns  | 8.5391 ns  | 
| DecodeUtf16 | SSE2   | 2048     | 572.23 ns   | 11.0704 ns | 10.3553 ns | 
| DecodeUtf16 | Scalar | 2048     | 1,935.50 ns | 35.5806 ns | 33.2821 ns | 
|             |        |          |             |            |            | 
| EncodeUtf16 | AVX2   | 32       | 14.31 ns    | 0.3031 ns  | 0.2836 ns  | 
| EncodeUtf16 | SSSE3  | 32       | 14.12 ns    | 0.2935 ns  | 0.2745 ns  | 
| EncodeUtf16 | SSE2   | 32       | 14.21 ns    | 0.3023 ns  | 0.3105 ns  | 
| EncodeUtf16 | Scalar | 32       | 14.06 ns    | 0.2855 ns  | 0.2670 ns  | 
|             |        |          |             |            |            | 
| EncodeUtf16 | AVX2   | 2048     | 14.12 ns    | 0.3073 ns  | 0.2875 ns  | 
| EncodeUtf16 | SSSE3  | 2048     | 13.80 ns    | 0.3083 ns  | 0.3028 ns  | 
| EncodeUtf16 | SSE2   | 2048     | 13.95 ns    | 0.2076 ns  | 0.1942 ns  | 
| EncodeUtf16 | Scalar | 2048     | 13.91 ns    | 0.2728 ns  | 0.2552 ns  | 
|             |        |          |             |            |            | 
| DecodeUtf8  | AVX2   | 32       | 28.73 ns    | 0.5997 ns  | 1.1117 ns  | 
| DecodeUtf8  | SSE4.1 | 32       | 25.65 ns    | 0.4005 ns  | 0.3746 ns  | 
| DecodeUtf8  | SSSE3  | 32       | 26.23 ns    | 0.4507 ns  | 0.4215 ns  | 
| DecodeUtf8  | SSE2   | 32       | 32.38 ns    | 0.6531 ns  | 0.6109 ns  | 
| DecodeUtf8  | Scalar | 32       | 42.46 ns    | 0.8818 ns  | 0.8660 ns  | 
|             |        |          |             |            |            | 
| DecodeUtf8  | AVX2   | 2048     | 163.69 ns   | 3.2454 ns  | 3.4726 ns  | 
| DecodeUtf8  | SSE4.1 | 2048     | 342.94 ns   | 6.5188 ns  | 5.7788 ns  | 
| DecodeUtf8  | SSSE3  | 2048     | 390.88 ns   | 7.7759 ns  | 7.2736 ns  | 
| DecodeUtf8  | SSE2   | 2048     | 514.38 ns   | 8.9631 ns  | 8.3840 ns  | 
| DecodeUtf8  | Scalar | 2048     | 1,902.77 ns | 37.6761 ns | 44.8507 ns | 
|             |        |          |             |            |            | 
| EncodeUtf8  | AVX2   | 32       | 18.61 ns    | 0.3902 ns  | 0.3650 ns  | 
| EncodeUtf8  | SSSE3  | 32       | 18.64 ns    | 0.4056 ns  | 0.3794 ns  | 
| EncodeUtf8  | SSE2   | 32       | 21.24 ns    | 0.4344 ns  | 0.4266 ns  | 
| EncodeUtf8  | Scalar | 32       | 28.89 ns    | 0.5113 ns  | 0.4782 ns  | 
|             |        |          |             |            |            | 
| EncodeUtf8  | AVX2   | 2048     | 72.08 ns    | 1.4621 ns  | 1.2961 ns  | 
| EncodeUtf8  | SSSE3  | 2048     | 108.16 ns   | 2.1941 ns  | 2.0523 ns  | 
| EncodeUtf8  | SSE2   | 2048     | 139.82 ns   | 2.7601 ns  | 2.8344 ns  | 
| EncodeUtf8  | Scalar | 2048     | 660.42 ns   | 13.1854 ns | 14.1082 ns | 
