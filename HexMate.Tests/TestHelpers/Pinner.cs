namespace HexMate.Tests.TestHelpers
{
    internal static unsafe class Pinner
    {
        public delegate void ByteBytePtrAction(byte* a, byte* b);

        public delegate void ByteCharPtrAction(byte* a, char* b);

        public delegate void CharBytePtrAction(char* a, byte* b);

        public static void Pin(byte[] a, byte[] b, ByteBytePtrAction action)
        {
            fixed (byte* aPtr = a)
            fixed (byte* bPtr = b)
            {
                action(aPtr, bPtr);
            }
        }

        public static void Pin(byte[] a, char[] b, ByteCharPtrAction action)
        {
            fixed (byte* aPtr = a)
            fixed (char* bPtr = b)
            {
                action(aPtr, bPtr);
            }
        }

        public static void Pin(char[] a, byte[] b, CharBytePtrAction action)
        {
            fixed (char* aPtr = a)
            fixed (byte* bPtr = b)
            {
                action(aPtr, bPtr);
            }
        }
    }
}
