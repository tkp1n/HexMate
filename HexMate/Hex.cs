#if SPAN
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
#if INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif

//        System.Buffers.Text candidate
namespace HexMate
{
    public static unsafe class Hex
    {
        /// <summary>
        /// Decode the span of UTF-8 encoded text represented as hexadecimal characters into binary data.
        /// If the input is not a multiple of 2, it will decode as much as it can, to the closest multiple of 2.
        /// </summary>
        /// <param name="utf8">The input span which contains UTF-8 encoded text that needs to be decoded.</param>
        /// <param name="bytes">The output span which contains the result of the operation, i.e. the decoded binary data.</param>
        /// <param name="bytesConsumed">The number of input bytes consumed during the operation. This can be used to slice the input for subsequent calls, if necessary.</param>
        /// <param name="bytesWritten">The number of bytes written into the output span. This can be used to slice the output for subsequent calls, if necessary.</param>
        /// <param name="isFinalBlock">True (default) when the input span contains the entire data to decode.
        /// Set to false only if it is known that the input span contains partial data with more data to follow.</param>
        /// <returns>It returns the OperationStatus enum values:
        /// - Done - on successful processing of the entire input span
        /// - DestinationTooSmall - if there is not enough space in the output span to fit the decoded input
        /// - NeedMoreData - only if isFinalBlock is false and the input is not a multiple of 2, otherwise the partial input would be considered as InvalidData
        /// - InvalidData - if the input contains bytes outside of the expected range, or if the input is incomplete (i.e. not a multiple of 2) and isFinalBlock is true.
        /// </returns>
        public static OperationStatus DecodeFromUtf8(ReadOnlySpan<byte> utf8, Span<byte> bytes, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true)
        {
            if (utf8.IsEmpty)
            {
                bytesConsumed = 0;
                bytesWritten = 0;
                return OperationStatus.Done;
            }

            int bytesToProcess;
            OperationStatus positiveResult;
            if (utf8.Length % 2 == 0)
            {
                positiveResult = OperationStatus.Done;
                bytesToProcess = utf8.Length;
            }
            else
            {
                positiveResult = OperationStatus.NeedMoreData;
                bytesToProcess = FastMath.RoundDownTo2(utf8.Length);
            }

            bytesToProcess >>= 1;

            if (bytes.Length < bytesToProcess)
            {
                if (isFinalBlock)
                {
                    bytesConsumed = 0;
                    bytesWritten = 0;
                    return OperationStatus.InvalidData;
                }

                bytesToProcess = bytes.Length;
                positiveResult = OperationStatus.DestinationTooSmall;
            }

            var remaining = bytesToProcess;

            fixed (byte* srcBytes = &MemoryMarshal.GetReference(utf8))
            fixed (byte* destBytes = &MemoryMarshal.GetReference(bytes))
            {
                var src = srcBytes;
                var dest = destBytes;

#if INTRINSICS
                if (Avx2.IsSupported && remaining >= 32)
                {
                    Utf8HexParser.Avx2.TryParse(ref src, ref dest, remaining);
                    remaining = bytesToProcess - (int) (dest - destBytes);
                }

                if (remaining >= 16)
                {
                    if (Ssse3.IsSupported)
                    {
                        Utf8HexParser.Ssse3.TryParse(ref src, ref dest, remaining);
                        remaining = bytesToProcess - (int) (dest - destBytes);
                    }
                    else if (Sse2.IsSupported)
                    {
                        Utf8HexParser.Sse2.TryParse(ref src, ref dest, remaining);
                        remaining = bytesToProcess - (int) (dest - destBytes);
                    }
                }
#endif
#if SPAN
                if (!Utf8HexParser.Span.TryParse(ref src, ref dest, remaining)) goto InvalidData;
#else
                if (!Utf8HexParser.Fixed.TryParse(ref src, ref dest, remaining)) goto InvalidData;
#endif
                Debug.Assert(bytesToProcess - (int) (dest - destBytes) == 0);
                bytesConsumed = (int) (src - srcBytes);
                bytesWritten = (int) (dest - destBytes);
                return positiveResult;

            InvalidData:
                bytesConsumed = (int) (src - srcBytes);
                bytesWritten = (int) (dest - destBytes);
                return OperationStatus.InvalidData;
            }
        }

        /// <summary>
        /// Decode the span of UTF-8 encoded text represented as hexadecimal characters into binary data.
        /// The decoded binary output is smaller than the text data contained in the input (the operation deflates the data).
        /// If the input is not a multiple of 2, it will not decode any.
        /// </summary>
        /// <param name="buffer">The input span which contains UTF-8 encoded text that needs to be decoded.</param>
        /// <param name="bytesWritten">The number of bytes written into the buffer.</param>
        /// <returns>It returns the OperationStatus enum values:
        /// - Done - on successful processing of the entire input span
        /// - InvalidData - if the input contains bytes outside of the expected range, or if the input is incomplete (i.e. not a multiple of 2).
        /// It does not return DestinationTooSmall since that is not possible for hex decoding.
        /// It does not return NeedMoreData since this method tramples the data in the buffer and
        /// hence can only be called once with all the data in the buffer.
        /// </returns>
        public static OperationStatus DecodeFromUtf8InPlace(Span<byte> buffer, out int bytesWritten)
        {
            if (buffer.IsEmpty)
            {
                bytesWritten = 0;
                return OperationStatus.Done;
            }

            if (buffer.Length % 2 != 0)
            {
                bytesWritten = 0;
                return OperationStatus.InvalidData;
            }

            var bytesToProcess = buffer.Length >> 1;
            var remaining = bytesToProcess;

            fixed (byte* srcBytes = &MemoryMarshal.GetReference(buffer))
            {
                var src = srcBytes;
                var dest = srcBytes;

#if INTRINSICS
                if (Avx2.IsSupported && remaining >= 32)
                {
                    Utf8HexParser.Avx2.TryParse(ref src, ref dest, remaining);
                    remaining = bytesToProcess - (int) (dest - srcBytes);
                }

                if (remaining >= 16)
                {
                    if (Ssse3.IsSupported)
                    {
                        Utf8HexParser.Ssse3.TryParse(ref src, ref dest, remaining);
                        remaining = bytesToProcess - (int) (dest - srcBytes);
                    }
                    else if (Sse2.IsSupported)
                    {
                        Utf8HexParser.Sse2.TryParse(ref src, ref dest, remaining);
                        remaining = bytesToProcess - (int) (dest - srcBytes);
                    }
                }
#endif
#if SPAN
                if (!Utf8HexParser.Span.TryParse(ref src, ref dest, remaining)) goto InvalidData;
#else
                if (!Utf8HexParser.Fixed.TryParse(ref src, ref dest, remaining)) goto InvalidData;
#endif
                Debug.Assert(bytesToProcess - (int) (dest - srcBytes) == 0);
                bytesWritten = (int) (dest - srcBytes);
                return OperationStatus.Done;

            InvalidData:
                bytesWritten = (int) (dest - srcBytes);
                return OperationStatus.InvalidData;
            }
        }

        /// <summary>
        /// Encode the span of binary data into UTF-8 encoded text represented as hexadecimal characters.
        /// </summary>
        /// <param name="bytes">The input span which contains binary data that needs to be encoded.</param>
        /// <param name="utf8">The output span which contains the result of the operation, i.e. the UTF-8 encoded as hexadecimal characters.</param>
        /// <param name="bytesConsumed">The number of input bytes consumed during the operation. This can be used to slice the input for subsequent calls, if necessary.</param>
        /// <param name="bytesWritten">The number of bytes written into the output span. This can be used to slice the output for subsequent calls, if necessary.</param>
        /// <param name="isFinalBlock">True (default) when the input span contains the entire data to encode.
        /// Set to false only if it is known that the input span contains partial data with more data to follow.</param>
        /// <returns>It returns the OperationStatus enum values:
        /// - Done - on successful processing of the entire input span
        /// - DestinationTooSmall - if there is not enough space in the output span to fit the encoded input
        /// - NeedMoreData - only if isFinalBlock is false
        /// It does not return InvalidData since that is not possible for hex encoding.
        /// </returns>
        public static OperationStatus EncodeToUtf8(ReadOnlySpan<byte> bytes, Span<byte> utf8, out int bytesConsumed, out int bytesWritten, bool isFinalBlock = true)
        {
            if (bytes.IsEmpty)
            {
                bytesConsumed = 0;
                bytesWritten = 0;
                return OperationStatus.Done;
            }

            int remaining;
            OperationStatus result;
            if (utf8.Length >= bytes.Length << 1)
            {
                result = OperationStatus.Done;
                remaining = bytes.Length;
            }
            else
            {
                result = OperationStatus.DestinationTooSmall;
                remaining = utf8.Length >> 1;
            }

            bytesConsumed = remaining;
            bytesWritten = remaining << 1;

            fixed (byte* srcBytes = &MemoryMarshal.GetReference(bytes))
            fixed (byte* destBytes = &MemoryMarshal.GetReference(utf8))
            {
                var src = srcBytes + remaining;
                var dest = destBytes + bytesWritten;

#if INTRINSICS
                if (Avx2.IsSupported && remaining >= 32)
                {
                    remaining -= Utf8HexFormatter.Avx2.Format(ref src, ref dest, remaining);
                    Debug.Assert(remaining < 32);
                }

                if (remaining >= 16)
                {
                    if (Ssse3.IsSupported)
                    {
                        remaining -= Utf8HexFormatter.Ssse3.Format(ref src, ref dest, remaining);
                        Debug.Assert(remaining < 16);
                    }
                    else if (Sse2.IsSupported)
                    {
                        remaining -= Utf8HexFormatter.Sse2.Format(ref src, ref dest, remaining);
                        Debug.Assert(remaining < 16);
                    }
                }
#endif
                if (remaining > 0)
                {
                    remaining -= Utf8HexFormatter.Fixed.Format(ref src, ref dest, remaining);
                }
                Debug.Assert(remaining == 0);
            }

            return isFinalBlock ? result : OperationStatus.NeedMoreData;
        }

        /// <summary>
        /// Encode the span of binary data (in-place) into UTF-8 encoded text represented as hexadecimal characters.
        /// The encoded text output is larger than the binary data contained in the input (the operation inflates the data).
        /// </summary>
        /// <param name="buffer">The input span which contains binary data that needs to be encoded.
        /// It needs to be large enough to fit the result of the operation.</param>
        /// <param name="dataLength">The amount of binary data contained within the buffer that needs to be encoded
        /// (and needs to be smaller than the buffer length).</param>
        /// <param name="bytesWritten">The number of bytes written into the buffer.</param>
        /// <returns>It returns the OperationStatus enum values:
        /// - Done - on successful processing of the entire buffer
        /// - DestinationTooSmall - if there is not enough space in the buffer beyond dataLength to fit the result of encoding the input
        /// It does not return NeedMoreData since this method tramples the data in the buffer and hence can only be called once with all the data in the buffer.
        /// It does not return InvalidData since that is not possible for hex encoding.
        /// </returns>
        public static OperationStatus EncodeToUtf8InPlace(Span<byte> buffer, int dataLength, out int bytesWritten)
        {
            if (buffer.IsEmpty)
            {
                bytesWritten = 0;
                return OperationStatus.Done;
            }

            if (buffer.Length < dataLength << 1)
            {
                bytesWritten = 0;
                return OperationStatus.DestinationTooSmall;
            }

            var remaining = dataLength;
            bytesWritten = remaining << 1;

            fixed (byte* bytes = &MemoryMarshal.GetReference(buffer))
            {
                var src = bytes + remaining;
                var dest = bytes + bytesWritten;

#if INTRINSICS
                if (Avx2.IsSupported && remaining >= 32)
                {
                    remaining -= Utf8HexFormatter.Avx2.Format(ref src, ref dest, remaining);
                    Debug.Assert(remaining < 32);
                }

                if (remaining >= 16)
                {
                    if (Ssse3.IsSupported)
                    {
                        remaining -= Utf8HexFormatter.Ssse3.Format(ref src, ref dest, remaining);
                        Debug.Assert(remaining < 16);
                    }
                    else if (Sse2.IsSupported)
                    {
                        remaining -= Utf8HexFormatter.Sse2.Format(ref src, ref dest, remaining);
                        Debug.Assert(remaining < 16);
                    }
                }
#endif
                if (remaining > 0)
                {
                    remaining -= Utf8HexFormatter.Fixed.Format(ref src, ref dest, remaining);
                }
                Debug.Assert(remaining == 0);
            }

            return OperationStatus.Done;
        }
    }
}
#endif