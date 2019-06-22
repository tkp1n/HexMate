#if SPAN
using System;
using System.Buffers;
using System.Linq;
using System.Text;
using Xunit;

namespace HexMate.Tests
{
    public class HexTests
    {
        // EncodeToUtf8InPlace

        [Fact]
        public void EncodeEmptyBufferInPlace()
        {
            var status = Hex.EncodeToUtf8InPlace(Span<byte>.Empty, 0, out var written);
            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(0, written);
        }

        [Fact]
        public void EncodeInPlaceBufferTooSmall()
        {
            Span<byte> buffer = stackalloc byte[7];
            var status = Hex.EncodeToUtf8InPlace(buffer, 4, out var written);
            Assert.Equal(OperationStatus.DestinationTooSmall, status);
            Assert.Equal(0, written);
        }

        [Fact]
        public void EncodeInPlaceBufferOverSized()
        {
            var dataLength = 32 + 16 + 4;
            var encodedLength = dataLength * 2;

            var buffer = new byte[encodedLength + 10];
            var encoded = Encoding.UTF8.GetBytes(BitConverter.ToString(buffer, 0, dataLength).Replace("-", string.Empty));
            var expected = encoded.Take(encodedLength).Concat(new byte[10]).ToArray();
            var status = Hex.EncodeToUtf8InPlace(buffer, dataLength, out var written);
            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(encodedLength, written);
            Assert.Equal(expected, buffer);
        }

        [Fact]
        public void EncodeInPlaceUsing32And16ByteSimd()
        {
            var dataLength = 32 + 16 + 4;
            var encodedLength = dataLength * 2;

            var buffer = new byte[encodedLength];
            var encoded = Encoding.UTF8.GetBytes(BitConverter.ToString(buffer, 0, dataLength).Replace("-", string.Empty));
            var status = Hex.EncodeToUtf8InPlace(buffer, dataLength, out var written);
            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(buffer.Length, written);
            Assert.Equal(encoded, buffer);
        }

        [Fact]
        public void EncodeInPlaceUsing32ByteSimd()
        {
            var dataLength = 32 + 4;
            var encodedLength = dataLength * 2;

            var buffer = new byte[encodedLength];
            var encoded = Encoding.UTF8.GetBytes(BitConverter.ToString(buffer, 0, dataLength).Replace("-", string.Empty));
            var status = Hex.EncodeToUtf8InPlace(buffer, dataLength, out var written);
            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(buffer.Length, written);
            Assert.Equal(encoded, buffer);
        }

        [Fact]
        public void EncodeInPlaceUsing16ByteSimd()
        {
            var dataLength = 16;
            var encodedLength = dataLength * 2;

            var buffer = new byte[encodedLength];
            var encoded = Encoding.UTF8.GetBytes(BitConverter.ToString(buffer, 0, dataLength).Replace("-", string.Empty));
            var status = Hex.EncodeToUtf8InPlace(buffer, dataLength, out var written);
            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(buffer.Length, written);
            Assert.Equal(encoded, buffer);
        }

        // EncodeToUtf8

        [Fact]
        public void EncodeEmptyBuffer()
        {
            var status = Hex.EncodeToUtf8(Span<byte>.Empty, Span<byte>.Empty, out var consumed, out var written, false);
            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(0, consumed);
            Assert.Equal(0, written);
        }

        [Fact]
        public void EncodeBufferTooSmall()
        {
            var dataLength = 20;
            var encodedLength = dataLength * 2 - 5;

            var buffer = new byte[dataLength];
            var encoded = new byte[encodedLength];
            var expected = Encoding.UTF8.GetBytes(BitConverter.ToString(buffer, 0, dataLength - 3).Replace("-", string.Empty));

            var status = Hex.EncodeToUtf8(buffer, encoded, out var read, out var written);

            Assert.Equal(OperationStatus.DestinationTooSmall, status);

            Assert.Equal(17, read);
            Assert.Equal(34, written);

            Assert.Equal(expected.Concat(new byte[1]).ToArray(), encoded);
        }

        [Fact]
        public void EncodeBufferOverSized()
        {
            var dataLength = 32 + 16 + 4;
            var encodedLength = dataLength * 2;

            var data = new byte[dataLength];
            var buffer = new byte[encodedLength + 10];
            var encoded = Encoding.UTF8.GetBytes(BitConverter.ToString(data, 0, dataLength).Replace("-", string.Empty));
            var expected = encoded.Take(encodedLength).Concat(new byte[10]).ToArray();

            var status = Hex.EncodeToUtf8(data, buffer, out var read, out var written);

            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(dataLength, read);
            Assert.Equal(encodedLength, written);
            Assert.Equal(expected, buffer);
        }

        [Fact]
        public void EncodeUsing32And16ByteSimd()
        {
            var dataLength = 32 + 16 + 4;
            var encodedLength = dataLength * 2;

            var buffer = new byte[dataLength];
            var encoded = new byte[encodedLength];
            var expected = Encoding.UTF8.GetBytes(BitConverter.ToString(buffer, 0, dataLength).Replace("-", string.Empty));

            var status = Hex.EncodeToUtf8(buffer, encoded, out var read, out var written);

            Assert.Equal(OperationStatus.Done, status);

            Assert.Equal(buffer.Length, read);
            Assert.Equal(encoded.Length, written);

            Assert.Equal(expected, encoded);
        }

        [Fact]
        public void EncodeUsing32ByteSimd()
        {
            var dataLength = 32 + 4;
            var encodedLength = dataLength * 2;

            var buffer = new byte[dataLength];
            var encoded = new byte[encodedLength];
            var expected = Encoding.UTF8.GetBytes(BitConverter.ToString(buffer, 0, dataLength).Replace("-", string.Empty));

            var status = Hex.EncodeToUtf8(buffer, encoded, out var read, out var written);

            Assert.Equal(OperationStatus.Done, status);

            Assert.Equal(buffer.Length, read);
            Assert.Equal(encoded.Length, written);

            Assert.Equal(expected, encoded);
        }

        [Fact]
        public void EncodeUsing16ByteSimdNonFinal()
        {
            var dataLength = 16;
            var encodedLength = dataLength * 2;

            var buffer = new byte[dataLength];
            var encoded = new byte[encodedLength];
            var expected = Encoding.UTF8.GetBytes(BitConverter.ToString(buffer, 0, dataLength).Replace("-", string.Empty));

            var status = Hex.EncodeToUtf8(buffer, encoded, out var read, out var written, false);

            Assert.Equal(OperationStatus.NeedMoreData, status);

            Assert.Equal(buffer.Length, read);
            Assert.Equal(encoded.Length, written);

            Assert.Equal(expected, encoded);
        }

        // DecodeFromUtf8InPlace

        [Fact]
        public void DecodeEmptyBufferInPlace()
        {
            var status = Hex.DecodeFromUtf8InPlace(Span<byte>.Empty, out var written);
            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(0, written);
        }

        [Fact]
        public void DecodeInPlaceUsing16And32ByteSimd()
        {
            var dataLength = 32 + 16 + 4;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            var expected = new byte[dataLength];
            expected.AsSpan().Fill(0xFF);
            var buffer = new byte[encodedLength];
            encoded.CopyTo(buffer.AsSpan());
            expected.CopyTo(buffer.AsSpan());

            var status = Hex.DecodeFromUtf8InPlace(encoded, out var written);

            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(dataLength, written);
            Assert.Equal(buffer, encoded);
        }

        [Fact]
        public void DecodeInPlaceUsing32ByteSimd()
        {
            var dataLength = 32 + 4;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            var expected = new byte[dataLength];
            expected.AsSpan().Fill(0xFF);
            var buffer = new byte[encodedLength];
            encoded.CopyTo(buffer.AsSpan());
            expected.CopyTo(buffer.AsSpan());

            var status = Hex.DecodeFromUtf8InPlace(encoded, out var written);

            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(dataLength, written);
            Assert.Equal(buffer, encoded);
        }

        [Fact]
        public void DecodeInPlaceUsing16ByteSimd()
        {
            var dataLength = 16 + 4;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            var expected = new byte[dataLength];
            expected.AsSpan().Fill(0xFF);
            var buffer = new byte[encodedLength];
            encoded.CopyTo(buffer.AsSpan());
            expected.CopyTo(buffer.AsSpan());

            var status = Hex.DecodeFromUtf8InPlace(encoded, out var written);

            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(dataLength, written);
            Assert.Equal(buffer, encoded);
        }

        [Fact]
        public void DecodeInPlaceOddNofBytes()
        {
            var dataLength = 15;
            var encodedLength = dataLength * 2 - 1;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);

            var status = Hex.DecodeFromUtf8InPlace(encoded, out var written);

            Assert.Equal(OperationStatus.InvalidData, status);
            Assert.Equal(0, written);
        }

        [Fact]
        public void DecodeInPlaceInvalidDataIn32ByteChunk()
        {
            var dataLength = 32;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            encoded[5] = 0x00;

            var status = Hex.DecodeFromUtf8InPlace(encoded, out var written);

            Assert.Equal(OperationStatus.InvalidData, status);
            Assert.Equal(2, written);
        }

        [Fact]
        public void DecodeInPlaceInvalidDataIn16ByteChunk()
        {
            var dataLength = 16;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            encoded[5] = 0x00;

            var status = Hex.DecodeFromUtf8InPlace(encoded, out var written);

            Assert.Equal(OperationStatus.InvalidData, status);
            Assert.Equal(2, written);
        }

        [Fact]
        public void DecodeInPlaceInvalidDataInScalarChunk()
        {
            var dataLength = 10;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            encoded[5] = 0x00;

            var status = Hex.DecodeFromUtf8InPlace(encoded, out var written);

            Assert.Equal(OperationStatus.InvalidData, status);
            Assert.Equal(2, written);
        }

        // DecodeFromUtf8

        [Fact]
        public void DecodeEmptyBuffer()
        {
            var status = Hex.DecodeFromUtf8(Span<byte>.Empty, Span<byte>.Empty, out var consumed, out var written, false);
            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(0, consumed);
            Assert.Equal(0, written);
        }

        [Fact]
        public void DecodeBufferTooSmallNonFinal()
        {
            var dataLength = 20;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            var expected = new byte[dataLength - 1];
            expected.AsSpan().Fill(0xFF);
            var buffer = new byte[dataLength - 1];

            var status = Hex.DecodeFromUtf8(encoded, buffer, out var consumed, out var written, false);

            Assert.Equal(OperationStatus.DestinationTooSmall, status);
            Assert.Equal(encoded.Length - 2, consumed);
            Assert.Equal(buffer.Length, written);
            Assert.Equal(expected, buffer);
        }

        [Fact]
        public void DecodeBufferTooSmallFinal()
        {
            var dataLength = 20;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            var buffer = new byte[dataLength - 1];

            var status = Hex.DecodeFromUtf8(encoded, buffer, out var consumed, out var written);

            Assert.Equal(OperationStatus.InvalidData, status);
            Assert.Equal(0, consumed);
            Assert.Equal(0, written);
        }

        [Fact]
        public void DecodeBufferOverSized()
        {
            var dataLength = 20;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            var buffer = new byte[dataLength + 1];

            var status = Hex.DecodeFromUtf8(encoded, buffer, out var consumed, out var written);

            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(encodedLength, consumed);
            Assert.Equal(dataLength, written);
        }

        [Fact]
        public void DecodeUsing16And32ByteSimd()
        {
            var dataLength = 32 + 16 + 4;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            var expected = new byte[dataLength];
            expected.AsSpan().Fill(0xFF);
            var buffer = new byte[dataLength];

            var status = Hex.DecodeFromUtf8(encoded, buffer, out var consumed, out var written);

            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(encoded.Length, consumed);
            Assert.Equal(buffer.Length, written);
            Assert.Equal(expected, buffer);
        }

        [Fact]
        public void DecodeUsing32ByteSimd()
        {
            var dataLength = 32 + 4;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            var expected = new byte[dataLength];
            expected.AsSpan().Fill(0xFF);
            var buffer = new byte[dataLength];

            var status = Hex.DecodeFromUtf8(encoded, buffer, out var consumed, out var written);

            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(encoded.Length, consumed);
            Assert.Equal(buffer.Length, written);
            Assert.Equal(expected, buffer);
        }

        [Fact]
        public void DecodeUsing16ByteSimd()
        {
            var dataLength = 16 + 4;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            var expected = new byte[dataLength];
            expected.AsSpan().Fill(0xFF);
            var buffer = new byte[dataLength];

            var status = Hex.DecodeFromUtf8(encoded, buffer, out var consumed, out var written);

            Assert.Equal(OperationStatus.Done, status);
            Assert.Equal(encoded.Length, consumed);
            Assert.Equal(buffer.Length, written);
            Assert.Equal(expected, buffer);
        }

        [Fact]
        public void DecodeOddNofBytes()
        {
            var dataLength = 15;
            var encodedLength = dataLength * 2 - 1;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            var expected = new byte[dataLength];
            expected.AsSpan().Fill(0xFF);
            var buffer = new byte[dataLength];
            expected[dataLength - 1] = 0;

            var status = Hex.DecodeFromUtf8(encoded, buffer, out var consumed, out var written);

            Assert.Equal(OperationStatus.NeedMoreData, status);
            Assert.Equal(encodedLength - 1, consumed);
            Assert.Equal(dataLength - 1, written);
            Assert.Equal(expected, buffer);
        }

        [Fact]
        public void DecodeInvalidDataIn32ByteChunk()
        {
            var dataLength = 32;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            encoded[5] = 0x00;
            var buffer = new byte[dataLength];

            var status = Hex.DecodeFromUtf8(encoded, buffer, out var consumed, out var written);

            Assert.Equal(OperationStatus.InvalidData, status);
            Assert.Equal(5, consumed);
            Assert.Equal(2, written);
        }

        [Fact]
        public void DecodeInvalidDataIn16ByteChunk()
        {
            var dataLength = 16;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            encoded[5] = 0x00;
            var buffer = new byte[dataLength];

            var status = Hex.DecodeFromUtf8(encoded, buffer, out var consumed, out var written);

            Assert.Equal(OperationStatus.InvalidData, status);
            Assert.Equal(5, consumed);
            Assert.Equal(2, written);
        }

        [Fact]
        public void DecodeInvalidDataInScalarChunk()
        {
            var dataLength = 10;
            var encodedLength = dataLength * 2;

            var encoded = new byte[encodedLength];
            encoded.AsSpan().Fill(0x46);
            encoded[5] = 0x00;
            var buffer = new byte[dataLength];

            var status = Hex.DecodeFromUtf8(encoded, buffer, out var consumed, out var written);

            Assert.Equal(OperationStatus.InvalidData, status);
            Assert.Equal(5, consumed);
            Assert.Equal(2, written);
        }
    }
}
#endif