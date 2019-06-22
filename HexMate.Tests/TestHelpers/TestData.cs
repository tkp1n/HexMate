using System;
using System.Text;

namespace HexMate.Tests.TestHelpers
{
    public static class TestData
    {
        public static byte[] Binary(int length = 4096)
        {
            var result = new byte[4096];
            var r = new Random();
            r.NextBytes(result);

            return result;
        }

        private static byte[] HexUpperUtf8(byte[] binary) =>
            Encoding.ASCII.GetBytes(BitConverter.ToString(binary).Replace("-", string.Empty));

        private static byte[] HexLowerUtf8(byte[] binary) =>
            Encoding.ASCII.GetBytes(BitConverter.ToString(binary).Replace("-", string.Empty).ToLower());

        private static char[] HexUpperUtf16(byte[] binary) =>
            BitConverter.ToString(binary).Replace("-", string.Empty).ToCharArray();

        private static char[] HexLowerUtf16(byte[] binary) =>
            BitConverter.ToString(binary).Replace("-", string.Empty).ToLower().ToCharArray();

        public static byte[] HexMixedUtf8(byte[] binary)
        {
            var upper = HexUpperUtf8(binary);
            var lower = HexLowerUtf8(binary);

            var r = new Random();
            var res = new byte[upper.Length];
            for (var i = 0; i < upper.Length; i++)
            {
                res[i] = r.Next() % 2 == 0 ? upper[i] : lower[i];
            }

            return res;
        }

        public static char[] HexMixedUtf16(byte[] binary)
        {
            var upper = HexUpperUtf16(binary);
            var lower = HexLowerUtf16(binary);

            var r = new Random();
            var res = new char[upper.Length];
            for (var i = 0; i < upper.Length; i++)
            {
                res[i] = r.Next() % 2 == 0 ? upper[i] : lower[i];
            }

            return res;
        }
    }
}