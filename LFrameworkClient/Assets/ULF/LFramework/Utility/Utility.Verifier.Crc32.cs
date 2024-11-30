namespace LFramework
{
    public static partial class Utility
    {
        public static partial class Verifier
        {
            /// <summary>
            /// CRC32 算法。
            /// </summary>
            private sealed class Crc32
            {
                private const int TableLength = 256;
                private const uint DefaultPolynomial = 0xedb88320;
                private const uint DefaultSeed = 0xffffffff;

                private readonly uint _seed;
                private readonly uint[] _table;
                private uint _hash;

                public Crc32()
                    : this(DefaultPolynomial, DefaultSeed)
                {
                }

                public Crc32(uint polynomial, uint seed)
                {
                    _seed = seed;
                    _table = InitializeTable(polynomial);
                    _hash = seed;
                }

                public void Initialize()
                {
                    _hash = _seed;
                }

                public void HashCore(byte[] bytes, int offset, int length)
                {
                    _hash = CalculateHash(_table, _hash, bytes, offset, length);
                }

                public uint HashFinal()
                {
                    return ~_hash;
                }

                private static uint CalculateHash(uint[] table, uint value, byte[] bytes, int offset, int length)
                {
                    int last = offset + length;
                    for (int i = offset; i < last; i++)
                    {
                        unchecked
                        {
                            value = (value >> 8) ^ table[bytes[i] ^ value & 0xff];
                        }
                    }

                    return value;
                }

                private static uint[] InitializeTable(uint polynomial)
                {
                    uint[] table = new uint[TableLength];
                    for (int i = 0; i < TableLength; i++)
                    {
                        uint entry = (uint)i;
                        for (int j = 0; j < 8; j++)
                        {
                            if ((entry & 1) == 1)
                            {
                                entry = (entry >> 1) ^ polynomial;
                            }
                            else
                            {
                                entry >>= 1;
                            }
                        }

                        table[i] = entry;
                    }

                    return table;
                }
            }
        }
    }
}
