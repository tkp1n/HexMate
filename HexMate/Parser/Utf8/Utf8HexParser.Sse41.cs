#if INTRINSICS
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Sse2;
using static System.Runtime.Intrinsics.X86.Ssse3;
using static System.Runtime.Intrinsics.X86.Sse41;
using static HexMate.VectorConstants;
using static HexMate.VectorUtils;

namespace HexMate
{
    internal static partial class Utf8HexParser
    {
        internal static unsafe class Sse41
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool TryParse(ref byte* srcBytes, ref byte* destBytes, int destLength)
            {
                Debug.Assert(System.Runtime.Intrinsics.X86.Sse41.IsSupported);

                var x0F = Vector128.Create((byte) 0x0F);
                var xF0 = Vector128.Create((byte) 0xF0);
                var digHexSelector = ReadVector128(s_upperLowerDigHexSelector);
                var digits = ReadVector128(s_digits);
                var hexs = ReadVector128(s_hexs);
                var evenBytes = ReadVector128(s_evenBytes);
                var oddBytes = ReadVector128(s_oddBytes);
                var src = srcBytes;
                var dest = destBytes;

                var target = dest + FastMath.RoundDownTo16(destLength);
                int leftOk, rightOk;
                while (dest != target)
                {
                    var inputLeft = LoadVector128(src);
                    src += 16;
                    var inputRight = LoadVector128(src);
                    src += 16;

                    var loNibbleLeft = And(inputLeft, x0F);
                    var loNibbleRight = And(inputRight, x0F);

                    var hiNibbleLeft = And(inputLeft, xF0);
                    var hiNibbleRight = And(inputRight, xF0);

                    var leftDigits = Shuffle(digits, loNibbleLeft);
                    var leftHex = Shuffle(hexs, loNibbleLeft);

                    var hiNibbleShLeft = ShiftRightLogical(hiNibbleLeft.AsInt16(), 4).AsByte();
                    var hiNibbleShRight = ShiftRightLogical(hiNibbleRight.AsInt16(), 4).AsByte();

                    var rightDigits = Shuffle(digits, loNibbleRight);
                    var rightHex = Shuffle(hexs, loNibbleRight);

                    var magicLeft = Shuffle(digHexSelector, hiNibbleShLeft);
                    var magicRight = Shuffle(digHexSelector, hiNibbleShRight);

                    var valueLeft = BlendVariable(leftDigits, leftHex, magicLeft);
                    var valueRight = BlendVariable(rightDigits, rightHex, magicRight);

                    var errLeft = ShiftLeftLogical(magicLeft.AsInt16(), 7).AsByte();
                    var errRight = ShiftLeftLogical(magicRight.AsInt16(), 7).AsByte();

                    var evenBytesLeft = Shuffle(valueLeft, evenBytes);
                    var oddBytesLeft = Shuffle(valueLeft, oddBytes);
                    var evenBytesRight = Shuffle(valueRight, evenBytes);
                    var oddBytesRight = Shuffle(valueRight, oddBytes);

                    evenBytesLeft = ShiftLeftLogical(evenBytesLeft.AsUInt16(), 4).AsByte();
                    evenBytesRight = ShiftLeftLogical(evenBytesRight.AsUInt16(), 4).AsByte();

                    evenBytesLeft = Or(evenBytesLeft, oddBytesLeft);
                    evenBytesRight = Or(evenBytesRight, oddBytesRight);

                    var result = Blend(evenBytesLeft.AsUInt16(), evenBytesRight.AsUInt16(), 0b11110000).AsByte();

                    var validationResultLeft = Or(errLeft, valueLeft);
                    var validationResultRight = Or(errRight, valueRight);

                    leftOk = MoveMask(validationResultLeft);
                    rightOk = MoveMask(validationResultRight);

                    if ((leftOk | rightOk) != 0) goto Err;

                    Store(dest, result);
                    dest += 16;
                }

                srcBytes = src;
                destBytes = dest;
                return true;

            Err:
                if (leftOk != 0)
                {
                    srcBytes = src - 32;
                    destBytes = dest;
                    return false;
                }
                else
                {
                    Debug.Assert(rightOk != 0);
                    srcBytes = src - 16;
                    destBytes = dest;
                    return false;
                }
            }
        }
    }
}
#endif