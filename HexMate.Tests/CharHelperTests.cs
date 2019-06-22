using System;
using Xunit;

namespace HexMate.Tests
{
    public unsafe class CharHelperTests
    {
        [Fact]
        public void NormalHexString()
        {
            var binary = new byte[1024];
            new Random().NextBytes(binary);
            var hexString = BitConverter.ToString(binary).Replace("-", string.Empty);

            fixed (char* hexStringPtr = hexString)
            {
                var result = CharHelper.CountUsefulCharacters(hexStringPtr, hexString.Length);

                Assert.Equal(hexString.Length, result);
            }
        }

        [Fact]
        public void UselessCharactersIn32ByteChunk()
        {
            var binary = new byte[16];
            new Random().NextBytes(binary);
            var hexString = BitConverter.ToString(binary).Replace("-", string.Empty).ToCharArray();

            hexString[10] = ' ';
            hexString[11] = ' ';
            hexString[15] = ' ';

            fixed (char* hexStringPtr = hexString)
            {
                var result = CharHelper.CountUsefulCharacters(hexStringPtr, hexString.Length);

                Assert.Equal(hexString.Length - 3, result);
            }
        }

        [Fact]
        public void UselessCharactersIn16ByteChunk()
        {
            var binary = new byte[8 + 4];
            new Random().NextBytes(binary);
            var hexString = BitConverter.ToString(binary).Replace("-", string.Empty).ToCharArray();

            hexString[17] = ' ';
            hexString[18] = ' ';
            hexString[20] = ' ';

            fixed (char* hexStringPtr = hexString)
            {
                var result = CharHelper.CountUsefulCharacters(hexStringPtr, hexString.Length);

                Assert.Equal(hexString.Length - 3, result);
            }
        }

        [Fact]
        public void UselessCharactersInScalarPart()
        {
            var binary = new byte[8 + 4 + 3];
            new Random().NextBytes(binary);
            var hexString = BitConverter.ToString(binary).Replace("-", string.Empty).ToCharArray();

            hexString[25] = ' ';
            hexString[27] = ' ';

            fixed (char* hexStringPtr = hexString)
            {
                var result = CharHelper.CountUsefulCharacters(hexStringPtr, hexString.Length);

                Assert.Equal(hexString.Length - 2, result);
            }
        }
    }
}