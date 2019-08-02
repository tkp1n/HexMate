using System.Collections.Generic;
using Xunit;

namespace HexMate.Tests
{
    public class ScalarConstantsTests
    {
        [Fact]
        public void TestUtf8LookupHexUpperTables()
        {
            Assert.Equal(
                ScalarConstants.s_utf8LookupHexUpperLE,
                ScalarConstants.s_utf8LookupHexUpperBE,
                LeBeUShortEquals.Instance
            );
        }

        [Fact]
        public void TestUtf8LookupHexLowerTables()
        {
            Assert.Equal(
                ScalarConstants.s_utf8LookupHexLowerLE,
                ScalarConstants.s_utf8LookupHexLowerBE,
                LeBeUShortEquals.Instance
            );
        }

        [Fact]
        public void TestUtf16LookupHexUpperTables()
        {
            Assert.Equal(
                ScalarConstants.s_utf16LookupHexUpperLE,
                ScalarConstants.s_utf16LookupHexUpperBE,
                LeBeUIntEquals.Instance
            );
        }

        [Fact]
        public void TestUtf16LookupHexLowerTables()
        {
            Assert.Equal(
                ScalarConstants.s_utf16LookupHexLowerLE,
                ScalarConstants.s_utf16LookupHexLowerBE,
                LeBeUIntEquals.Instance
            );
        }

        private class LeBeUShortEquals : EqualityComparer<ushort>
        {
            public static readonly LeBeUShortEquals Instance = new LeBeUShortEquals();

            private LeBeUShortEquals() { }

            public override bool Equals(ushort x, ushort y)
            {
                var lo = y & 0xFF;
                var hi = y >> 8;

                var be = lo << 8 | hi;
                return x == be;
            }

            public override int GetHashCode(ushort obj)
            {
                return Default.GetHashCode(obj);
            }
        }

        private class LeBeUIntEquals : EqualityComparer<uint>
        {
            public static readonly LeBeUIntEquals Instance = new LeBeUIntEquals();

            private LeBeUIntEquals() { }

            public override bool Equals(uint x, uint y)
            {
                var lo = y & 0xFFFF;
                var hi = y >> 16;

                var be = lo << 16 | hi;
                return x == be;
            }

            public override int GetHashCode(uint obj)
            {
                return Default.GetHashCode(obj);
            }
        }
    }
}