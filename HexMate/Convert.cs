using System;
using System.Diagnostics;
#if SPAN
using System.Runtime.InteropServices;
#endif
#if INTRINSICS
using System.Runtime.Intrinsics.X86;
#endif

//        System candidate
namespace HexMate
{
    public static class Convert
    {
        /// <summary>
        /// Converts a subset of a Unicode character array, which encodes binary data as hex characters,
        /// to an equivalent 8-bit unsigned integer array.
        /// Parameters specify the subset in the input array and the number of elements to convert.
        /// </summary>
        /// <param name="inArray">A Unicode character array.</param>
        /// <param name="offset">A position within <paramref name="inArray"/>.</param>
        /// <param name="length">The number of elements in <paramref name="inArray"/> to convert.</param>
        /// <returns>An array of 8-bit unsigned integers equivalent to <paramref name="length"/> elements at position <paramref name="offset"/> in <paramref name="inArray"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <code>null</code>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="length"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> plus <paramref name="length"/> indicates a position not within inArray.</exception>
        /// <exception cref="FormatException">The length of <paramref name="inArray"/>, ignoring white-space characters, is not zero or a multiple of 2.</exception>
        /// <exception cref="FormatException">The format of <paramref name="inArray"/> is invalid. <paramref name="inArray"/> contains a non-hex character.</exception>
        public static byte[] FromHexCharArray(char[] inArray, int offset, int length)
        {
            if (inArray == null)
                throw new ArgumentNullException(nameof(inArray));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_Index);
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_GenericPositive);
            if (offset > (inArray.Length - length))
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_OffsetLength);
            if (length == 0)
                return Array.Empty<byte>();

            unsafe
            {
                fixed (char* sIn = inArray)
                {
                    var input = sIn + offset;
                    var resultLength = FromHex_ComputeResultLength(input, length);
#if GC_ALLOC_UNINIT
                    var result = GC.AllocateUninitializedArray<byte>(resultLength);
#else
                    var result = new byte[resultLength];
#endif
                    fixed (byte* outPtr = result)
                    {
                        var res = ConvertFromHexArray(outPtr, result.Length, input, length);
                        if (res < 0)
                            throw new FormatException(SR.Format_BadHexChar);
                        Debug.Assert(res == result.Length);
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Converts the specified string, which encodes binary data as hex characters, to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>An array of 8-bit unsigned integers that is equivalent to <paramref name="s"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is <code>null</code>.</exception>
        /// <exception cref="FormatException">The length of <paramref name="s"/>, ignoring white-space characters, is not zero or a multiple of 2.</exception>
        /// <exception cref="FormatException">The format of <paramref name="s"/> is invalid. <paramref name="s"/> contains a non-hex character.</exception>
        public static byte[] FromHexString(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            if (s.Length == 0)
                return Array.Empty<byte>();

            unsafe
            {
                fixed (char* inPtr = s)
                {
                    var resultLength = FromHex_ComputeResultLength(inPtr, s.Length);
#if GC_ALLOC_UNINIT
                    var result = GC.AllocateUninitializedArray<byte>(resultLength);
#else
                    var result = new byte[resultLength];
#endif
                    fixed (byte* outPtr = result)
                    {
                        var res = ConvertFromHexArray(outPtr, result.Length, inPtr, s.Length);
                        if (res < 0)
                            throw new FormatException(SR.Format_BadHexChar);
                        Debug.Assert(res == result.Length);
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Converts a subset of an 8-bit unsigned integer array to an equivalent subset of a Unicode character array encoded with hex characters.
        /// Parameters specify the subsets as offsets in the input and output arrays, the number of elements in the input array to convert,
        /// whether line breaks are inserted in the output array, and whether to insert upper- or lowercase hex characters.
        /// </summary>
        /// <param name="inArray">An input array of 8-bit unsigned integers.</param>
        /// <param name="offsetIn">A position within <paramref name="inArray"/>.</param>
        /// <param name="length">The number of elements of <paramref name="inArray"/> to convert.</param>
        /// <param name="outArray">An output array of Unicode characters.</param>
        /// <param name="offsetOut">A position within <paramref name="outArray"/>.</param>
        /// <param name="options"><see cref="HexFormattingOptions.Lowercase"/> to produce lowercase output. <see cref="HexFormattingOptions.InsertLineBreaks"/> to insert a line break every 72 characters. <see cref="HexFormattingOptions.None"/> to do neither.</param>
        /// <returns>A 32-bit signed integer containing the number of bytes in <paramref name="outArray"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="inArray"/> or <paramref name="outArray"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offsetIn"/>, <paramref name="offsetOut"/>, or <paramref name="length"/> is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offsetIn"/> plus <paramref name="length"/> is greater than the length of <paramref name="inArray"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offsetOut"/> plus the number of elements to return is greater than the length of <paramref name="outArray"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid <see cref="HexFormattingOptions"/> value.</exception>
        public static int ToHexCharArray(byte[] inArray, int offsetIn, int length, char[] outArray, int offsetOut, HexFormattingOptions options = default)
        {
            if (inArray == null)
                throw new ArgumentNullException(nameof(inArray));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_Index);
            if (offsetIn < 0)
                throw new ArgumentOutOfRangeException(nameof(offsetIn), SR.ArgumentOutOfRange_GenericPositive);
            if (offsetIn > (inArray.Length - length))
                throw new ArgumentOutOfRangeException(nameof(offsetIn), SR.ArgumentOutOfRange_OffsetLength);
            if (outArray == null)
                throw new ArgumentNullException(nameof(outArray));
            if (offsetOut < 0)
                throw new ArgumentOutOfRangeException(nameof(offsetOut), SR.ArgumentOutOfRange_GenericPositive);
            if (options < HexFormattingOptions.None || options > (HexFormattingOptions.Lowercase | HexFormattingOptions.InsertLineBreaks))
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (int)options), nameof(options));

            var insertLineBreaks = (options & HexFormattingOptions.InsertLineBreaks) != 0;
            var outlen = ToHex_CalculateAndValidateOutputLength(inArray.Length - offsetIn, insertLineBreaks);
            if (offsetOut > outArray.Length - outlen)
                throw new ArgumentOutOfRangeException(nameof(offsetOut), SR.ArgumentOutOfRange_OffsetOut);

            unsafe
            {
                fixed (byte* inPtr = inArray)
                fixed (char* outPtr = outArray)
                {
                    ConvertToHexArray(outPtr + offsetOut, outlen, inPtr + offsetIn, length, options);
                }
            }

            return outlen;
        }

        /// <summary>
        /// Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with hex characters.
        /// A parameter specifies whether to insert line breaks in the return value and whether to insert upper- or lowercase hex characters.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <param name="options"><see cref="HexFormattingOptions.Lowercase"/> to produce lowercase output. <see cref="HexFormattingOptions.InsertLineBreaks"/> to insert a line break every 72 characters. <see cref="HexFormattingOptions.None"/> to do neither.</param>
        /// <returns>The string representation in hex of the elements in <paramref name="inArray"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <code>null</code>.</exception>
        /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid <see cref="HexFormattingOptions"/> value.</exception>
        public static string ToHexString(byte[] inArray, HexFormattingOptions options = default)
            => ToHexString(inArray, 0, inArray?.Length ?? 0, options);

        /// <summary>
        /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with hex characters.
        /// Parameters specify the subset as an offset in the input array, the number of elements in the array to convert,
        /// whether to insert line breaks in the return value, and whether to insert upper- or lowercase hex characters.
        /// </summary>
        /// <param name="inArray">An array of 8-bit unsigned integers.</param>
        /// <param name="offset">An offset in <paramref name="inArray"/>.</param>
        /// <param name="length">The number of elements of <paramref name="inArray"/> to convert.</param>
        /// <param name="options"><see cref="HexFormattingOptions.Lowercase"/> to produce lowercase output. <see cref="HexFormattingOptions.InsertLineBreaks"/> to insert a line break every 72 characters. <see cref="HexFormattingOptions.None"/> to do neither.</param>
        /// <returns>The string representation in hex of <paramref name="length"/> elements of <paramref name="inArray"/>, starting at position <paramref name="offset"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <code>null</code>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="length"/> is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> plus <paramref name="length"/> is greater than the length of <paramref name="inArray"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid <see cref="HexFormattingOptions"/> value.</exception>
        public static string ToHexString(byte[] inArray, int offset, int length, HexFormattingOptions options = default)
        {
            if (inArray == null)
                throw new ArgumentNullException(nameof(inArray));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_Index);
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_GenericPositive);
            if (offset > (inArray.Length - length))
                throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_OffsetLength);
            if (options < HexFormattingOptions.None || options > (HexFormattingOptions.Lowercase | HexFormattingOptions.InsertLineBreaks))
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (int)options), nameof(options));
            if (length == 0)
                return string.Empty;

            var insertLineBreaks = (options & HexFormattingOptions.InsertLineBreaks) != 0;
            var outlen = ToHex_CalculateAndValidateOutputLength(inArray.Length - offset, insertLineBreaks);
#if SPAN
            return string.Create(outlen, (arr: inArray, off: offset, len: length, opt: options), (span, state) =>
            {
                unsafe
                {
                    fixed (byte* inPtr = state.arr)
                    fixed (char* outPtr = &MemoryMarshal.GetReference(span))
                    {
                        ConvertToHexArray(outPtr, span.Length, inPtr + state.off, state.len, state.opt);
                    }
                }
            });
#else
            var result = new string('\0', outlen);
            unsafe
            {
                fixed (byte* inPtr = inArray)
                fixed (char* outPtr = result)
                {
                    ConvertToHexArray(outPtr, outlen, inPtr + offset, length, options);
                }
            }

            return result;
#endif
        }

#if SPAN
        /// <summary>
        /// Converts the span, which encodes binary data as hex characters, to an equivalent 8-bit unsigned integer array.
        /// </summary>
        /// <param name="chars">The span to convert.</param>
        /// <returns>An array of 8-bit unsigned integers that is equivalent to <paramref name="chars"/>.</returns>
        /// <exception cref="FormatException">The length of <paramref name="chars"/>, ignoring white-space characters, is not zero or a multiple of 2.</exception>
        /// <exception cref="FormatException">The format of <paramref name="chars"/> is invalid. <paramref name="chars"/> contains a non-hex character.</exception>
        public static byte[] FromHexString(ReadOnlySpan<char> chars)
        {
            if (chars.Length == 0)
                return Array.Empty<byte>();

            unsafe
            {
                fixed (char* inPtr = &MemoryMarshal.GetReference(chars))
                {
                    var resultLength = FromHex_ComputeResultLength(inPtr, chars.Length);
#if GC_ALLOC_UNINIT
                    var result = GC.AllocateUninitializedArray<byte>(resultLength);
#else
                    var result = new byte[resultLength];
#endif
                    fixed (byte* outPtr = result)
                    {
                        var res = ConvertFromHexArray(outPtr, result.Length, inPtr, chars.Length);
                        if (res < 0)
                            throw new FormatException(SR.Format_BadHexChar);
                        Debug.Assert(res == result.Length);
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Converts a span of 8-bit unsigned integers to its equivalent string representation that is encoded with hex characters.
        /// A parameter specifies whether to insert line breaks in the return value and whether to insert upper- or lowercase hex characters.
        /// </summary>
        /// <param name="bytes">A span of 8-bit unsigned integers.</param>
        /// <param name="options"><see cref="HexFormattingOptions.Lowercase"/> to produce lowercase output. <see cref="HexFormattingOptions.InsertLineBreaks"/> to insert a line break every 72 characters. <see cref="HexFormattingOptions.None"/> to do neither.</param>
        /// <returns>The string representation in hex of the elements in <paramref name="bytes"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid <see cref="HexFormattingOptions"/> value.</exception>
        public static string ToHexString(ReadOnlySpan<byte> bytes, HexFormattingOptions options = default)
        {
            if (options < HexFormattingOptions.None || options > (HexFormattingOptions.Lowercase | HexFormattingOptions.InsertLineBreaks))
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (int)options), nameof(options));
            if (bytes.Length == 0)
                return string.Empty;

            var insertLineBreaks = (options & HexFormattingOptions.InsertLineBreaks) != 0;
            var outlen = ToHex_CalculateAndValidateOutputLength(bytes.Length, insertLineBreaks);

            unsafe
            {
                fixed (byte* bytesPtr = &MemoryMarshal.GetReference(bytes))
                {
                    return string.Create(outlen, (Bytes: (IntPtr) bytesPtr, bytes.Length, Options: options), (span, state) =>
                    {
                        fixed (char* outPtr = &MemoryMarshal.GetReference(span))
                        {
                            ConvertToHexArray(outPtr, span.Length, (byte*) state.Bytes, state.Length, state.Options);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Converts the span, which encodes binary data as hex characters, into a span of equivalent 8-bit unsigned integers.
        /// </summary>
        /// <param name="chars">The span to convert.</param>
        /// <param name="bytes">The output span.</param>
        /// <param name="bytesWritten">The number of bytes written into <see cref="bytes"/>.</param>
        /// <returns>Whether the conversion operation completed successfully.</returns>
        public static bool TryFromHexChars(ReadOnlySpan<char> chars, Span<byte> bytes, out int bytesWritten)
        {
            if (chars.Length == 0)
            {
                bytesWritten = 0;
                return true;
            }

            unsafe
            {
                fixed (char* inPtr = &MemoryMarshal.GetReference(chars))
                fixed (byte* outPtr = &MemoryMarshal.GetReference(bytes))
                {
                    var res = ConvertFromHexArray(outPtr, bytes.Length, inPtr, chars.Length);
                    if (res < 0)
                    {
                        bytesWritten = 0;
                        return false;
                    }

                    bytesWritten = res;
                    return true;
                }
            }
        }

        /// <summary>
        /// Converts the specified string, which encodes binary data as hex characters, into a span of equivalent 8-bit unsigned integers.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <param name="bytes">The output span.</param>
        /// <param name="bytesWritten">The number of bytes written into <see cref="bytes"/>.</param>
        /// <returns>Whether the conversion operation completed successfully.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is <code>null</code>.</exception>
        public static bool TryFromHexString(string s, Span<byte> bytes, out int bytesWritten)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            return TryFromHexChars(s.AsSpan(), bytes, out bytesWritten);
        }

        /// <summary>
        /// Converts a span of 8-bit unsigned integers to an equivalent span of a Unicode characters encoded with hex characters.
        /// A parameter specifies whether to insert line breaks in the return value and whether to insert upper- or lowercase hex characters.
        /// </summary>
        /// <param name="bytes">The input span.</param>
        /// <param name="chars">The output span</param>
        /// <param name="charsWritten">The number of bytes written into <see cref="chars"/>.</param>
        /// <param name="options"><see cref="HexFormattingOptions.Lowercase"/> to produce lowercase output. <see cref="HexFormattingOptions.InsertLineBreaks"/> to insert a line break every 72 characters. <see cref="HexFormattingOptions.None"/> to do neither.</param>
        /// <returns>Whether the conversion operation completed successfully.</returns>
        /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid <see cref="HexFormattingOptions"/> value.</exception>
        public static bool TryToHexChars(ReadOnlySpan<byte> bytes, Span<char> chars, out int charsWritten, HexFormattingOptions options = default)
        {
            if (options < HexFormattingOptions.None || options > (HexFormattingOptions.Lowercase | HexFormattingOptions.InsertLineBreaks))
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (int)options), nameof(options));
            if (bytes.Length == 0)
            {
                charsWritten = 0;
                return true;
            }

            var insertLineBreaks = (options & HexFormattingOptions.InsertLineBreaks) != 0;
            var outlen = ToHex_CalculateAndValidateOutputLength(bytes.Length, insertLineBreaks);

            if (chars.Length < outlen)
            {
                charsWritten = 0;
                return false;
            }

            unsafe
            {
                fixed (byte* inPtr = &MemoryMarshal.GetReference(bytes))
                fixed (char* outPtr = &MemoryMarshal.GetReference(chars))
                {
                    ConvertToHexArray(outPtr, chars.Length, inPtr, bytes.Length, options);
                }
            }

            charsWritten = outlen;
            return true;
        }
#endif
        private const int hexLineBreakPosition = 72;
        private const int errInvalidData = -1;
        private const int errDestTooShort = -2;

        private static int ToHex_CalculateAndValidateOutputLength(int inputLength, bool insertLineBreaks)
        {
            var outlen = (long) inputLength * 2;

            if (outlen == 0)
                return 0;

            if (insertLineBreaks)
            {
                var newLines = outlen / hexLineBreakPosition;
                if ((outlen % hexLineBreakPosition) == 0)
                {
                    --newLines;
                }
                outlen += newLines * 2;              // the number of line break chars we'll add, "\r\n"
            }

            // If we overflow an int then we cannot allocate enough
            // memory to output the value so throw
            if (outlen > int.MaxValue)
                throw new OutOfMemoryException();

            return (int)outlen;
        }

        private static unsafe void ConvertToHexArray(char* outChars, int outLength, byte* inData, int inLength, HexFormattingOptions options)
        {
            const int perLine = hexLineBreakPosition / 2;
            var remaining = inLength;

            var src = inData + inLength;
            var dest = outChars + outLength;

            var toLower = (options & HexFormattingOptions.Lowercase) != 0;
            var insertLineBreaks = (options & HexFormattingOptions.InsertLineBreaks) != 0;
            if (!insertLineBreaks)
            {
#if INTRINSICS
                if (Avx2.IsSupported & remaining >= 32)
                {
                    remaining -= Utf16HexFormatter.Avx2.Format(ref src, ref dest, remaining, toLower);
                    Debug.Assert(remaining < 32);
                }

                if (remaining >= 16)
                {
                    if (Ssse3.IsSupported)
                    {
                        remaining -= Utf16HexFormatter.Ssse3.Format(ref src, ref dest, remaining, toLower);
                        Debug.Assert(remaining < 16);
                    }
                    else if (Sse2.IsSupported)
                    {
                        remaining -= Utf16HexFormatter.Sse2.Format(ref src, ref dest, remaining, toLower);
                        Debug.Assert(remaining < 16);
                    }
                }
#endif
                if (remaining > 0)
                {
                    remaining -= Utf16HexFormatter.Fixed.Format(ref src, ref dest, remaining, toLower);
                }
                Debug.Assert(remaining == 0);
            }
            else
            {
                var remainder = remaining % perLine;
                var remainingOnCurrentLine = remainder == 0 ? perLine : remainder;
                var bytesInIteration = remainingOnCurrentLine;
                while (remaining != 0)
                {
#if INTRINSICS
                    if (Avx2.IsSupported)
                    {
                        if (remainingOnCurrentLine >= 32)
                        {
                            remainingOnCurrentLine -= Utf16HexFormatter.Avx2.Format(ref src, ref dest, remainingOnCurrentLine, toLower);
                            Debug.Assert(remainingOnCurrentLine < 32);
                        }
                    }
                    else if (remainingOnCurrentLine >= 16)
                    {
                        // Never run SSE code if AVX2 is available, as the remaining 8 bytes per line cannot be SSE-processed
                        if (Ssse3.IsSupported)
                        {
                            remainingOnCurrentLine -= Utf16HexFormatter.Ssse3.Format(ref src, ref dest, remainingOnCurrentLine, toLower);
                            Debug.Assert(remainingOnCurrentLine < 16);
                        }
                        else if (Sse2.IsSupported)
                        {
                            remainingOnCurrentLine -= Utf16HexFormatter.Sse2.Format(ref src, ref dest, remainingOnCurrentLine, toLower);
                            Debug.Assert(remainingOnCurrentLine < 16);
                        }
                    }
#endif
                    if (remainingOnCurrentLine > 0)
                    {
                        remainingOnCurrentLine -= Utf16HexFormatter.Fixed.Format(ref src, ref dest, remainingOnCurrentLine, toLower);
                    }
                    Debug.Assert(remainingOnCurrentLine == 0);

                    remaining -= bytesInIteration;
                    if (remaining == 0) return;

                    *(--dest) = '\n';
                    *(--dest) = '\r';
                    remainingOnCurrentLine = perLine;
                    bytesInIteration = remainingOnCurrentLine;
                }
            }
        }

        private static bool IsWhitespace(int character)
            => character == ' ' || character == '\n' || character == '\r' || character == '\t';

        private static bool IsValid(uint character)
        {
            character -= '0';
            if (character > 9) goto Invalid;
            character -= 'A' - '0';
            if (character > 5) goto Invalid;
            character -= 'a' - 'A';
            if (character > 5) goto Invalid;

            return true;
        Invalid:
            return false;
        }

        private static unsafe int FromHex_ComputeResultLength(char* inputPtr, int inputLength)
            => CharHelper.CountUsefulCharacters(inputPtr, inputLength) / 2;

        private static unsafe int ConvertFromHexArray(byte* outData, int outLength, char* inChars, int inLength)
        {
            Debug.Assert(inLength != 0);
            if (inLength == 1)
            {
                return errInvalidData;
            }

            var charsToRead = inLength;
            var remainingIn = charsToRead;
            var bytesToWrite = outLength;
            var remainingOut = bytesToWrite;
            var src = inChars;
            var last = src;
            var srcEnd = src + inLength;
            var dest = outData;

            while (remainingOut > 0)
            {
#if INTRINSICS
                // Ignore errors in the SIMD part, and handle them in scalar part below
                if (Avx2.IsSupported && remainingIn >= 64 && remainingOut >= 32)
                {
                    Utf16HexParser.Avx2.TryParse(ref src, ref dest, remainingOut);
                    remainingIn = charsToRead - (int) (src - inChars);
                    remainingOut = bytesToWrite - (int) (dest - outData);
                }

                if (remainingIn >= 32 && remainingOut >= 16)
                {
                    if (Ssse3.IsSupported)
                    {
                        Utf16HexParser.Ssse3.TryParse(ref src, ref dest, remainingOut);
                        remainingIn = charsToRead - (int) (src - inChars);
                        remainingOut = bytesToWrite - (int) (dest - outData);
                    }
                    else if (Sse2.IsSupported)
                    {
                        Utf16HexParser.Sse2.TryParse(ref src, ref dest, remainingOut);
                        remainingIn = charsToRead - (int) (src - inChars);
                        remainingOut = bytesToWrite - (int) (dest - outData);
                    }
                }
#endif
#if SPAN
                if (!Utf16HexParser.Span.TryParse(ref src, remainingIn, ref dest, remainingOut))
#else
                if (!Utf16HexParser.Fixed.TryParse(ref src, remainingIn, ref dest, remainingOut))
#endif
                {
                    goto InvalidData;
                }
                remainingIn = charsToRead - (int) (src - inChars);
                remainingOut = bytesToWrite - (int) (dest - outData);

                if (src >= srcEnd)
                {
                    goto Ok;
                }

                if (src == last)
                {
                    // No progress == invalid data
                    goto InvalidData;
                }

                last = src;
            }

            // No more space in destination

            if (src < srcEnd)
            {
                while (IsWhitespace(*src))
                {
                    // Skip past any consecutive trailing white-space in the input
                    if (++src >= srcEnd)
                    {
                        goto Ok;
                    }
                }

                if (IsValid(*src))
                {
                    goto OutLenTooShort;
                }
                else
                {
                    goto InvalidData;
                }
            }

        Ok:
            return (int) (dest - outData);

        InvalidData:
            return errInvalidData;

        OutLenTooShort:
            return errDestTooShort;
        }
    }
}