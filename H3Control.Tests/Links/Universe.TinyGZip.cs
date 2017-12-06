 
#pragma warning disable 642, 219 
﻿namespace Universe.TinyGZip.InternalImplementation
{
    public sealed class Adler
    {
        private static readonly uint BASE = 65521U;
        private static readonly int NMAX = 5552;

        public static uint Adler32(uint adler, byte[] buf, int index, int len)
        {
            if (buf == null)
                return 1U;
            var num1 = adler & ushort.MaxValue;
            var num2 = adler >> 16 & ushort.MaxValue;
            while (len > 0)
            {
                var num3 = len < NMAX ? len : NMAX;
                len -= num3;
                while (num3 >= 16)
                {
                    var num4 = num1 + buf[index++];
                    var num5 = num2 + num4;
                    var num6 = num4 + buf[index++];
                    var num7 = num5 + num6;
                    var num8 = num6 + buf[index++];
                    var num9 = num7 + num8;
                    var num10 = num8 + buf[index++];
                    var num11 = num9 + num10;
                    var num12 = num10 + buf[index++];
                    var num13 = num11 + num12;
                    var num14 = num12 + buf[index++];
                    var num15 = num13 + num14;
                    var num16 = num14 + buf[index++];
                    var num17 = num15 + num16;
                    var num18 = num16 + buf[index++];
                    var num19 = num17 + num18;
                    var num20 = num18 + buf[index++];
                    var num21 = num19 + num20;
                    var num22 = num20 + buf[index++];
                    var num23 = num21 + num22;
                    var num24 = num22 + buf[index++];
                    var num25 = num23 + num24;
                    var num26 = num24 + buf[index++];
                    var num27 = num25 + num26;
                    var num28 = num26 + buf[index++];
                    var num29 = num27 + num28;
                    var num30 = num28 + buf[index++];
                    var num31 = num29 + num30;
                    var num32 = num30 + buf[index++];
                    var num33 = num31 + num32;
                    num1 = num32 + buf[index++];
                    num2 = num33 + num1;
                    num3 -= 16;
                }
                if (num3 != 0)
                {
                    do
                    {
                        num1 += buf[index++];
                        num2 += num1;
                    } while (--num3 != 0);
                }
                num1 %= BASE;
                num2 %= BASE;
            }
            return num2 << 16 | num1;
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    internal enum BlockState
    {
        NeedMore,
        BlockDone,
        FinishStarted,
        FinishDone
    }
} 
 

namespace Universe.TinyGZip
{
    public enum CompressionLevel
    {
        Level0 = 0,
        None = 0,
        BestSpeed = 1,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Level5 = 5,
        Default = 6,
        Level6 = 6,
        Level7 = 7,
        Level8 = 8,
        BestCompression = 9,
        Level9 = 9
    }
} 
 

namespace Universe.TinyGZip
{
    public enum CompressionMode
    {
        Compress,
        Decompress
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    public enum CompressionStrategy
    {
        Default,
        Filtered,
        HuffmanOnly
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    [Guid("ebc25cf6-9120-4283-b972-0e5520d0000C")]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CRC32
    {
        private const int BUFFER_SIZE = 8192;
        private uint _register = uint.MaxValue;
        private uint[] crc32Table;
        private readonly uint dwPolynomial;
        private readonly bool reverseBits;

        public CRC32()
            : this(false)
        {
        }

        public CRC32(bool reverseBits)
            : this(-306674912, reverseBits)
        {
        }

        public CRC32(int polynomial, bool reverseBits)
        {
            this.reverseBits = reverseBits;
            dwPolynomial = (uint) polynomial;
            GenerateLookupTable();
        }

        public long TotalBytesRead { get; private set; }

        public int Crc32Result
        {
            get { return ~(int) _register; }
        }

        public int GetCrc32(Stream input)
        {
            return GetCrc32AndCopy(input, null);
        }

        public int GetCrc32AndCopy(Stream input, Stream output)
        {
            if (input == null)
                throw new Exception("The input stream must not be null.");
            var numArray = new byte[8192];
            var count1 = 8192;
            TotalBytesRead = 0L;
            var count2 = input.Read(numArray, 0, count1);
            if (output != null)
                output.Write(numArray, 0, count2);
            TotalBytesRead += count2;
            while (count2 > 0)
            {
                SlurpBlock(numArray, 0, count2);
                count2 = input.Read(numArray, 0, count1);
                if (output != null)
                    output.Write(numArray, 0, count2);
                TotalBytesRead += count2;
            }
            return ~(int) _register;
        }

        public int ComputeCrc32(int W, byte B)
        {
            return _InternalComputeCrc32((uint) W, B);
        }

        internal int _InternalComputeCrc32(uint W, byte B)
        {
            return (int) (crc32Table[(W ^ B) & 0xFF] ^ (W >> 8));
        }

        public void SlurpBlock(byte[] block, int offset, int count)
        {
            if (block == null)
                throw new Exception("The data buffer must not be null.");

            for (var i = 0; i < count; i++)
            {
                var x = offset + i;
                var b = block[x];
                if (reverseBits)
                {
                    var temp = (_register >> 24) ^ b;
                    _register = (_register << 8) ^ crc32Table[temp];
                }
                else
                {
                    var temp = (_register & 0x000000FF) ^ b;
                    _register = (_register >> 8) ^ crc32Table[temp];
                }
            }
            TotalBytesRead += count;
        }

        public void UpdateCRC(byte b)
        {
            if (reverseBits)
            {
                var temp = (_register >> 24) ^ b;
                _register = (_register << 8) ^ crc32Table[temp];
            }
            else
            {
                var temp = (_register & 0x000000FF) ^ b;
                _register = (_register >> 8) ^ crc32Table[temp];
            }
        }

        public void UpdateCRC(byte b, int n)
        {
            while (n-- > 0)
            {
                if (reverseBits)
                {
                    var temp = (_register >> 24) ^ b;
                    _register = (_register << 8) ^ crc32Table[(temp >= 0)
                        ? temp
                        : (temp + 256)];
                }
                else
                {
                    var temp = (_register & 0x000000FF) ^ b;
                    _register = (_register >> 8) ^ crc32Table[(temp >= 0)
                        ? temp
                        : (temp + 256)];
                }
            }
        }

        private static uint ReverseBits(uint data)
        {
            var num1 = data;
            var num2 = (uint) (((int) num1 & 1431655765) << 1 | (int) (num1 >> 1) & 1431655765);
            var num3 = (uint) (((int) num2 & 858993459) << 2 | (int) (num2 >> 2) & 858993459);
            var num4 = (uint) (((int) num3 & 252645135) << 4 | (int) (num3 >> 4) & 252645135);
            return (uint) ((int) num4 << 24 | ((int) num4 & 65280) << 8 | (int) (num4 >> 8) & 65280) | num4 >> 24;
        }

        private static byte ReverseBits(byte data)
        {
            var num1 = data*131586U;
            var num2 = 17055760U;
            return (byte) ((uint) (16781313*((int) (num1 & num2) + ((int) num1 << 2 & (int) num2 << 1))) >> 24);
        }

        private void GenerateLookupTable()
        {
            crc32Table = new uint[256];
            byte data1 = 0;
            do
            {
                uint data2 = data1;
                for (byte index = 8; (int) index > 0; --index)
                {
                    if (((int) data2 & 1) == 1)
                        data2 = data2 >> 1 ^ dwPolynomial;
                    else
                        data2 >>= 1;
                }
                if (reverseBits)
                    crc32Table[ReverseBits(data1)] = ReverseBits(data2);
                else
                    crc32Table[data1] = data2;
                ++data1;
            } while (data1 != 0);
        }

        private uint gf2_matrix_times(uint[] matrix, uint vec)
        {
            var num = 0U;
            var index = 0;
            while ((int) vec != 0)
            {
                if (((int) vec & 1) == 1)
                    num ^= matrix[index];
                vec >>= 1;
                ++index;
            }
            return num;
        }

        private void gf2_matrix_square(uint[] square, uint[] mat)
        {
            for (var index = 0; index < 32; ++index)
                square[index] = gf2_matrix_times(mat, mat[index]);
        }

        public void Combine(int crc, int length)
        {
            var numArray1 = new uint[32];
            var numArray2 = new uint[32];
            if (length == 0)
                return;
            var vec = ~_register;
            var num1 = (uint) crc;
            numArray2[0] = dwPolynomial;
            var num2 = 1U;
            for (var index = 1; index < 32; ++index)
            {
                numArray2[index] = num2;
                num2 <<= 1;
            }
            gf2_matrix_square(numArray1, numArray2);
            gf2_matrix_square(numArray2, numArray1);
            var num3 = (uint) length;
            do
            {
                gf2_matrix_square(numArray1, numArray2);
                if (((int) num3 & 1) == 1)
                    vec = gf2_matrix_times(numArray1, vec);
                var num4 = num3 >> 1;
                if ((int) num4 != 0)
                {
                    gf2_matrix_square(numArray2, numArray1);
                    if (((int) num4 & 1) == 1)
                        vec = gf2_matrix_times(numArray2, vec);
                    num3 = num4 >> 1;
                }
                else
                    break;
            } while ((int) num3 != 0);
            _register = ~(vec ^ num1);
        }

        public void Reset()
        {
            _register = uint.MaxValue;
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    using System;
    using System.IO;

    public class CrcCalculatorStream : Stream, IDisposable
    {
        private static readonly long UnsetLengthLimit = -99L;
        private readonly CRC32 _Crc32;
        internal Stream _innerStream;
        private readonly long _lengthLimit = -99L;

        public CrcCalculatorStream(Stream stream)
            : this(true, UnsetLengthLimit, stream, null)
        {
        }

        public CrcCalculatorStream(Stream stream, bool leaveOpen)
            : this(leaveOpen, UnsetLengthLimit, stream, null)
        {
        }

        public CrcCalculatorStream(Stream stream, long length)
            : this(true, length, stream, null)
        {
            if (length < 0L)
                throw new ArgumentException("length");
        }

        public CrcCalculatorStream(Stream stream, long length, bool leaveOpen)
            : this(leaveOpen, length, stream, null)
        {
            if (length < 0L)
                throw new ArgumentException("length");
        }

        public CrcCalculatorStream(Stream stream, long length, bool leaveOpen, CRC32 crc32)
            : this(leaveOpen, length, stream, crc32)
        {
            if (length < 0L)
                throw new ArgumentException("length");
        }

        private CrcCalculatorStream(bool leaveOpen, long length, Stream stream, CRC32 crc32)
        {
            _innerStream = stream;
            _Crc32 = crc32 ?? new CRC32();
            _lengthLimit = length;
            LeaveOpen = leaveOpen;
        }

        public long TotalBytesSlurped
        {
            get { return _Crc32.TotalBytesRead; }
        }

        public int Crc
        {
            get { return _Crc32.Crc32Result; }
        }

        public bool LeaveOpen { get; set; }

        public override bool CanRead
        {
            get { return _innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return _innerStream.CanWrite; }
        }

        public override long Length
        {
            get
            {
                if (_lengthLimit == UnsetLengthLimit)
                    return _innerStream.Length;
                return _lengthLimit;
            }
        }

        public override long Position
        {
            get { return _Crc32.TotalBytesRead; }
            set { throw new NotSupportedException(); }
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var count1 = count;
            if (_lengthLimit != UnsetLengthLimit)
            {
                if (_Crc32.TotalBytesRead >= _lengthLimit)
                    return 0;
                var num = _lengthLimit - _Crc32.TotalBytesRead;
                if (num < count)
                    count1 = (int) num;
            }
            var count2 = _innerStream.Read(buffer, offset, count1);
            if (count2 > 0)
                _Crc32.SlurpBlock(buffer, offset, count2);
            return count2;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (count > 0)
                _Crc32.SlurpBlock(buffer, offset, count);
            _innerStream.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            base.Close();
            if (LeaveOpen)
                return;
            _innerStream.Close();
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    using System;

    #pragma warning disable 642, 219
    internal sealed class DeflateManager
    {
        private static readonly int MEM_LEVEL_MAX = 9;
        private static readonly int MEM_LEVEL_DEFAULT = 8;

        private static readonly string[] _ErrorMessage = new string[10]
        {
            "need dictionary",
            "stream end",
            "",
            "file error",
            "stream error",
            "data error",
            "insufficient memory",
            "buffer error",
            "incompatible version",
            ""
        };

        private static readonly int PRESET_DICT = 32;
        private static readonly int INIT_STATE = 42;
        private static readonly int BUSY_STATE = 113;
        private static readonly int FINISH_STATE = 666;
        private static readonly int Z_DEFLATED = 8;
        private static readonly int STORED_BLOCK = 0;
        private static readonly int STATIC_TREES = 1;
        private static readonly int DYN_TREES = 2;
        private static readonly int Z_BINARY = 0;
        private static readonly int Z_ASCII = 1;
        private static readonly int Z_UNKNOWN = 2;
        private static readonly int Buf_size = 16;
        private static readonly int MIN_MATCH = 3;
        private static readonly int MAX_MATCH = 258;
        private static readonly int MIN_LOOKAHEAD = MAX_MATCH + MIN_MATCH + 1;
        private static readonly int HEAP_SIZE = 2*InternalConstants.L_CODES + 1;
        private static readonly int END_BLOCK = 256;
        internal ZlibCodec _codec;
        internal int _distanceOffset;
        internal int _lengthOffset;
        private bool _WantRfc1950HeaderBytes = true;
        internal short bi_buf;
        internal int bi_valid;
        internal short[] bl_count = new short[InternalConstants.MAX_BITS + 1];
        internal short[] bl_tree;
        internal int block_start;
        internal CompressionLevel compressionLevel;
        internal CompressionStrategy compressionStrategy;
        private Config config;
        internal sbyte data_type;
        private CompressFunc DeflateFunction;
        internal sbyte[] depth = new sbyte[2*InternalConstants.L_CODES + 1];
        internal short[] dyn_dtree;
        internal short[] dyn_ltree;
        internal int hash_bits;
        internal int hash_mask;
        internal int hash_shift;
        internal int hash_size;
        internal short[] head;
        internal int[] heap = new int[2*InternalConstants.L_CODES + 1];
        internal int heap_len;
        internal int heap_max;
        internal int ins_h;
        internal int last_eob_len;
        internal int last_flush;
        internal int last_lit;
        internal int lit_bufsize;
        internal int lookahead;
        internal int match_available;
        internal int match_length;
        internal int match_start;
        internal int matches;
        internal int nextPending;
        internal int opt_len;
        internal byte[] pending;
        internal int pendingCount;
        internal short[] prev;
        internal int prev_length;
        internal int prev_match;
        private bool Rfc1950BytesEmitted;
        internal int static_len;
        internal int status;
        internal int strstart;
        internal Tree treeBitLengths = new Tree();
        internal Tree treeDistances = new Tree();
        internal Tree treeLiterals = new Tree();
        internal int w_bits;
        internal int w_mask;
        internal int w_size;
        internal byte[] window;
        internal int window_size;

        internal DeflateManager()
        {
            dyn_ltree = new short[HEAP_SIZE*2];
            dyn_dtree = new short[(2*InternalConstants.D_CODES + 1)*2];
            bl_tree = new short[(2*InternalConstants.BL_CODES + 1)*2];
        }

        internal bool WantRfc1950HeaderBytes
        {
            get { return _WantRfc1950HeaderBytes; }
            set { _WantRfc1950HeaderBytes = value; }
        }

        private void _InitializeLazyMatch()
        {
            window_size = 2*w_size;
            Array.Clear(head, 0, hash_size);
            config = Config.Lookup(compressionLevel);
            SetDeflater();
            strstart = 0;
            block_start = 0;
            lookahead = 0;
            match_length = prev_length = MIN_MATCH - 1;
            match_available = 0;
            ins_h = 0;
        }

        private void _InitializeTreeData()
        {
            treeLiterals.dyn_tree = dyn_ltree;
            treeLiterals.staticTree = StaticTree.Literals;
            treeDistances.dyn_tree = dyn_dtree;
            treeDistances.staticTree = StaticTree.Distances;
            treeBitLengths.dyn_tree = bl_tree;
            treeBitLengths.staticTree = StaticTree.BitLengths;
            bi_buf = 0;
            bi_valid = 0;
            last_eob_len = 8;
            _InitializeBlocks();
        }

        internal void _InitializeBlocks()
        {
            for (var index = 0; index < InternalConstants.L_CODES; ++index)
                dyn_ltree[index*2] = 0;
            for (var index = 0; index < InternalConstants.D_CODES; ++index)
                dyn_dtree[index*2] = 0;
            for (var index = 0; index < InternalConstants.BL_CODES; ++index)
                bl_tree[index*2] = 0;
            dyn_ltree[END_BLOCK*2] = 1;
            opt_len = static_len = 0;
            last_lit = matches = 0;
        }

        internal void pqdownheap(short[] tree, int k)
        {
            var n = heap[k];
            var index = k << 1;
            while (index <= heap_len)
            {
                if (index < heap_len && _IsSmaller(tree, heap[index + 1], heap[index], depth))
                    ++index;
                if (!_IsSmaller(tree, n, heap[index], depth))
                {
                    heap[k] = heap[index];
                    k = index;
                    index <<= 1;
                }
                else
                    break;
            }
            heap[k] = n;
        }

        internal static bool _IsSmaller(short[] tree, int n, int m, sbyte[] depth)
        {
            var num1 = tree[n*2];
            var num2 = tree[m*2];
            return num1 < num2 || num1 == num2 && depth[n] <= depth[m];
        }

        internal void scan_tree(short[] tree, int max_code)
        {
            var num1 = -1;
            int num2 = tree[1];
            var num3 = 0;
            var num4 = 7;
            var num5 = 4;
            if (num2 == 0)
            {
                num4 = 138;
                num5 = 3;
            }
            tree[(max_code + 1)*2 + 1] = short.MaxValue;
            for (var index = 0; index <= max_code; ++index)
            {
                var num6 = num2;
                num2 = tree[(index + 1)*2 + 1];
                if (++num3 >= num4 || num6 != num2)
                {
                    if (num3 < num5)
                        bl_tree[num6*2] = (short) (bl_tree[num6*2] + num3);
                    else if (num6 != 0)
                    {
                        if (num6 != num1)
                            ++bl_tree[num6*2];
                        ++bl_tree[InternalConstants.REP_3_6*2];
                    }
                    else if (num3 <= 10)
                        ++bl_tree[InternalConstants.REPZ_3_10*2];
                    else
                        ++bl_tree[InternalConstants.REPZ_11_138*2];
                    num3 = 0;
                    num1 = num6;
                    if (num2 == 0)
                    {
                        num4 = 138;
                        num5 = 3;
                    }
                    else if (num6 == num2)
                    {
                        num4 = 6;
                        num5 = 3;
                    }
                    else
                    {
                        num4 = 7;
                        num5 = 4;
                    }
                }
            }
        }

        internal int build_bl_tree()
        {
            scan_tree(dyn_ltree, treeLiterals.max_code);
            scan_tree(dyn_dtree, treeDistances.max_code);
            treeBitLengths.build_tree(this);
            var index = InternalConstants.BL_CODES - 1;
            while (index >= 3 && bl_tree[Tree.bl_order[index]*2 + 1] == 0)
                --index;
            opt_len += 3*(index + 1) + 5 + 5 + 4;
            return index;
        }

        internal void send_all_trees(int lcodes, int dcodes, int blcodes)
        {
            send_bits(lcodes - 257, 5);
            send_bits(dcodes - 1, 5);
            send_bits(blcodes - 4, 4);
            for (var index = 0; index < blcodes; ++index)
                send_bits(bl_tree[Tree.bl_order[index]*2 + 1], 3);
            send_tree(dyn_ltree, lcodes - 1);
            send_tree(dyn_dtree, dcodes - 1);
        }

        internal void send_tree(short[] tree, int max_code)
        {
            var num1 = -1;
            int num2 = tree[1];
            var num3 = 0;
            var num4 = 7;
            var num5 = 4;
            if (num2 == 0)
            {
                num4 = 138;
                num5 = 3;
            }
            for (var index = 0; index <= max_code; ++index)
            {
                var c = num2;
                num2 = tree[(index + 1)*2 + 1];
                if (++num3 >= num4 || c != num2)
                {
                    if (num3 < num5)
                    {
                        do
                        {
                            send_code(c, bl_tree);
                        } while (--num3 != 0);
                    }
                    else if (c != 0)
                    {
                        if (c != num1)
                        {
                            send_code(c, bl_tree);
                            --num3;
                        }
                        send_code(InternalConstants.REP_3_6, bl_tree);
                        send_bits(num3 - 3, 2);
                    }
                    else if (num3 <= 10)
                    {
                        send_code(InternalConstants.REPZ_3_10, bl_tree);
                        send_bits(num3 - 3, 3);
                    }
                    else
                    {
                        send_code(InternalConstants.REPZ_11_138, bl_tree);
                        send_bits(num3 - 11, 7);
                    }
                    num3 = 0;
                    num1 = c;
                    if (num2 == 0)
                    {
                        num4 = 138;
                        num5 = 3;
                    }
                    else if (c == num2)
                    {
                        num4 = 6;
                        num5 = 3;
                    }
                    else
                    {
                        num4 = 7;
                        num5 = 4;
                    }
                }
            }
        }

        private void put_bytes(byte[] p, int start, int len)
        {
            Array.Copy(p, start, pending, pendingCount, len);
            pendingCount += len;
        }

        internal void send_code(int c, short[] tree)
        {
            var index = c*2;
            send_bits(tree[index] & ushort.MaxValue, tree[index + 1] & ushort.MaxValue);
        }

        internal void send_bits(int value, int length)
        {
            var num = length;
            if (bi_valid > Buf_size - num)
            {
                bi_buf |= (short) (value << bi_valid & ushort.MaxValue);
                pending[pendingCount++] = (byte) bi_buf;
                pending[pendingCount++] = (byte) ((uint) bi_buf >> 8);
                bi_buf = (short) ((uint) value >> Buf_size - bi_valid);
                bi_valid += num - Buf_size;
            }
            else
            {
                bi_buf |= (short) (value << bi_valid & ushort.MaxValue);
                bi_valid += num;
            }
        }

        internal void _tr_align()
        {
            send_bits(STATIC_TREES << 1, 3);
            send_code(END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
            bi_flush();
            if (1 + last_eob_len + 10 - bi_valid < 9)
            {
                send_bits(STATIC_TREES << 1, 3);
                send_code(END_BLOCK, StaticTree.lengthAndLiteralsTreeCodes);
                bi_flush();
            }
            last_eob_len = 7;
        }

        internal bool _tr_tally(int dist, int lc)
        {
            pending[_distanceOffset + last_lit*2] = (byte) ((uint) dist >> 8);
            pending[_distanceOffset + last_lit*2 + 1] = (byte) dist;
            pending[_lengthOffset + last_lit] = (byte) lc;
            ++last_lit;
            if (dist == 0)
            {
                ++dyn_ltree[lc*2];
            }
            else
            {
                ++matches;
                --dist;
                ++dyn_ltree[(Tree.LengthCode[lc] + InternalConstants.LITERALS + 1)*2];
                ++dyn_dtree[Tree.DistanceCode(dist)*2];
            }
            if ((last_lit & 8191) == 0 && compressionLevel > CompressionLevel.Level2)
            {
                var num1 = last_lit << 3;
                var num2 = strstart - block_start;
                for (var index = 0; index < InternalConstants.D_CODES; ++index)
                    num1 = (int) (num1 + dyn_dtree[index*2]*(5L + Tree.ExtraDistanceBits[index]));
                if (matches < last_lit/2 && num1 >> 3 < num2/2)
                    return true;
            }
            return last_lit == lit_bufsize - 1 || last_lit == lit_bufsize;
        }

        internal void send_compressed_block(short[] ltree, short[] dtree)
        {
            var num1 = 0;
            if (last_lit != 0)
            {
                do
                {
                    var index1 = _distanceOffset + num1*2;
                    var num2 = pending[index1] << 8 & 65280 | pending[index1 + 1] & byte.MaxValue;
                    var c1 = pending[_lengthOffset + num1] & byte.MaxValue;
                    ++num1;
                    if (num2 == 0)
                    {
                        send_code(c1, ltree);
                    }
                    else
                    {
                        int index2 = Tree.LengthCode[c1];
                        send_code(index2 + InternalConstants.LITERALS + 1, ltree);
                        var length1 = Tree.ExtraLengthBits[index2];
                        if (length1 != 0)
                            send_bits(c1 - Tree.LengthBase[index2], length1);
                        var dist = num2 - 1;
                        var c2 = Tree.DistanceCode(dist);
                        send_code(c2, dtree);
                        var length2 = Tree.ExtraDistanceBits[c2];
                        if (length2 != 0)
                            send_bits(dist - Tree.DistanceBase[c2], length2);
                    }
                } while (num1 < last_lit);
            }
            send_code(END_BLOCK, ltree);
            last_eob_len = ltree[END_BLOCK*2 + 1];
        }

        internal void set_data_type()
        {
            var num1 = 0;
            var num2 = 0;
            var num3 = 0;
            for (; num1 < 7; ++num1)
                num3 += dyn_ltree[num1*2];
            for (; num1 < 128; ++num1)
                num2 += dyn_ltree[num1*2];
            for (; num1 < InternalConstants.LITERALS; ++num1)
                num3 += dyn_ltree[num1*2];
            data_type = num3 > num2 >> 2 ? (sbyte) Z_BINARY : (sbyte) Z_ASCII;
        }

        internal void bi_flush()
        {
            if (bi_valid == 16)
            {
                pending[pendingCount++] = (byte) bi_buf;
                pending[pendingCount++] = (byte) ((uint) bi_buf >> 8);
                bi_buf = 0;
                bi_valid = 0;
            }
            else
            {
                if (bi_valid < 8)
                    return;
                pending[pendingCount++] = (byte) bi_buf;
                bi_buf >>= 8;
                bi_valid -= 8;
            }
        }

        internal void bi_windup()
        {
            if (bi_valid > 8)
            {
                pending[pendingCount++] = (byte) bi_buf;
                pending[pendingCount++] = (byte) ((uint) bi_buf >> 8);
            }
            else if (bi_valid > 0)
                pending[pendingCount++] = (byte) bi_buf;
            bi_buf = 0;
            bi_valid = 0;
        }

        internal void copy_block(int buf, int len, bool header)
        {
            bi_windup();
            last_eob_len = 8;
            if (header)
            {
                pending[pendingCount++] = (byte) len;
                pending[pendingCount++] = (byte) (len >> 8);
                pending[pendingCount++] = (byte) ~len;
                pending[pendingCount++] = (byte) (~len >> 8);
            }
            put_bytes(window, buf, len);
        }

        internal void flush_block_only(bool eof)
        {
            _tr_flush_block(block_start >= 0 ? block_start : -1, strstart - block_start, eof);
            block_start = strstart;
            _codec.flush_pending();
        }

        internal BlockState DeflateNone(FlushType flush)
        {
            int num1 = ushort.MaxValue;
            if (num1 > pending.Length - 5)
                num1 = pending.Length - 5;
            while (true)
            {
                if (lookahead <= 1)
                {
                    _fillWindow();
                    if (lookahead != 0 || flush != FlushType.None)
                    {
                        if (lookahead == 0)
                            goto label_13;
                    }
                    else
                        break;
                }
                strstart += lookahead;
                lookahead = 0;
                var num2 = block_start + num1;
                if (strstart == 0 || strstart >= num2)
                {
                    lookahead = strstart - num2;
                    strstart = num2;
                    flush_block_only(false);
                    if (_codec.AvailableBytesOut == 0)
                        goto label_7;
                }
                if (strstart - block_start >= w_size - MIN_LOOKAHEAD)
                {
                    flush_block_only(false);
                    if (_codec.AvailableBytesOut == 0)
                        goto label_10;
                }
            }
            return BlockState.NeedMore;
            label_7:
            return BlockState.NeedMore;
            label_10:
            return BlockState.NeedMore;
            label_13:
            flush_block_only(flush == FlushType.Finish);
            if (_codec.AvailableBytesOut == 0)
                return flush == FlushType.Finish ? BlockState.FinishStarted : BlockState.NeedMore;
            return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
        }

        internal void _tr_stored_block(int buf, int stored_len, bool eof)
        {
            send_bits((STORED_BLOCK << 1) + (eof ? 1 : 0), 3);
            copy_block(buf, stored_len, true);
        }

        internal void _tr_flush_block(int buf, int stored_len, bool eof)
        {
            var num1 = 0;
            int num2;
            int num3;
            if (compressionLevel > CompressionLevel.None)
            {
                if (data_type == Z_UNKNOWN)
                    set_data_type();
                treeLiterals.build_tree(this);
                treeDistances.build_tree(this);
                num1 = build_bl_tree();
                num2 = opt_len + 3 + 7 >> 3;
                num3 = static_len + 3 + 7 >> 3;
                if (num3 <= num2)
                    num2 = num3;
            }
            else
                num2 = num3 = stored_len + 5;
            if (stored_len + 4 <= num2 && buf != -1)
                _tr_stored_block(buf, stored_len, eof);
            else if (num3 == num2)
            {
                send_bits((STATIC_TREES << 1) + (eof ? 1 : 0), 3);
                send_compressed_block(StaticTree.lengthAndLiteralsTreeCodes, StaticTree.distTreeCodes);
            }
            else
            {
                send_bits((DYN_TREES << 1) + (eof ? 1 : 0), 3);
                send_all_trees(treeLiterals.max_code + 1, treeDistances.max_code + 1, num1 + 1);
                send_compressed_block(dyn_ltree, dyn_dtree);
            }
            _InitializeBlocks();
            if (!eof)
                return;
            bi_windup();
        }

        private void _fillWindow()
        {
            do
            {
                var size = window_size - lookahead - strstart;
                if (size == 0 && strstart == 0 && lookahead == 0)
                    size = w_size;
                else if (size == -1)
                    --size;
                else if (strstart >= w_size + w_size - MIN_LOOKAHEAD)
                {
                    Array.Copy(window, w_size, window, 0, w_size);
                    match_start -= w_size;
                    strstart -= w_size;
                    block_start -= w_size;
                    var num1 = hash_size;
                    var index1 = num1;
                    do
                    {
                        var num2 = head[--index1] & ushort.MaxValue;
                        head[index1] = num2 >= w_size ? (short) (num2 - w_size) : (short) 0;
                    } while (--num1 != 0);
                    var num3 = w_size;
                    var index2 = num3;
                    do
                    {
                        var num2 = prev[--index2] & ushort.MaxValue;
                        prev[index2] = num2 >= w_size ? (short) (num2 - w_size) : (short) 0;
                    } while (--num3 != 0);
                    size += w_size;
                }
                if (_codec.AvailableBytesIn != 0)
                {
                    lookahead += _codec.read_buf(window, strstart + lookahead, size);
                    if (lookahead >= MIN_MATCH)
                    {
                        ins_h = window[strstart] & byte.MaxValue;
                        ins_h = (ins_h << hash_shift ^ window[strstart + 1] & byte.MaxValue) & hash_mask;
                    }
                }
                else
                    goto label_16;
            } while (lookahead < MIN_LOOKAHEAD && _codec.AvailableBytesIn != 0);
            goto label_12;
            label_16:
            return;
            label_12:
            ;
        }

        internal BlockState DeflateFast(FlushType flush)
        {
            var cur_match = 0;
            while (true)
            {
                if (lookahead < MIN_LOOKAHEAD)
                {
                    _fillWindow();
                    if (lookahead >= MIN_LOOKAHEAD || flush != FlushType.None)
                    {
                        if (lookahead == 0)
                            goto label_20;
                    }
                    else
                        break;
                }
                if (lookahead >= MIN_MATCH)
                {
                    ins_h = (ins_h << hash_shift ^ window[strstart + (MIN_MATCH - 1)] & byte.MaxValue) & hash_mask;
                    cur_match = head[ins_h] & ushort.MaxValue;
                    prev[strstart & w_mask] = head[ins_h];
                    head[ins_h] = (short) strstart;
                }
                if (cur_match != 0L && (strstart - cur_match & ushort.MaxValue) <= w_size - MIN_LOOKAHEAD &&
                    compressionStrategy != CompressionStrategy.HuffmanOnly)
                    match_length = longest_match(cur_match);
                bool flag;
                if (match_length >= MIN_MATCH)
                {
                    flag = _tr_tally(strstart - match_start, match_length - MIN_MATCH);
                    lookahead -= match_length;
                    if (match_length <= config.MaxLazy && lookahead >= MIN_MATCH)
                    {
                        --match_length;
                        do
                        {
                            ++strstart;
                            ins_h = (ins_h << hash_shift ^ window[strstart + (MIN_MATCH - 1)] & byte.MaxValue) & hash_mask;
                            cur_match = head[ins_h] & ushort.MaxValue;
                            prev[strstart & w_mask] = head[ins_h];
                            head[ins_h] = (short) strstart;
                        } while (--match_length != 0);
                        ++strstart;
                    }
                    else
                    {
                        strstart += match_length;
                        match_length = 0;
                        ins_h = window[strstart] & byte.MaxValue;
                        ins_h = (ins_h << hash_shift ^ window[strstart + 1] & byte.MaxValue) & hash_mask;
                    }
                }
                else
                {
                    flag = _tr_tally(0, window[strstart] & byte.MaxValue);
                    --lookahead;
                    ++strstart;
                }
                if (flag)
                {
                    flush_block_only(false);
                    if (_codec.AvailableBytesOut == 0)
                        goto label_17;
                }
            }
            return BlockState.NeedMore;
            label_17:
            return BlockState.NeedMore;
            label_20:
            flush_block_only(flush == FlushType.Finish);
            if (_codec.AvailableBytesOut != 0)
                return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
            return flush == FlushType.Finish ? BlockState.FinishStarted : BlockState.NeedMore;
        }

        internal BlockState DeflateSlow(FlushType flush)
        {
            var cur_match = 0;
            while (true)
            {
                if (lookahead < MIN_LOOKAHEAD)
                {
                    _fillWindow();
                    if (lookahead >= MIN_LOOKAHEAD || flush != FlushType.None)
                    {
                        if (lookahead == 0)
                            goto label_28;
                    }
                    else
                        break;
                }
                if (lookahead >= MIN_MATCH)
                {
                    ins_h = (ins_h << hash_shift ^ window[strstart + (MIN_MATCH - 1)] & byte.MaxValue) & hash_mask;
                    cur_match = head[ins_h] & ushort.MaxValue;
                    prev[strstart & w_mask] = head[ins_h];
                    head[ins_h] = (short) strstart;
                }
                prev_length = match_length;
                prev_match = match_start;
                match_length = MIN_MATCH - 1;
                if (cur_match != 0 && prev_length < config.MaxLazy && (strstart - cur_match & ushort.MaxValue) <= w_size - MIN_LOOKAHEAD)
                {
                    if (compressionStrategy != CompressionStrategy.HuffmanOnly)
                        match_length = longest_match(cur_match);
                    if (match_length <= 5 &&
                        (compressionStrategy == CompressionStrategy.Filtered || match_length == MIN_MATCH && strstart - match_start > 4096))
                        match_length = MIN_MATCH - 1;
                }
                if (prev_length >= MIN_MATCH && match_length <= prev_length)
                {
                    var num = strstart + lookahead - MIN_MATCH;
                    var flag = _tr_tally(strstart - 1 - prev_match, prev_length - MIN_MATCH);
                    lookahead -= prev_length - 1;
                    prev_length -= 2;
                    do
                    {
                        if (++strstart <= num)
                        {
                            ins_h = (ins_h << hash_shift ^ window[strstart + (MIN_MATCH - 1)] & byte.MaxValue) & hash_mask;
                            cur_match = head[ins_h] & ushort.MaxValue;
                            prev[strstart & w_mask] = head[ins_h];
                            head[ins_h] = (short) strstart;
                        }
                    } while (--prev_length != 0);
                    match_available = 0;
                    match_length = MIN_MATCH - 1;
                    ++strstart;
                    if (flag)
                    {
                        flush_block_only(false);
                        if (_codec.AvailableBytesOut == 0)
                            goto label_19;
                    }
                }
                else if (match_available != 0)
                {
                    if (_tr_tally(0, window[strstart - 1] & byte.MaxValue))
                        flush_block_only(false);
                    ++strstart;
                    --lookahead;
                    if (_codec.AvailableBytesOut == 0)
                        goto label_24;
                }
                else
                {
                    match_available = 1;
                    ++strstart;
                    --lookahead;
                }
            }
            return BlockState.NeedMore;
            label_19:
            return BlockState.NeedMore;
            label_24:
            return BlockState.NeedMore;
            label_28:
            if (match_available != 0)
            {
                _tr_tally(0, window[strstart - 1] & byte.MaxValue);
                match_available = 0;
            }
            flush_block_only(flush == FlushType.Finish);
            if (_codec.AvailableBytesOut != 0)
                return flush == FlushType.Finish ? BlockState.FinishDone : BlockState.BlockDone;
            return flush == FlushType.Finish ? BlockState.FinishStarted : BlockState.NeedMore;
        }

        internal int longest_match(int cur_match)
        {
            var num1 = config.MaxChainLength;
            var index1 = strstart;
            var num2 = prev_length;
            var num3 = strstart > w_size - MIN_LOOKAHEAD ? strstart - (w_size - MIN_LOOKAHEAD) : 0;
            var num4 = config.NiceLength;
            var num5 = w_mask;
            var num6 = strstart + MAX_MATCH;
            var num7 = window[index1 + num2 - 1];
            var num8 = window[index1 + num2];
            if (prev_length >= config.GoodLength)
                num1 >>= 2;
            if (num4 > lookahead)
                num4 = lookahead;
            do
            {
                var index2 = cur_match;
                if (window[index2 + num2] == num8 && window[index2 + num2 - 1] == num7 && window[index2] == window[index1] &&
                    window[++index2] == window[index1 + 1])
                {
                    var num9 = index1 + 2;
                    var num10 = index2 + 1;
                    do
                        ; while (window[++num9] == window[++num10] && window[++num9] == window[++num10] &&
                                 (window[++num9] == window[++num10] && window[++num9] == window[++num10]) &&
                                 (window[++num9] == window[++num10] && window[++num9] == window[++num10] &&
                                  (window[++num9] == window[++num10] && window[++num9] == window[++num10])) && num9 < num6);
                    var num11 = MAX_MATCH - (num6 - num9);
                    index1 = num6 - MAX_MATCH;
                    if (num11 > num2)
                    {
                        match_start = cur_match;
                        num2 = num11;
                        if (num11 < num4)
                        {
                            num7 = window[index1 + num2 - 1];
                            num8 = window[index1 + num2];
                        }
                        else
                            break;
                    }
                }
            } while ((cur_match = prev[cur_match & num5] & ushort.MaxValue) > num3 && --num1 != 0);
            if (num2 <= lookahead)
                return num2;
            return lookahead;
        }

        internal int Initialize(ZlibCodec codec, CompressionLevel level)
        {
            return Initialize(codec, level, 15);
        }

        internal int Initialize(ZlibCodec codec, CompressionLevel level, int bits)
        {
            return Initialize(codec, level, bits, MEM_LEVEL_DEFAULT, CompressionStrategy.Default);
        }

        internal int Initialize(ZlibCodec codec, CompressionLevel level, int bits, CompressionStrategy compressionStrategy)
        {
            return Initialize(codec, level, bits, MEM_LEVEL_DEFAULT, compressionStrategy);
        }

        internal int Initialize(ZlibCodec codec, CompressionLevel level, int windowBits, int memLevel, CompressionStrategy strategy)
        {
            _codec = codec;
            _codec.Message = null;
            if (windowBits < 9 || windowBits > 15)
                throw new ZlibException("windowBits must be in the range 9..15.");
            if (memLevel < 1 || memLevel > MEM_LEVEL_MAX)
                throw new ZlibException(string.Format("memLevel must be in the range 1.. {0}", MEM_LEVEL_MAX));
            _codec.dstate = this;
            w_bits = windowBits;
            w_size = 1 << w_bits;
            w_mask = w_size - 1;
            hash_bits = memLevel + 7;
            hash_size = 1 << hash_bits;
            hash_mask = hash_size - 1;
            hash_shift = (hash_bits + MIN_MATCH - 1)/MIN_MATCH;
            window = new byte[w_size*2];
            prev = new short[w_size];
            head = new short[hash_size];
            lit_bufsize = 1 << memLevel + 6;
            pending = new byte[lit_bufsize*4];
            _distanceOffset = lit_bufsize;
            _lengthOffset = 3*lit_bufsize;
            compressionLevel = level;
            compressionStrategy = strategy;
            Reset();
            return 0;
        }

        internal void Reset()
        {
            _codec.TotalBytesIn = _codec.TotalBytesOut = 0L;
            _codec.Message = null;
            pendingCount = 0;
            nextPending = 0;
            Rfc1950BytesEmitted = false;
            status = WantRfc1950HeaderBytes ? INIT_STATE : BUSY_STATE;
            _codec._Adler32 = Adler.Adler32(0U, null, 0, 0);
            last_flush = 0;
            _InitializeTreeData();
            _InitializeLazyMatch();
        }

        internal int End()
        {
            if (status != INIT_STATE && status != BUSY_STATE && status != FINISH_STATE)
                return -2;
            pending = null;
            head = null;
            prev = null;
            window = null;
            return status == BUSY_STATE ? -3 : 0;
        }

        private void SetDeflater()
        {
            switch (config.Flavor)
            {
                case DeflateFlavor.Store:
                    DeflateFunction = DeflateNone;
                    break;
                case DeflateFlavor.Fast:
                    DeflateFunction = DeflateFast;
                    break;
                case DeflateFlavor.Slow:
                    DeflateFunction = DeflateSlow;
                    break;
            }
        }

        internal int SetParams(CompressionLevel level, CompressionStrategy strategy)
        {
            var num = 0;
            if (compressionLevel != level)
            {
                var config = Config.Lookup(level);
                if (config.Flavor != this.config.Flavor && _codec.TotalBytesIn != 0L)
                    num = _codec.Deflate(FlushType.Partial);
                compressionLevel = level;
                this.config = config;
                SetDeflater();
            }
            compressionStrategy = strategy;
            return num;
        }

        internal int SetDictionary(byte[] dictionary)
        {
            var length = dictionary.Length;
            var sourceIndex = 0;
            if (dictionary == null || status != INIT_STATE)
                throw new ZlibException("Stream error.");
            _codec._Adler32 = Adler.Adler32(_codec._Adler32, dictionary, 0, dictionary.Length);
            if (length < MIN_MATCH)
                return 0;
            if (length > w_size - MIN_LOOKAHEAD)
            {
                length = w_size - MIN_LOOKAHEAD;
                sourceIndex = dictionary.Length - length;
            }
            Array.Copy(dictionary, sourceIndex, window, 0, length);
            strstart = length;
            block_start = length;
            ins_h = window[0] & byte.MaxValue;
            ins_h = (ins_h << hash_shift ^ window[1] & byte.MaxValue) & hash_mask;
            for (var index = 0; index <= length - MIN_MATCH; ++index)
            {
                ins_h = (ins_h << hash_shift ^ window[index + (MIN_MATCH - 1)] & byte.MaxValue) & hash_mask;
                prev[index & w_mask] = head[ins_h];
                head[ins_h] = (short) index;
            }
            return 0;
        }

        internal int Deflate(FlushType flush)
        {
            if (_codec.OutputBuffer == null || _codec.InputBuffer == null && _codec.AvailableBytesIn != 0 ||
                status == FINISH_STATE && flush != FlushType.Finish)
            {
                _codec.Message = _ErrorMessage[4];
                throw new ZlibException(string.Format("Something is fishy. [{0}]", _codec.Message));
            }
            if (_codec.AvailableBytesOut == 0)
            {
                _codec.Message = _ErrorMessage[7];
                throw new ZlibException("OutputBuffer is full (AvailableBytesOut == 0)");
            }
            var num1 = last_flush;
            last_flush = (int) flush;
            if (status == INIT_STATE)
            {
                var num2 = Z_DEFLATED + (w_bits - 8 << 4) << 8;
                var num3 = (int) (compressionLevel - 1 & (CompressionLevel) 255) >> 1;
                if (num3 > 3)
                    num3 = 3;
                var num4 = num2 | num3 << 6;
                if (strstart != 0)
                    num4 |= PRESET_DICT;
                var num5 = num4 + (31 - num4%31);
                status = BUSY_STATE;
                pending[pendingCount++] = (byte) (num5 >> 8);
                pending[pendingCount++] = (byte) num5;
                if (strstart != 0)
                {
                    pending[pendingCount++] = (byte) ((_codec._Adler32 & 4278190080U) >> 24);
                    pending[pendingCount++] = (byte) ((_codec._Adler32 & 16711680U) >> 16);
                    pending[pendingCount++] = (byte) ((_codec._Adler32 & 65280U) >> 8);
                    pending[pendingCount++] = (byte) (_codec._Adler32 & byte.MaxValue);
                }
                _codec._Adler32 = Adler.Adler32(0U, null, 0, 0);
            }
            if (pendingCount != 0)
            {
                _codec.flush_pending();
                if (_codec.AvailableBytesOut == 0)
                {
                    last_flush = -1;
                    return 0;
                }
            }
            else if (_codec.AvailableBytesIn == 0 && flush <= (FlushType) num1 && flush != FlushType.Finish)
                return 0;
            if (status == FINISH_STATE && _codec.AvailableBytesIn != 0)
            {
                _codec.Message = _ErrorMessage[7];
                throw new ZlibException("status == FINISH_STATE && _codec.AvailableBytesIn != 0");
            }
            if (_codec.AvailableBytesIn != 0 || lookahead != 0 || flush != FlushType.None && status != FINISH_STATE)
            {
                var blockState = DeflateFunction(flush);
                if (blockState == BlockState.FinishStarted || blockState == BlockState.FinishDone)
                    status = FINISH_STATE;
                if (blockState == BlockState.NeedMore || blockState == BlockState.FinishStarted)
                {
                    if (_codec.AvailableBytesOut == 0)
                        last_flush = -1;
                    return 0;
                }
                if (blockState == BlockState.BlockDone)
                {
                    if (flush == FlushType.Partial)
                    {
                        _tr_align();
                    }
                    else
                    {
                        _tr_stored_block(0, 0, false);
                        if (flush == FlushType.Full)
                        {
                            for (var index = 0; index < hash_size; ++index)
                                head[index] = 0;
                        }
                    }
                    _codec.flush_pending();
                    if (_codec.AvailableBytesOut == 0)
                    {
                        last_flush = -1;
                        return 0;
                    }
                }
            }
            if (flush != FlushType.Finish)
                return 0;
            if (!WantRfc1950HeaderBytes || Rfc1950BytesEmitted)
                return 1;
            pending[pendingCount++] = (byte) ((_codec._Adler32 & 4278190080U) >> 24);
            pending[pendingCount++] = (byte) ((_codec._Adler32 & 16711680U) >> 16);
            pending[pendingCount++] = (byte) ((_codec._Adler32 & 65280U) >> 8);
            pending[pendingCount++] = (byte) (_codec._Adler32 & byte.MaxValue);
            _codec.flush_pending();
            Rfc1950BytesEmitted = true;
            return pendingCount != 0 ? 0 : 1;
        }

        internal delegate BlockState CompressFunc(FlushType flush);

        internal class Config
        {
            private static readonly Config[] Table = new Config[10]
            {
                new Config(0, 0, 0, 0, DeflateFlavor.Store),
                new Config(4, 4, 8, 4, DeflateFlavor.Fast),
                new Config(4, 5, 16, 8, DeflateFlavor.Fast),
                new Config(4, 6, 32, 32, DeflateFlavor.Fast),
                new Config(4, 4, 16, 16, DeflateFlavor.Slow),
                new Config(8, 16, 32, 32, DeflateFlavor.Slow),
                new Config(8, 16, 128, 128, DeflateFlavor.Slow),
                new Config(8, 32, 128, 256, DeflateFlavor.Slow),
                new Config(32, 128, 258, 1024, DeflateFlavor.Slow),
                new Config(32, 258, 258, 4096, DeflateFlavor.Slow)
            };

            internal DeflateFlavor Flavor;
            internal int GoodLength;
            internal int MaxChainLength;
            internal int MaxLazy;
            internal int NiceLength;

            private Config(int goodLength, int maxLazy, int niceLength, int maxChainLength, DeflateFlavor flavor)
            {
                GoodLength = goodLength;
                MaxLazy = maxLazy;
                NiceLength = niceLength;
                MaxChainLength = maxChainLength;
                Flavor = flavor;
            }

            public static Config Lookup(CompressionLevel level)
            {
                return Table[(int) level];
            }
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    internal enum DeflateFlavor
    {
        Store,
        Fast,
        Slow
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    using System;
    using System.IO;

    public class DeflateStream : Stream
    {
        internal ZlibBaseStream _baseStream;
        private bool _disposed;
        internal Stream _innerStream;

        public DeflateStream(Stream stream, CompressionMode mode)
            : this(stream, mode, CompressionLevel.Default, false)
        {
        }

        public DeflateStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : this(stream, mode, level, false)
        {
        }

        public DeflateStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : this(stream, mode, CompressionLevel.Default, leaveOpen)
        {
        }

        public DeflateStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            _innerStream = stream;
            _baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.DEFLATE, leaveOpen);
        }

        public virtual FlushType FlushMode
        {
            get { return _baseStream._flushMode; }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("DeflateStream");
                _baseStream._flushMode = value;
            }
        }

        public int BufferSize
        {
            get { return _baseStream._bufferSize; }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("DeflateStream");
                if (_baseStream._workingBuffer != null)
                    throw new ZlibException("The working buffer is already set.");
                if (value < 1024)
                    throw new ZlibException(string.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, 1024));
                _baseStream._bufferSize = value;
            }
        }

        public CompressionStrategy Strategy
        {
            get { return _baseStream.Strategy; }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("DeflateStream");
                _baseStream.Strategy = value;
            }
        }

        public virtual long TotalIn
        {
            get { return _baseStream._z.TotalBytesIn; }
        }

        public virtual long TotalOut
        {
            get { return _baseStream._z.TotalBytesOut; }
        }

        public override bool CanRead
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("DeflateStream");
                return _baseStream._stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("DeflateStream");
                return _baseStream._stream.CanWrite;
            }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
                    return _baseStream._z.TotalBytesOut;
                if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
                    return _baseStream._z.TotalBytesIn;
                return 0L;
            }
            set { throw new NotImplementedException(); }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (_disposed)
                    return;
                if (disposing && _baseStream != null)
                    _baseStream.Close();
                _disposed = true;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            if (_disposed)
                throw new ObjectDisposedException("DeflateStream");
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException("DeflateStream");
            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException("DeflateStream");
            _baseStream.Write(buffer, offset, count);
        }

        public static byte[] CompressString(string s)
        {
            using (var memoryStream = new MemoryStream())
            {
                Stream compressor = new DeflateStream(memoryStream, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZlibBaseStream.CompressString(s, compressor);
                return memoryStream.ToArray();
            }
        }

        public static byte[] CompressBuffer(byte[] b)
        {
            using (var memoryStream = new MemoryStream())
            {
                Stream compressor = new DeflateStream(memoryStream, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZlibBaseStream.CompressBuffer(b, compressor);
                return memoryStream.ToArray();
            }
        }

        public static string UncompressString(byte[] compressed)
        {
            using (var memoryStream = new MemoryStream(compressed))
            {
                Stream decompressor = new DeflateStream(memoryStream, CompressionMode.Decompress);
                return ZlibBaseStream.UncompressString(compressed, decompressor);
            }
        }

        public static byte[] UncompressBuffer(byte[] compressed)
        {
            using (var memoryStream = new MemoryStream(compressed))
            {
                Stream decompressor = new DeflateStream(memoryStream, CompressionMode.Decompress);
                return ZlibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    public enum FlushType
    {
        None,
        Partial,
        Sync,
        Full,
        Finish
    }
} 
 
﻿namespace Universe.TinyGZip
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using SysGZip = System.IO.Compression;

    public class GZipExtentions
    {
        private static bool? _isSupported = null;
        static readonly object SyncIsSupported = new object();
        static readonly string _notSupportedMessage = "System.IO.Compression.GZipStream does not support compression/decompression.";

        public static Stream CreateCompressedFile(string fullPath)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                File.WriteAllBytes(fullPath, new byte[0]);
                ProcessStartInfo si = new ProcessStartInfo("compact", "/c \"" + fullPath + "\"");
                si.UseShellExecute = false;
                si.CreateNoWindow = true;
                try
                {
                    using (var p = Process.Start(si))
                    {
                        p.WaitForExit();
                    }

                    FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 16384);
                    return fs;
                }
                catch (Exception)
                {
                }
            }

            FileStream fileStream = new FileStream(fullPath + ".gz", FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 16384);
            Stream gz = CreateCompressor(fileStream);
            return gz;

        }


        public static Stream CreateDecompressor(Stream gzipped)
        {
            return CreateDecompressor(gzipped, false);
        }

        public static Stream CreateDecompressor(Stream gzipped, bool leaveOpen)
        {
            if (gzipped == null)
                throw new ArgumentNullException("gzipped");

            if (IsSystemGZipSupported)
                return new SysGZip.GZipStream(gzipped, SysGZip.CompressionMode.Decompress, leaveOpen);
            else
                return new Universe.TinyGZip.GZipStream(gzipped, CompressionMode.Decompress, leaveOpen);
        }

        public static Stream CreateCompressor(Stream plain, bool leaveOpen)
        {
            if (plain == null)
                throw new ArgumentNullException("plain");

            if (IsSystemGZipSupported)
                return new SysGZip.GZipStream(plain, SysGZip.CompressionMode.Compress, leaveOpen);
            else
                return new Universe.TinyGZip.GZipStream(plain, CompressionMode.Compress, CompressionLevel.Level1, leaveOpen);
        }

        public static Stream CreateCompressor(Stream plain)
        {
            return CreateCompressor(plain, false);
        }

        public static bool IsSystemGZipSupported
        {
            get
            {
                if (!_isSupported.HasValue)
                    lock(SyncIsSupported)
                        if (!_isSupported.HasValue)
                            _isSupported = IsSystemGZipSupport_Decompress() && IsSystemGZipSupport_Compress();

                return _isSupported.Value;
            }
        }

        static bool IsSystemGZipSupport_Decompress()
        {
            byte[] gzipped = new byte[] {
                0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x63, 0x65,
                0x61, 0x66, 0x62, 0x04, 0x00, 0x77, 0x03, 0xd7, 0xc6, 0x05, 0x00, 0x00, 0x00
            };

            try
            {
                MemoryStream mem = new MemoryStream(gzipped);
                using (SysGZip.GZipStream s = new SysGZip.GZipStream(mem, SysGZip.CompressionMode.Decompress))
                {
                    byte[] plain = new byte[5 + 1];
                    var n = s.Read(plain, 0, plain.Length);
                    if (n != 5)
                        throw new NotSupportedException(_notSupportedMessage);

                    if (plain[0] != 5 || plain[1] != 4 || plain[2] != 3 || plain[3] != 2 || plain[4] != 1)
                    {
                        throw new NotSupportedException(_notSupportedMessage);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(_notSupportedMessage + " We are using TinyGZip implementation" + Environment.NewLine + ex);
                return false;
            }
        }

        static bool IsSystemGZipSupport_Compress()
        {
            byte[] plain = new byte[]{5, 4, 3, 2, 1};

            try
            {
                MemoryStream mem = new MemoryStream();
                using (SysGZip.GZipStream s = new SysGZip.GZipStream(mem, SysGZip.CompressionMode.Compress, false))
                {
                    s.Write(plain, 0, plain.Length);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(_notSupportedMessage + " We are using TinyGZip implementation" + Environment.NewLine + ex);
                return false;
            }

            if (false)
                throw new NotSupportedException(_notSupportedMessage);

            return true;
        }
    }
}
 
 

namespace Universe.TinyGZip
{
    using System;
    using System.IO;
    using System.Text;

    using InternalImplementation;

    public class GZipStream : Stream
    {
        internal static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static readonly Encoding iso8859dash1 = new Iso8859Dash1Encoding();
        internal ZlibBaseStream _baseStream;
        private string _Comment;
        private bool _disposed;
        private string _FileName;
        private bool _firstReadDone;
        private int _headerByteCount;
        public DateTime? LastModified;

        public GZipStream(Stream stream, CompressionMode mode)
            : this(stream, mode, CompressionLevel.Default, false)
        {
        }

        public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : this(stream, mode, level, false)
        {
        }

        public GZipStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : this(stream, mode, CompressionLevel.Default, leaveOpen)
        {
        }

        public GZipStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            _baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.GZIP, leaveOpen);
        }

        public string Comment
        {
            get { return _Comment; }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("GZipStream");
                _Comment = value;
            }
        }

        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("GZipStream");
                _FileName = value;
                if (_FileName == null)
                    return;
                if (_FileName.IndexOf("/") != -1)
                    _FileName = _FileName.Replace("/", "\\");
                if (_FileName.EndsWith("\\"))
                    throw new Exception("Illegal filename");
                if (_FileName.IndexOf("\\") == -1)
                    return;
                _FileName = Path.GetFileName(_FileName);
            }
        }

        public int Crc32 { get; private set; }

        public virtual FlushType FlushMode
        {
            get { return _baseStream._flushMode; }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("GZipStream");
                _baseStream._flushMode = value;
            }
        }

        public int BufferSize
        {
            get { return _baseStream._bufferSize; }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("GZipStream");
                if (_baseStream._workingBuffer != null)
                    throw new ZlibException("The working buffer is already set.");
                if (value < 1024)
                    throw new ZlibException(string.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, 1024));
                _baseStream._bufferSize = value;
            }
        }

        public virtual long TotalIn
        {
            get { return _baseStream._z.TotalBytesIn; }
        }

        public virtual long TotalOut
        {
            get { return _baseStream._z.TotalBytesOut; }
        }

        public override bool CanRead
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("GZipStream");
                return _baseStream._stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("GZipStream");
                return _baseStream._stream.CanWrite;
            }
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
                    return _baseStream._z.TotalBytesOut + _headerByteCount;
                if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
                    return _baseStream._z.TotalBytesIn + _baseStream._gzipHeaderByteCount;
                return 0L;
            }
            set { throw new NotImplementedException(); }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (_disposed)
                    return;
                if (disposing && _baseStream != null)
                {
                    _baseStream.Close();
                    Crc32 = _baseStream.Crc32;
                }
                _disposed = true;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            if (_disposed)
                throw new ObjectDisposedException("GZipStream");
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException("GZipStream");
            var num = _baseStream.Read(buffer, offset, count);
            if (!_firstReadDone)
            {
                _firstReadDone = true;
                FileName = _baseStream._GzipFileName;
                Comment = _baseStream._GzipComment;
            }
            return num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException("GZipStream");
            if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Undefined)
            {
                if (!_baseStream._wantCompress)
                    throw new InvalidOperationException();
                _headerByteCount = EmitHeader();
            }
            _baseStream.Write(buffer, offset, count);
        }

        private int EmitHeader()
        {
            var numArray1 = Comment == null ? null : iso8859dash1.GetBytes(Comment);
            var numArray2 = FileName == null ? null : iso8859dash1.GetBytes(FileName);
            var num1 = Comment == null ? 0 : numArray1.Length + 1;
            var num2 = FileName == null ? 0 : numArray2.Length + 1;
            var buffer = new byte[10 + num1 + num2];
            var num3 = 0;
            var numArray3 = buffer;
            var index1 = num3;
            var num4 = 1;
            var num5 = index1 + num4;
            var num6 = 31;
            numArray3[index1] = (byte) num6;
            var numArray4 = buffer;
            var index2 = num5;
            var num7 = 1;
            var num8 = index2 + num7;
            var num9 = 139;
            numArray4[index2] = (byte) num9;
            var numArray5 = buffer;
            var index3 = num8;
            var num10 = 1;
            var num11 = index3 + num10;
            var num12 = 8;
            numArray5[index3] = (byte) num12;
            byte num13 = 0;
            if (Comment != null)
                num13 ^= 16;
            if (FileName != null)
                num13 ^= 8;
            var numArray6 = buffer;
            var index4 = num11;
            var num14 = 1;
            var destinationIndex1 = index4 + num14;
            int num15 = num13;
            numArray6[index4] = (byte) num15;
            if (!LastModified.HasValue)
                LastModified = DateTime.Now;
            Array.Copy(BitConverter.GetBytes((int) (LastModified.Value - _unixEpoch).TotalSeconds), 0, buffer, destinationIndex1, 4);
            var num16 = destinationIndex1 + 4;
            var numArray7 = buffer;
            var index5 = num16;
            var num17 = 1;
            var num18 = index5 + num17;
            var num19 = 0;
            numArray7[index5] = (byte) num19;
            var numArray8 = buffer;
            var index6 = num18;
            var num20 = 1;
            var destinationIndex2 = index6 + num20;
            int num21 = byte.MaxValue;
            numArray8[index6] = (byte) num21;
            if (num2 != 0)
            {
                Array.Copy(numArray2, 0, buffer, destinationIndex2, num2 - 1);
                var num22 = destinationIndex2 + (num2 - 1);
                var numArray9 = buffer;
                var index7 = num22;
                var num23 = 1;
                destinationIndex2 = index7 + num23;
                var num24 = 0;
                numArray9[index7] = (byte) num24;
            }
            if (num1 != 0)
            {
                Array.Copy(numArray1, 0, buffer, destinationIndex2, num1 - 1);
                var num22 = destinationIndex2 + (num1 - 1);
                var numArray9 = buffer;
                var index7 = num22;
                var num23 = 1;
                var num24 = index7 + num23;
                var num25 = 0;
                numArray9[index7] = (byte) num25;
            }
            _baseStream._stream.Write(buffer, 0, buffer.Length);
            return buffer.Length;
        }

        public static byte[] CompressString(string s)
        {
            using (var memoryStream = new MemoryStream())
            {
                Stream compressor = new GZipStream(memoryStream, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZlibBaseStream.CompressString(s, compressor);
                return memoryStream.ToArray();
            }
        }

        public static byte[] CompressBuffer(byte[] b)
        {
            using (var memoryStream = new MemoryStream())
            {
                Stream compressor = new GZipStream(memoryStream, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZlibBaseStream.CompressBuffer(b, compressor);
                return memoryStream.ToArray();
            }
        }

        public static string UncompressString(byte[] compressed)
        {
            using (var memoryStream = new MemoryStream(compressed))
            {
                Stream decompressor = new GZipStream(memoryStream, CompressionMode.Decompress);
                return ZlibBaseStream.UncompressString(compressed, decompressor);
            }
        }

        public static byte[] UncompressBuffer(byte[] compressed)
        {
            using (var memoryStream = new MemoryStream(compressed))
            {
                Stream decompressor = new GZipStream(memoryStream, CompressionMode.Decompress);
                return ZlibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }
    }
} 
 
namespace Universe.TinyGZip.InternalImplementation
{
    using System;

    internal sealed class InflateBlocks
    {
        private const int MANY = 1440;

        internal static readonly int[] border = new int[19]
        {
            16,
            17,
            18,
            0,
            8,
            7,
            9,
            6,
            10,
            5,
            11,
            4,
            12,
            3,
            13,
            2,
            14,
            1,
            15
        };

        internal ZlibCodec _codec;
        internal int[] bb = new int[1];
        internal int bitb;
        internal int bitk;
        internal int[] blens;
        internal uint check;
        internal object checkfn;
        internal InflateCodes codes = new InflateCodes();
        internal int end;
        internal int[] hufts;
        internal int index;
        internal InfTree inftree = new InfTree();
        internal int last;
        internal int left;
        private InflateBlockMode mode;
        internal int readAt;
        internal int table;
        internal int[] tb = new int[1];
        internal byte[] window;
        internal int writeAt;

        internal InflateBlocks(ZlibCodec codec, object checkfn, int w)
        {
            _codec = codec;
            hufts = new int[4320];
            window = new byte[w];
            end = w;
            this.checkfn = checkfn;
            mode = InflateBlockMode.TYPE;
            var num = (int) Reset();
        }

        internal uint Reset()
        {
            var num = check;
            mode = InflateBlockMode.TYPE;
            bitk = 0;
            bitb = 0;
            readAt = writeAt = 0;
            if (checkfn != null)
                _codec._Adler32 = check = Adler.Adler32(0U, null, 0, 0);
            return num;
        }

        internal int Process(int r)
        {
            int t; 
            int b; 
            int k; 
            int p; 
            int n; 
            int q; 
            int m; 


            p = _codec.NextIn;
            n = _codec.AvailableBytesIn;
            b = bitb;
            k = bitk;

            q = writeAt;
            m = q < readAt ? readAt - q - 1 : end - q;


            while (true)
            {
                switch (mode)
                {
                    case InflateBlockMode.TYPE:

                        while (k < (3))
                        {
                            if (n != 0)
                            {
                                r = ZlibConstants.Z_OK;
                            }
                            else
                            {
                                bitb = b;
                                bitk = k;
                                _codec.AvailableBytesIn = n;
                                _codec.TotalBytesIn += p - _codec.NextIn;
                                _codec.NextIn = p;
                                writeAt = q;
                                return Flush(r);
                            }

                            n--;
                            b |= (_codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }
                        t = b & 7;
                        last = t & 1;

                        switch ((uint) t >> 1)
                        {
                            case 0: 
                                b >>= 3;
                                k -= (3);
                                t = k & 7; 
                                b >>= t;
                                k -= t;
                                mode = InflateBlockMode.LENS; 
                                break;

                            case 1: 
                                var bl = new int[1];
                                var bd = new int[1];
                                var tl = new int[1][];
                                var td = new int[1][];
                                InfTree.inflate_trees_fixed(bl, bd, tl, td, _codec);
                                codes.Init(bl[0], bd[0], tl[0], 0, td[0], 0);
                                b >>= 3;
                                k -= 3;
                                mode = InflateBlockMode.CODES;
                                break;

                            case 2: 
                                b >>= 3;
                                k -= 3;
                                mode = InflateBlockMode.TABLE;
                                break;

                            case 3: 
                                b >>= 3;
                                k -= 3;
                                mode = InflateBlockMode.BAD;
                                _codec.Message = "invalid block type";
                                r = ZlibConstants.Z_DATA_ERROR;
                                bitb = b;
                                bitk = k;
                                _codec.AvailableBytesIn = n;
                                _codec.TotalBytesIn += p - _codec.NextIn;
                                _codec.NextIn = p;
                                writeAt = q;
                                return Flush(r);
                        }
                        break;

                    case InflateBlockMode.LENS:

                        while (k < (32))
                        {
                            if (n != 0)
                            {
                                r = ZlibConstants.Z_OK;
                            }
                            else
                            {
                                bitb = b;
                                bitk = k;
                                _codec.AvailableBytesIn = n;
                                _codec.TotalBytesIn += p - _codec.NextIn;
                                _codec.NextIn = p;
                                writeAt = q;
                                return Flush(r);
                            }
                            ;
                            n--;
                            b |= (_codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        if ((((~b) >> 16) & 0xffff) != (b & 0xffff))
                        {
                            mode = InflateBlockMode.BAD;
                            _codec.Message = "invalid stored block lengths";
                            r = ZlibConstants.Z_DATA_ERROR;

                            bitb = b;
                            bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }
                        left = (b & 0xffff);
                        b = k = 0; 
                        mode = left != 0 ? InflateBlockMode.STORED : (last != 0 ? InflateBlockMode.DRY : InflateBlockMode.TYPE);
                        break;

                    case InflateBlockMode.STORED:
                        if (n == 0)
                        {
                            bitb = b;
                            bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }

                        if (m == 0)
                        {
                            if (q == end && readAt != 0)
                            {
                                q = 0;
                                m = q < readAt ? readAt - q - 1 : end - q;
                            }
                            if (m == 0)
                            {
                                writeAt = q;
                                r = Flush(r);
                                q = writeAt;
                                m = q < readAt ? readAt - q - 1 : end - q;
                                if (q == end && readAt != 0)
                                {
                                    q = 0;
                                    m = q < readAt ? readAt - q - 1 : end - q;
                                }
                                if (m == 0)
                                {
                                    bitb = b;
                                    bitk = k;
                                    _codec.AvailableBytesIn = n;
                                    _codec.TotalBytesIn += p - _codec.NextIn;
                                    _codec.NextIn = p;
                                    writeAt = q;
                                    return Flush(r);
                                }
                            }
                        }
                        r = ZlibConstants.Z_OK;

                        t = left;
                        if (t > n)
                            t = n;
                        if (t > m)
                            t = m;
                        Array.Copy(_codec.InputBuffer, p, window, q, t);
                        p += t;
                        n -= t;
                        q += t;
                        m -= t;
                        if ((left -= t) != 0)
                            break;
                        mode = last != 0 ? InflateBlockMode.DRY : InflateBlockMode.TYPE;
                        break;

                    case InflateBlockMode.TABLE:

                        while (k < (14))
                        {
                            if (n != 0)
                            {
                                r = ZlibConstants.Z_OK;
                            }
                            else
                            {
                                bitb = b;
                                bitk = k;
                                _codec.AvailableBytesIn = n;
                                _codec.TotalBytesIn += p - _codec.NextIn;
                                _codec.NextIn = p;
                                writeAt = q;
                                return Flush(r);
                            }

                            n--;
                            b |= (_codec.InputBuffer[p++] & 0xff) << k;
                            k += 8;
                        }

                        table = t = (b & 0x3fff);
                        if ((t & 0x1f) > 29 || ((t >> 5) & 0x1f) > 29)
                        {
                            mode = InflateBlockMode.BAD;
                            _codec.Message = "too many length or distance symbols";
                            r = ZlibConstants.Z_DATA_ERROR;

                            bitb = b;
                            bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }
                        t = 258 + (t & 0x1f) + ((t >> 5) & 0x1f);
                        if (blens == null || blens.Length < t)
                        {
                            blens = new int[t];
                        }
                        else
                        {
                            Array.Clear(blens, 0, t);
                        }

                        b >>= 14;
                        k -= 14;


                        index = 0;
                        mode = InflateBlockMode.BTREE;
                        goto case InflateBlockMode.BTREE;

                    case InflateBlockMode.BTREE:
                        while (index < 4 + (table >> 10))
                        {
                            while (k < (3))
                            {
                                if (n != 0)
                                {
                                    r = ZlibConstants.Z_OK;
                                }
                                else
                                {
                                    bitb = b;
                                    bitk = k;
                                    _codec.AvailableBytesIn = n;
                                    _codec.TotalBytesIn += p - _codec.NextIn;
                                    _codec.NextIn = p;
                                    writeAt = q;
                                    return Flush(r);
                                }

                                n--;
                                b |= (_codec.InputBuffer[p++] & 0xff) << k;
                                k += 8;
                            }

                            blens[border[index++]] = b & 7;

                            b >>= 3;
                            k -= 3;
                        }

                        while (index < 19)
                        {
                            blens[border[index++]] = 0;
                        }

                        bb[0] = 7;
                        t = inftree.inflate_trees_bits(blens, bb, tb, hufts, _codec);
                        if (t != ZlibConstants.Z_OK)
                        {
                            r = t;
                            if (r == ZlibConstants.Z_DATA_ERROR)
                            {
                                blens = null;
                                mode = InflateBlockMode.BAD;
                            }

                            bitb = b;
                            bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }

                        index = 0;
                        mode = InflateBlockMode.DTREE;
                        goto case InflateBlockMode.DTREE;

                    case InflateBlockMode.DTREE:
                        while (true)
                        {
                            t = table;
                            if (!(index < 258 + (t & 0x1f) + ((t >> 5) & 0x1f)))
                            {
                                break;
                            }

                            int i, j, c;

                            t = bb[0];

                            while (k < t)
                            {
                                if (n != 0)
                                {
                                    r = ZlibConstants.Z_OK;
                                }
                                else
                                {
                                    bitb = b;
                                    bitk = k;
                                    _codec.AvailableBytesIn = n;
                                    _codec.TotalBytesIn += p - _codec.NextIn;
                                    _codec.NextIn = p;
                                    writeAt = q;
                                    return Flush(r);
                                }

                                n--;
                                b |= (_codec.InputBuffer[p++] & 0xff) << k;
                                k += 8;
                            }

                            t = hufts[(tb[0] + (b & InternalInflateConstants.InflateMask[t]))*3 + 1];
                            c = hufts[(tb[0] + (b & InternalInflateConstants.InflateMask[t]))*3 + 2];

                            if (c < 16)
                            {
                                b >>= t;
                                k -= t;
                                blens[index++] = c;
                            }
                            else
                            {
                                i = c == 18 ? 7 : c - 14;
                                j = c == 18 ? 11 : 3;

                                while (k < (t + i))
                                {
                                    if (n != 0)
                                    {
                                        r = ZlibConstants.Z_OK;
                                    }
                                    else
                                    {
                                        bitb = b;
                                        bitk = k;
                                        _codec.AvailableBytesIn = n;
                                        _codec.TotalBytesIn += p - _codec.NextIn;
                                        _codec.NextIn = p;
                                        writeAt = q;
                                        return Flush(r);
                                    }

                                    n--;
                                    b |= (_codec.InputBuffer[p++] & 0xff) << k;
                                    k += 8;
                                }

                                b >>= t;
                                k -= t;

                                j += (b & InternalInflateConstants.InflateMask[i]);

                                b >>= i;
                                k -= i;

                                i = index;
                                t = table;
                                if (i + j > 258 + (t & 0x1f) + ((t >> 5) & 0x1f) || (c == 16 && i < 1))
                                {
                                    blens = null;
                                    mode = InflateBlockMode.BAD;
                                    _codec.Message = "invalid bit length repeat";
                                    r = ZlibConstants.Z_DATA_ERROR;

                                    bitb = b;
                                    bitk = k;
                                    _codec.AvailableBytesIn = n;
                                    _codec.TotalBytesIn += p - _codec.NextIn;
                                    _codec.NextIn = p;
                                    writeAt = q;
                                    return Flush(r);
                                }

                                c = (c == 16) ? blens[i - 1] : 0;
                                do
                                {
                                    blens[i++] = c;
                                } while (--j != 0);
                                index = i;
                            }
                        }

                        tb[0] = -1;
                    {
                        int[] bl = {9};
                        int[] bd = {6};
                        var tl = new int[1];
                        var td = new int[1];

                        t = table;
                        t = inftree.inflate_trees_dynamic(257 + (t & 0x1f), 1 + ((t >> 5) & 0x1f), blens, bl, bd, tl, td, hufts, _codec);

                        if (t != ZlibConstants.Z_OK)
                        {
                            if (t == ZlibConstants.Z_DATA_ERROR)
                            {
                                blens = null;
                                mode = InflateBlockMode.BAD;
                            }
                            r = t;

                            bitb = b;
                            bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }
                        codes.Init(bl[0], bd[0], hufts, tl[0], hufts, td[0]);
                    }
                        mode = InflateBlockMode.CODES;
                        goto case InflateBlockMode.CODES;

                    case InflateBlockMode.CODES:
                        bitb = b;
                        bitk = k;
                        _codec.AvailableBytesIn = n;
                        _codec.TotalBytesIn += p - _codec.NextIn;
                        _codec.NextIn = p;
                        writeAt = q;

                        r = codes.Process(this, r);
                        if (r != ZlibConstants.Z_STREAM_END)
                        {
                            return Flush(r);
                        }

                        r = ZlibConstants.Z_OK;
                        p = _codec.NextIn;
                        n = _codec.AvailableBytesIn;
                        b = bitb;
                        k = bitk;
                        q = writeAt;
                        m = q < readAt ? readAt - q - 1 : end - q;

                        if (last == 0)
                        {
                            mode = InflateBlockMode.TYPE;
                            break;
                        }
                        mode = InflateBlockMode.DRY;
                        goto case InflateBlockMode.DRY;

                    case InflateBlockMode.DRY:
                        writeAt = q;
                        r = Flush(r);
                        q = writeAt;
                        m = q < readAt ? readAt - q - 1 : end - q;
                        if (readAt != writeAt)
                        {
                            bitb = b;
                            bitk = k;
                            _codec.AvailableBytesIn = n;
                            _codec.TotalBytesIn += p - _codec.NextIn;
                            _codec.NextIn = p;
                            writeAt = q;
                            return Flush(r);
                        }
                        mode = InflateBlockMode.DONE;
                        goto case InflateBlockMode.DONE;

                    case InflateBlockMode.DONE:
                        r = ZlibConstants.Z_STREAM_END;
                        bitb = b;
                        bitk = k;
                        _codec.AvailableBytesIn = n;
                        _codec.TotalBytesIn += p - _codec.NextIn;
                        _codec.NextIn = p;
                        writeAt = q;
                        return Flush(r);

                    case InflateBlockMode.BAD:
                        r = ZlibConstants.Z_DATA_ERROR;

                        bitb = b;
                        bitk = k;
                        _codec.AvailableBytesIn = n;
                        _codec.TotalBytesIn += p - _codec.NextIn;
                        _codec.NextIn = p;
                        writeAt = q;
                        return Flush(r);


                    default:
                        r = ZlibConstants.Z_STREAM_ERROR;

                        bitb = b;
                        bitk = k;
                        _codec.AvailableBytesIn = n;
                        _codec.TotalBytesIn += p - _codec.NextIn;
                        _codec.NextIn = p;
                        writeAt = q;
                        return Flush(r);
                }
            }
        }

        internal void Free()
        {
            var num = (int) Reset();
            window = null;
            hufts = null;
        }

        internal void SetDictionary(byte[] d, int start, int n)
        {
            Array.Copy(d, start, window, 0, n);
            readAt = writeAt = n;
        }

        internal int SyncPoint()
        {
            return mode == InflateBlockMode.LENS ? 1 : 0;
        }

        internal int Flush(int r)
        {
            for (var index = 0; index < 2; ++index)
            {
                var num = index != 0 ? writeAt - readAt : (readAt <= writeAt ? writeAt : end) - readAt;
                if (num == 0)
                {
                    if (r == -5)
                        r = 0;
                    return r;
                }
                if (num > _codec.AvailableBytesOut)
                    num = _codec.AvailableBytesOut;
                if (num != 0 && r == -5)
                    r = 0;
                _codec.AvailableBytesOut -= num;
                _codec.TotalBytesOut += num;
                if (checkfn != null)
                    _codec._Adler32 = check = Adler.Adler32(check, window, readAt, num);
                Array.Copy(window, readAt, _codec.OutputBuffer, _codec.NextOut, num);
                _codec.NextOut += num;
                readAt += num;
                if (readAt == end && index == 0)
                {
                    readAt = 0;
                    if (writeAt == end)
                        writeAt = 0;
                }
                else
                    ++index;
            }
            return r;
        }

        private enum InflateBlockMode
        {
            TYPE,
            LENS,
            STORED,
            TABLE,
            BTREE,
            DTREE,
            CODES,
            DRY,
            DONE,
            BAD
        }
    }
} 
 
namespace Universe.TinyGZip.InternalImplementation
{
    using System;

    #pragma warning disable 642, 219
    internal sealed class InflateCodes
    {
        private const int START = 0;
        private const int LEN = 1;
        private const int LENEXT = 2;
        private const int DIST = 3;
        private const int DISTEXT = 4;
        private const int COPY = 5;
        private const int LIT = 6;
        private const int WASH = 7;
        private const int END = 8;
        private const int BADCODE = 9;
        internal int bitsToGet;
        internal byte dbits;
        internal int dist;
        internal int[] dtree;
        internal int dtree_index;
        internal byte lbits;
        internal int len;
        internal int lit;
        internal int[] ltree;
        internal int ltree_index;
        internal int mode;
        internal int need;
        internal int[] tree;
        internal int tree_index;

        internal void Init(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index)
        {
            mode = 0;
            lbits = (byte) bl;
            dbits = (byte) bd;
            ltree = tl;
            ltree_index = tl_index;
            dtree = td;
            dtree_index = td_index;
            tree = null;
        }

        internal int Process(InflateBlocks blocks, int r)
        {
            var z = blocks._codec;
            var num1 = z.NextIn;
            var num2 = z.AvailableBytesIn;
            var num3 = blocks.bitb;
            var num4 = blocks.bitk;
            var num5 = blocks.writeAt;
            var num6 = num5 < blocks.readAt ? blocks.readAt - num5 - 1 : blocks.end - num5;
            while (true)
            {
                switch (mode)
                {
                    case 0:
                        if (num6 >= 258 && num2 >= 10)
                        {
                            blocks.bitb = num3;
                            blocks.bitk = num4;
                            z.AvailableBytesIn = num2;
                            z.TotalBytesIn += num1 - z.NextIn;
                            z.NextIn = num1;
                            blocks.writeAt = num5;
                            r = InflateFast(lbits, dbits, ltree, ltree_index, dtree, dtree_index, blocks, z);
                            num1 = z.NextIn;
                            num2 = z.AvailableBytesIn;
                            num3 = blocks.bitb;
                            num4 = blocks.bitk;
                            num5 = blocks.writeAt;
                            num6 = num5 < blocks.readAt ? blocks.readAt - num5 - 1 : blocks.end - num5;
                            if (r != 0)
                            {
                                mode = r == 1 ? 7 : 9;
                                break;
                            }
                        }
                        need = lbits;
                        tree = ltree;
                        tree_index = ltree_index;
                        mode = 1;
                        goto case 1;
                    case 1:
                        var index1 = need;
                        while (num4 < index1)
                        {
                            if (num2 != 0)
                            {
                                r = 0;
                                --num2;
                                num3 |= (z.InputBuffer[num1++] & byte.MaxValue) << num4;
                                num4 += 8;
                            }
                            else
                            {
                                blocks.bitb = num3;
                                blocks.bitk = num4;
                                z.AvailableBytesIn = num2;
                                z.TotalBytesIn += num1 - z.NextIn;
                                z.NextIn = num1;
                                blocks.writeAt = num5;
                                return blocks.Flush(r);
                            }
                        }
                        var index2 = (tree_index + (num3 & InternalInflateConstants.InflateMask[index1]))*3;
                        num3 >>= tree[index2 + 1];
                        num4 -= tree[index2 + 1];
                        var num7 = tree[index2];
                        if (num7 == 0)
                        {
                            lit = tree[index2 + 2];
                            mode = 6;
                            break;
                        }
                        if ((num7 & 16) != 0)
                        {
                            bitsToGet = num7 & 15;
                            len = tree[index2 + 2];
                            mode = 2;
                            break;
                        }
                        if ((num7 & 64) == 0)
                        {
                            need = num7;
                            tree_index = index2/3 + tree[index2 + 2];
                            break;
                        }
                        if ((num7 & 32) != 0)
                        {
                            mode = 7;
                            break;
                        }
                        goto label_18;
                    case 2:
                        var index3 = bitsToGet;
                        while (num4 < index3)
                        {
                            if (num2 != 0)
                            {
                                r = 0;
                                --num2;
                                num3 |= (z.InputBuffer[num1++] & byte.MaxValue) << num4;
                                num4 += 8;
                            }
                            else
                            {
                                blocks.bitb = num3;
                                blocks.bitk = num4;
                                z.AvailableBytesIn = num2;
                                z.TotalBytesIn += num1 - z.NextIn;
                                z.NextIn = num1;
                                blocks.writeAt = num5;
                                return blocks.Flush(r);
                            }
                        }
                        len += num3 & InternalInflateConstants.InflateMask[index3];
                        num3 >>= index3;
                        num4 -= index3;
                        need = dbits;
                        tree = dtree;
                        tree_index = dtree_index;
                        mode = 3;
                        goto case 3;
                    case 3:
                        var index4 = need;
                        while (num4 < index4)
                        {
                            if (num2 != 0)
                            {
                                r = 0;
                                --num2;
                                num3 |= (z.InputBuffer[num1++] & byte.MaxValue) << num4;
                                num4 += 8;
                            }
                            else
                            {
                                blocks.bitb = num3;
                                blocks.bitk = num4;
                                z.AvailableBytesIn = num2;
                                z.TotalBytesIn += num1 - z.NextIn;
                                z.NextIn = num1;
                                blocks.writeAt = num5;
                                return blocks.Flush(r);
                            }
                        }
                        var index5 = (tree_index + (num3 & InternalInflateConstants.InflateMask[index4]))*3;
                        num3 >>= tree[index5 + 1];
                        num4 -= tree[index5 + 1];
                        var num8 = tree[index5];
                        if ((num8 & 16) != 0)
                        {
                            bitsToGet = num8 & 15;
                            dist = tree[index5 + 2];
                            mode = 4;
                            break;
                        }
                        if ((num8 & 64) == 0)
                        {
                            need = num8;
                            tree_index = index5/3 + tree[index5 + 2];
                            break;
                        }
                        goto label_34;
                    case 4:
                        var index6 = bitsToGet;
                        while (num4 < index6)
                        {
                            if (num2 != 0)
                            {
                                r = 0;
                                --num2;
                                num3 |= (z.InputBuffer[num1++] & byte.MaxValue) << num4;
                                num4 += 8;
                            }
                            else
                            {
                                blocks.bitb = num3;
                                blocks.bitk = num4;
                                z.AvailableBytesIn = num2;
                                z.TotalBytesIn += num1 - z.NextIn;
                                z.NextIn = num1;
                                blocks.writeAt = num5;
                                return blocks.Flush(r);
                            }
                        }
                        dist += num3 & InternalInflateConstants.InflateMask[index6];
                        num3 >>= index6;
                        num4 -= index6;
                        mode = 5;
                        goto case 5;
                    case 5:
                        var num9 = num5 - dist;
                        while (num9 < 0)
                            num9 += blocks.end;
                        for (; len != 0; --len)
                        {
                            if (num6 == 0)
                            {
                                if (num5 == blocks.end && blocks.readAt != 0)
                                {
                                    num5 = 0;
                                    num6 = num5 < blocks.readAt ? blocks.readAt - num5 - 1 : blocks.end - num5;
                                }
                                if (num6 == 0)
                                {
                                    blocks.writeAt = num5;
                                    r = blocks.Flush(r);
                                    num5 = blocks.writeAt;
                                    num6 = num5 < blocks.readAt ? blocks.readAt - num5 - 1 : blocks.end - num5;
                                    if (num5 == blocks.end && blocks.readAt != 0)
                                    {
                                        num5 = 0;
                                        num6 = num5 < blocks.readAt ? blocks.readAt - num5 - 1 : blocks.end - num5;
                                    }
                                    if (num6 == 0)
                                    {
                                        blocks.bitb = num3;
                                        blocks.bitk = num4;
                                        z.AvailableBytesIn = num2;
                                        z.TotalBytesIn += num1 - z.NextIn;
                                        z.NextIn = num1;
                                        blocks.writeAt = num5;
                                        return blocks.Flush(r);
                                    }
                                }
                            }
                            blocks.window[num5++] = blocks.window[num9++];
                            --num6;
                            if (num9 == blocks.end)
                                num9 = 0;
                        }
                        mode = 0;
                        break;
                    case 6:
                        if (num6 == 0)
                        {
                            if (num5 == blocks.end && blocks.readAt != 0)
                            {
                                num5 = 0;
                                num6 = num5 < blocks.readAt ? blocks.readAt - num5 - 1 : blocks.end - num5;
                            }
                            if (num6 == 0)
                            {
                                blocks.writeAt = num5;
                                r = blocks.Flush(r);
                                num5 = blocks.writeAt;
                                num6 = num5 < blocks.readAt ? blocks.readAt - num5 - 1 : blocks.end - num5;
                                if (num5 == blocks.end && blocks.readAt != 0)
                                {
                                    num5 = 0;
                                    num6 = num5 < blocks.readAt ? blocks.readAt - num5 - 1 : blocks.end - num5;
                                }
                                if (num6 == 0)
                                    goto label_65;
                            }
                        }
                        r = 0;
                        blocks.window[num5++] = (byte) lit;
                        --num6;
                        mode = 0;
                        break;
                    case 7:
                        goto label_68;
                    case 8:
                        goto label_73;
                    case 9:
                        goto label_74;
                    default:
                        goto label_75;
                }
            }
            label_18:
            mode = 9;
            z.Message = "invalid literal/length code";
            r = -3;
            blocks.bitb = num3;
            blocks.bitk = num4;
            z.AvailableBytesIn = num2;
            z.TotalBytesIn += num1 - z.NextIn;
            z.NextIn = num1;
            blocks.writeAt = num5;
            return blocks.Flush(r);
            label_34:
            mode = 9;
            z.Message = "invalid distance code";
            r = -3;
            blocks.bitb = num3;
            blocks.bitk = num4;
            z.AvailableBytesIn = num2;
            z.TotalBytesIn += num1 - z.NextIn;
            z.NextIn = num1;
            blocks.writeAt = num5;
            return blocks.Flush(r);
            label_65:
            blocks.bitb = num3;
            blocks.bitk = num4;
            z.AvailableBytesIn = num2;
            z.TotalBytesIn += num1 - z.NextIn;
            z.NextIn = num1;
            blocks.writeAt = num5;
            return blocks.Flush(r);
            label_68:
            if (num4 > 7)
            {
                num4 -= 8;
                ++num2;
                --num1;
            }
            blocks.writeAt = num5;
            r = blocks.Flush(r);
            num5 = blocks.writeAt;
            var num10 = num5 < blocks.readAt ? blocks.readAt - num5 - 1 : blocks.end - num5;
            if (blocks.readAt != blocks.writeAt)
            {
                blocks.bitb = num3;
                blocks.bitk = num4;
                z.AvailableBytesIn = num2;
                z.TotalBytesIn += num1 - z.NextIn;
                z.NextIn = num1;
                blocks.writeAt = num5;
                return blocks.Flush(r);
            }
            mode = 8;
            label_73:
            r = 1;
            blocks.bitb = num3;
            blocks.bitk = num4;
            z.AvailableBytesIn = num2;
            z.TotalBytesIn += num1 - z.NextIn;
            z.NextIn = num1;
            blocks.writeAt = num5;
            return blocks.Flush(r);
            label_74:
            r = -3;
            blocks.bitb = num3;
            blocks.bitk = num4;
            z.AvailableBytesIn = num2;
            z.TotalBytesIn += num1 - z.NextIn;
            z.NextIn = num1;
            blocks.writeAt = num5;
            return blocks.Flush(r);
            label_75:
            r = -2;
            blocks.bitb = num3;
            blocks.bitk = num4;
            z.AvailableBytesIn = num2;
            z.TotalBytesIn += num1 - z.NextIn;
            z.NextIn = num1;
            blocks.writeAt = num5;
            return blocks.Flush(r);
        }

        internal int InflateFast(int bl, int bd, int[] tl, int tl_index, int[] td, int td_index, InflateBlocks s, ZlibCodec z)
        {
            var num1 = z.NextIn;
            var num2 = z.AvailableBytesIn;
            var num3 = s.bitb;
            var num4 = s.bitk;
            var destinationIndex = s.writeAt;
            var num5 = destinationIndex < s.readAt ? s.readAt - destinationIndex - 1 : s.end - destinationIndex;
            var num6 = InternalInflateConstants.InflateMask[bl];
            var num7 = InternalInflateConstants.InflateMask[bd];
            do
            {
                while (num4 < 20)
                {
                    --num2;
                    num3 |= (z.InputBuffer[num1++] & byte.MaxValue) << num4;
                    num4 += 8;
                }
                var num8 = num3 & num6;
                var numArray1 = tl;
                var num9 = tl_index;
                var index1 = (num9 + num8)*3;
                int index2;
                if ((index2 = numArray1[index1]) == 0)
                {
                    num3 >>= numArray1[index1 + 1];
                    num4 -= numArray1[index1 + 1];
                    s.window[destinationIndex++] = (byte) numArray1[index1 + 2];
                    --num5;
                }
                else
                {
                    bool flag;
                    while (true)
                    {
                        num3 >>= numArray1[index1 + 1];
                        num4 -= numArray1[index1 + 1];
                        if ((index2 & 16) == 0)
                        {
                            if ((index2 & 64) == 0)
                            {
                                num8 = num8 + numArray1[index1 + 2] + (num3 & InternalInflateConstants.InflateMask[index2]);
                                index1 = (num9 + num8)*3;
                                if ((index2 = numArray1[index1]) != 0)
                                    flag = true;
                                else
                                    goto label_34;
                            }
                            else
                                goto label_35;
                        }
                        else
                            break;
                    }
                    var index3 = index2 & 15;
                    var length1 = numArray1[index1 + 2] + (num3 & InternalInflateConstants.InflateMask[index3]);
                    var num10 = num3 >> index3;
                    var num11 = num4 - index3;
                    while (num11 < 15)
                    {
                        --num2;
                        num10 |= (z.InputBuffer[num1++] & byte.MaxValue) << num11;
                        num11 += 8;
                    }
                    var num12 = num10 & num7;
                    var numArray2 = td;
                    var num13 = td_index;
                    var index4 = (num13 + num12)*3;
                    var index5 = numArray2[index4];
                    while (true)
                    {
                        num10 >>= numArray2[index4 + 1];
                        num11 -= numArray2[index4 + 1];
                        if ((index5 & 16) == 0)
                        {
                            if ((index5 & 64) == 0)
                            {
                                num12 = num12 + numArray2[index4 + 2] + (num10 & InternalInflateConstants.InflateMask[index5]);
                                index4 = (num13 + num12)*3;
                                index5 = numArray2[index4];
                                flag = true;
                            }
                            else
                                goto label_31;
                        }
                        else
                            break;
                    }
                    var index6 = index5 & 15;
                    while (num11 < index6)
                    {
                        --num2;
                        num10 |= (z.InputBuffer[num1++] & byte.MaxValue) << num11;
                        num11 += 8;
                    }
                    var num14 = numArray2[index4 + 2] + (num10 & InternalInflateConstants.InflateMask[index6]);
                    num3 = num10 >> index6;
                    num4 = num11 - index6;
                    num5 -= length1;
                    int sourceIndex1;
                    int num15;
                    if (destinationIndex >= num14)
                    {
                        var sourceIndex2 = destinationIndex - num14;
                        if (destinationIndex - sourceIndex2 > 0 && 2 > destinationIndex - sourceIndex2)
                        {
                            var numArray3 = s.window;
                            var index7 = destinationIndex;
                            var num16 = 1;
                            var num17 = index7 + num16;
                            var numArray4 = s.window;
                            var index8 = sourceIndex2;
                            var num18 = 1;
                            var num19 = index8 + num18;
                            int num20 = numArray4[index8];
                            numArray3[index7] = (byte) num20;
                            var numArray5 = s.window;
                            var index9 = num17;
                            var num21 = 1;
                            destinationIndex = index9 + num21;
                            var numArray6 = s.window;
                            var index10 = num19;
                            var num22 = 1;
                            sourceIndex1 = index10 + num22;
                            int num23 = numArray6[index10];
                            numArray5[index9] = (byte) num23;
                            length1 -= 2;
                        }
                        else
                        {
                            Array.Copy(s.window, sourceIndex2, s.window, destinationIndex, 2);
                            destinationIndex += 2;
                            sourceIndex1 = sourceIndex2 + 2;
                            length1 -= 2;
                        }
                    }
                    else
                    {
                        sourceIndex1 = destinationIndex - num14;
                        do
                        {
                            sourceIndex1 += s.end;
                        } while (sourceIndex1 < 0);
                        var length2 = s.end - sourceIndex1;
                        if (length1 > length2)
                        {
                            length1 -= length2;
                            if (destinationIndex - sourceIndex1 > 0 && length2 > destinationIndex - sourceIndex1)
                            {
                                do
                                {
                                    s.window[destinationIndex++] = s.window[sourceIndex1++];
                                } while (--length2 != 0);
                            }
                            else
                            {
                                Array.Copy(s.window, sourceIndex1, s.window, destinationIndex, length2);
                                destinationIndex += length2;
                                num15 = sourceIndex1 + length2;
                            }
                            sourceIndex1 = 0;
                        }
                    }
                    if (destinationIndex - sourceIndex1 > 0 && length1 > destinationIndex - sourceIndex1)
                    {
                        do
                        {
                            s.window[destinationIndex++] = s.window[sourceIndex1++];
                        } while (--length1 != 0);
                        goto label_39;
                    }
                    Array.Copy(s.window, sourceIndex1, s.window, destinationIndex, length1);
                    destinationIndex += length1;
                    num15 = sourceIndex1 + length1;
                    goto label_39;
                    label_31:
                    z.Message = "invalid distance code";
                    var num24 = z.AvailableBytesIn - num2;
                    var num25 = num11 >> 3 < num24 ? num11 >> 3 : num24;
                    var num26 = num2 + num25;
                    var num27 = num1 - num25;
                    var num28 = num11 - (num25 << 3);
                    s.bitb = num10;
                    s.bitk = num28;
                    z.AvailableBytesIn = num26;
                    z.TotalBytesIn += num27 - z.NextIn;
                    z.NextIn = num27;
                    s.writeAt = destinationIndex;
                    return -3;
                    label_34:
                    num3 >>= numArray1[index1 + 1];
                    num4 -= numArray1[index1 + 1];
                    s.window[destinationIndex++] = (byte) numArray1[index1 + 2];
                    --num5;
                    goto label_39;
                    label_35:
                    if ((index2 & 32) != 0)
                    {
                        var num16 = z.AvailableBytesIn - num2;
                        var num17 = num4 >> 3 < num16 ? num4 >> 3 : num16;
                        var num18 = num2 + num17;
                        var num19 = num1 - num17;
                        var num20 = num4 - (num17 << 3);
                        s.bitb = num3;
                        s.bitk = num20;
                        z.AvailableBytesIn = num18;
                        z.TotalBytesIn += num19 - z.NextIn;
                        z.NextIn = num19;
                        s.writeAt = destinationIndex;
                        return 1;
                    }
                    z.Message = "invalid literal/length code";
                    var num29 = z.AvailableBytesIn - num2;
                    var num30 = num4 >> 3 < num29 ? num4 >> 3 : num29;
                    var num31 = num2 + num30;
                    var num32 = num1 - num30;
                    var num33 = num4 - (num30 << 3);
                    s.bitb = num3;
                    s.bitk = num33;
                    z.AvailableBytesIn = num31;
                    z.TotalBytesIn += num32 - z.NextIn;
                    z.NextIn = num32;
                    s.writeAt = destinationIndex;
                    return -3;
                    label_39:
                    ;
                }
            } while (num5 >= 258 && num2 >= 10);
            var num34 = z.AvailableBytesIn - num2;
            var num35 = num4 >> 3 < num34 ? num4 >> 3 : num34;
            var num36 = num2 + num35;
            var num37 = num1 - num35;
            var num38 = num4 - (num35 << 3);
            s.bitb = num3;
            s.bitk = num38;
            z.AvailableBytesIn = num36;
            z.TotalBytesIn += num37 - z.NextIn;
            z.NextIn = num37;
            s.writeAt = destinationIndex;
            return 0;
        }
    }
} 
 
namespace Universe.TinyGZip.InternalImplementation
{
    internal sealed class InflateManager
    {
        private const int PRESET_DICT = 32;
        private const int Z_DEFLATED = 8;

        private static readonly byte[] mark = new byte[4]
        {
            0,
            0,
            byte.MaxValue,
            byte.MaxValue
        };

        internal ZlibCodec _codec;
        private bool _handleRfc1950HeaderBytes = true;
        internal InflateBlocks blocks;
        internal uint computedCheck;
        internal uint expectedCheck;
        internal int marker;
        internal int method;
        private InflateManagerMode mode;
        internal int wbits;

        public InflateManager()
        {
        }

        public InflateManager(bool expectRfc1950HeaderBytes)
        {
            _handleRfc1950HeaderBytes = expectRfc1950HeaderBytes;
        }

        internal bool HandleRfc1950HeaderBytes
        {
            get { return _handleRfc1950HeaderBytes; }
            set { _handleRfc1950HeaderBytes = value; }
        }

        internal int Reset()
        {
            _codec.TotalBytesIn = _codec.TotalBytesOut = 0L;
            _codec.Message = null;
            mode = HandleRfc1950HeaderBytes ? InflateManagerMode.METHOD : InflateManagerMode.BLOCKS;
            var num = (int) blocks.Reset();
            return 0;
        }

        internal int End()
        {
            if (blocks != null)
                blocks.Free();
            blocks = null;
            return 0;
        }

        internal int Initialize(ZlibCodec codec, int w)
        {
            _codec = codec;
            _codec.Message = null;
            blocks = null;
            if (w < 8 || w > 15)
            {
                End();
                throw new ZlibException("Bad window size.");
            }
            wbits = w;
            blocks = new InflateBlocks(codec, HandleRfc1950HeaderBytes ? this : null, 1 << w);
            Reset();
            return 0;
        }

        internal int Inflate(FlushType flush)
        {
            if (_codec.InputBuffer == null)
                throw new ZlibException("InputBuffer is null. ");
            var num1 = 0;
            var r = -5;
            while (true)
            {
                switch (mode)
                {
                    case InflateManagerMode.METHOD:
                        if (_codec.AvailableBytesIn != 0)
                        {
                            r = num1;
                            --_codec.AvailableBytesIn;
                            ++_codec.TotalBytesIn;
                            if (((method = _codec.InputBuffer[_codec.NextIn++]) & 15) != 8)
                            {
                                mode = InflateManagerMode.BAD;
                                _codec.Message = string.Format("unknown compression method (0x{0:X2})", method);
                                marker = 5;
                                break;
                            }
                            if ((method >> 4) + 8 > wbits)
                            {
                                mode = InflateManagerMode.BAD;
                                _codec.Message = string.Format("invalid window size ({0})", (method >> 4) + 8);
                                marker = 5;
                                break;
                            }
                            mode = InflateManagerMode.FLAG;
                            break;
                        }
                        goto label_4;
                    case InflateManagerMode.FLAG:
                        if (_codec.AvailableBytesIn != 0)
                        {
                            r = num1;
                            --_codec.AvailableBytesIn;
                            ++_codec.TotalBytesIn;
                            var num2 = _codec.InputBuffer[_codec.NextIn++] & byte.MaxValue;
                            if (((method << 8) + num2)%31 != 0)
                            {
                                mode = InflateManagerMode.BAD;
                                _codec.Message = "incorrect header check";
                                marker = 5;
                                break;
                            }
                            mode = (num2 & 32) == 0 ? InflateManagerMode.BLOCKS : InflateManagerMode.DICT4;
                            break;
                        }
                        goto label_11;
                    case InflateManagerMode.DICT4:
                        if (_codec.AvailableBytesIn != 0)
                        {
                            r = num1;
                            --_codec.AvailableBytesIn;
                            ++_codec.TotalBytesIn;
                            expectedCheck = (uint) ((ulong) (_codec.InputBuffer[_codec.NextIn++] << 24) & 4278190080UL);
                            mode = InflateManagerMode.DICT3;
                            break;
                        }
                        goto label_16;
                    case InflateManagerMode.DICT3:
                        if (_codec.AvailableBytesIn != 0)
                        {
                            r = num1;
                            --_codec.AvailableBytesIn;
                            ++_codec.TotalBytesIn;
                            expectedCheck += (uint) (_codec.InputBuffer[_codec.NextIn++] << 16 & 16711680);
                            mode = InflateManagerMode.DICT2;
                            break;
                        }
                        goto label_19;
                    case InflateManagerMode.DICT2:
                        if (_codec.AvailableBytesIn != 0)
                        {
                            r = num1;
                            --_codec.AvailableBytesIn;
                            ++_codec.TotalBytesIn;
                            expectedCheck += (uint) (_codec.InputBuffer[_codec.NextIn++] << 8 & 65280);
                            mode = InflateManagerMode.DICT1;
                            break;
                        }
                        goto label_22;
                    case InflateManagerMode.DICT1:
                        goto label_24;
                    case InflateManagerMode.DICT0:
                        goto label_27;
                    case InflateManagerMode.BLOCKS:
                        r = blocks.Process(r);
                        if (r == -3)
                        {
                            mode = InflateManagerMode.BAD;
                            marker = 0;
                            break;
                        }
                        if (r == 0)
                            r = num1;
                        if (r == 1)
                        {
                            r = num1;
                            computedCheck = blocks.Reset();
                            if (HandleRfc1950HeaderBytes)
                            {
                                mode = InflateManagerMode.CHECK4;
                                break;
                            }
                            goto label_35;
                        }
                        goto label_33;
                    case InflateManagerMode.CHECK4:
                        if (_codec.AvailableBytesIn != 0)
                        {
                            r = num1;
                            --_codec.AvailableBytesIn;
                            ++_codec.TotalBytesIn;
                            expectedCheck = (uint) ((ulong) (_codec.InputBuffer[_codec.NextIn++] << 24) & 4278190080UL);
                            mode = InflateManagerMode.CHECK3;
                            break;
                        }
                        goto label_38;
                    case InflateManagerMode.CHECK3:
                        if (_codec.AvailableBytesIn != 0)
                        {
                            r = num1;
                            --_codec.AvailableBytesIn;
                            ++_codec.TotalBytesIn;
                            expectedCheck += (uint) (_codec.InputBuffer[_codec.NextIn++] << 16 & 16711680);
                            mode = InflateManagerMode.CHECK2;
                            break;
                        }
                        goto label_41;
                    case InflateManagerMode.CHECK2:
                        if (_codec.AvailableBytesIn != 0)
                        {
                            r = num1;
                            --_codec.AvailableBytesIn;
                            ++_codec.TotalBytesIn;
                            expectedCheck += (uint) (_codec.InputBuffer[_codec.NextIn++] << 8 & 65280);
                            mode = InflateManagerMode.CHECK1;
                            break;
                        }
                        goto label_44;
                    case InflateManagerMode.CHECK1:
                        if (_codec.AvailableBytesIn != 0)
                        {
                            r = num1;
                            --_codec.AvailableBytesIn;
                            ++_codec.TotalBytesIn;
                            expectedCheck += _codec.InputBuffer[_codec.NextIn++] & (uint) byte.MaxValue;
                            if ((int) computedCheck != (int) expectedCheck)
                            {
                                mode = InflateManagerMode.BAD;
                                _codec.Message = "incorrect data check";
                                marker = 5;
                                break;
                            }
                            goto label_50;
                        }
                        goto label_47;
                    case InflateManagerMode.DONE:
                        goto label_51;
                    case InflateManagerMode.BAD:
                        goto label_52;
                    default:
                        goto label_53;
                }
            }
            label_4:
            return r;
            label_11:
            return r;
            label_16:
            return r;
            label_19:
            return r;
            label_22:
            return r;
            label_24:
            if (_codec.AvailableBytesIn == 0)
                return r;
            --_codec.AvailableBytesIn;
            ++_codec.TotalBytesIn;
            expectedCheck += _codec.InputBuffer[_codec.NextIn++] & (uint) byte.MaxValue;
            _codec._Adler32 = expectedCheck;
            mode = InflateManagerMode.DICT0;
            return 2;
            label_27:
            mode = InflateManagerMode.BAD;
            _codec.Message = "need dictionary";
            marker = 0;
            return -2;
            label_33:
            return r;
            label_35:
            mode = InflateManagerMode.DONE;
            return 1;
            label_38:
            return r;
            label_41:
            return r;
            label_44:
            return r;
            label_47:
            return r;
            label_50:
            mode = InflateManagerMode.DONE;
            return 1;
            label_51:
            return 1;
            label_52:
            throw new ZlibException(string.Format("Bad state ({0})", _codec.Message));
            label_53:
            throw new ZlibException("Stream error.");
        }

        internal int SetDictionary(byte[] dictionary)
        {
            var start = 0;
            var n = dictionary.Length;
            if (mode != InflateManagerMode.DICT0)
                throw new ZlibException("Stream error.");
            if ((int) Adler.Adler32(1U, dictionary, 0, dictionary.Length) != (int) _codec._Adler32)
                return -3;
            _codec._Adler32 = Adler.Adler32(0U, null, 0, 0);
            if (n >= 1 << wbits)
            {
                n = (1 << wbits) - 1;
                start = dictionary.Length - n;
            }
            blocks.SetDictionary(dictionary, start, n);
            mode = InflateManagerMode.BLOCKS;
            return 0;
        }

        internal int Sync()
        {
            if (mode != InflateManagerMode.BAD)
            {
                mode = InflateManagerMode.BAD;
                marker = 0;
            }
            int num1;
            if ((num1 = _codec.AvailableBytesIn) == 0)
                return -5;
            var index1 = _codec.NextIn;
            int index2;
            for (index2 = marker; num1 != 0 && index2 < 4; --num1)
            {
                if (_codec.InputBuffer[index1] == mark[index2])
                    ++index2;
                else
                    index2 = (int) _codec.InputBuffer[index1] == 0 ? 4 - index2 : 0;
                ++index1;
            }
            _codec.TotalBytesIn += index1 - _codec.NextIn;
            _codec.NextIn = index1;
            _codec.AvailableBytesIn = num1;
            marker = index2;
            if (index2 != 4)
                return -3;
            var num2 = _codec.TotalBytesIn;
            var num3 = _codec.TotalBytesOut;
            Reset();
            _codec.TotalBytesIn = num2;
            _codec.TotalBytesOut = num3;
            mode = InflateManagerMode.BLOCKS;
            return 0;
        }

        internal int SyncPoint(ZlibCodec z)
        {
            return blocks.SyncPoint();
        }

        private enum InflateManagerMode
        {
            METHOD,
            FLAG,
            DICT4,
            DICT3,
            DICT2,
            DICT1,
            DICT0,
            BLOCKS,
            CHECK4,
            CHECK3,
            CHECK2,
            CHECK1,
            DONE,
            BAD
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    using System;

    internal sealed class InfTree
    {
        private const int MANY = 1440;
        private const int Z_OK = 0;
        private const int Z_STREAM_END = 1;
        private const int Z_NEED_DICT = 2;
        private const int Z_ERRNO = -1;
        private const int Z_STREAM_ERROR = -2;
        private const int Z_DATA_ERROR = -3;
        private const int Z_MEM_ERROR = -4;
        private const int Z_BUF_ERROR = -5;
        private const int Z_VERSION_ERROR = -6;
        internal const int fixed_bl = 9;
        internal const int fixed_bd = 5;
        internal const int BMAX = 15;

        internal static readonly int[] fixed_tl = new int[1536]
        {
            96,
            7,
            256,
            0,
            8,
            80,
            0,
            8,
            16,
            84,
            8,
            115,
            82,
            7,
            31,
            0,
            8,
            112,
            0,
            8,
            48,
            0,
            9,
            192,
            80,
            7,
            10,
            0,
            8,
            96,
            0,
            8,
            32,
            0,
            9,
            160,
            0,
            8,
            0,
            0,
            8,
            128,
            0,
            8,
            64,
            0,
            9,
            224,
            80,
            7,
            6,
            0,
            8,
            88,
            0,
            8,
            24,
            0,
            9,
            144,
            83,
            7,
            59,
            0,
            8,
            120,
            0,
            8,
            56,
            0,
            9,
            208,
            81,
            7,
            17,
            0,
            8,
            104,
            0,
            8,
            40,
            0,
            9,
            176,
            0,
            8,
            8,
            0,
            8,
            136,
            0,
            8,
            72,
            0,
            9,
            240,
            80,
            7,
            4,
            0,
            8,
            84,
            0,
            8,
            20,
            85,
            8,
            227,
            83,
            7,
            43,
            0,
            8,
            116,
            0,
            8,
            52,
            0,
            9,
            200,
            81,
            7,
            13,
            0,
            8,
            100,
            0,
            8,
            36,
            0,
            9,
            168,
            0,
            8,
            4,
            0,
            8,
            132,
            0,
            8,
            68,
            0,
            9,
            232,
            80,
            7,
            8,
            0,
            8,
            92,
            0,
            8,
            28,
            0,
            9,
            152,
            84,
            7,
            83,
            0,
            8,
            124,
            0,
            8,
            60,
            0,
            9,
            216,
            82,
            7,
            23,
            0,
            8,
            108,
            0,
            8,
            44,
            0,
            9,
            184,
            0,
            8,
            12,
            0,
            8,
            140,
            0,
            8,
            76,
            0,
            9,
            248,
            80,
            7,
            3,
            0,
            8,
            82,
            0,
            8,
            18,
            85,
            8,
            163,
            83,
            7,
            35,
            0,
            8,
            114,
            0,
            8,
            50,
            0,
            9,
            196,
            81,
            7,
            11,
            0,
            8,
            98,
            0,
            8,
            34,
            0,
            9,
            164,
            0,
            8,
            2,
            0,
            8,
            130,
            0,
            8,
            66,
            0,
            9,
            228,
            80,
            7,
            7,
            0,
            8,
            90,
            0,
            8,
            26,
            0,
            9,
            148,
            84,
            7,
            67,
            0,
            8,
            122,
            0,
            8,
            58,
            0,
            9,
            212,
            82,
            7,
            19,
            0,
            8,
            106,
            0,
            8,
            42,
            0,
            9,
            180,
            0,
            8,
            10,
            0,
            8,
            138,
            0,
            8,
            74,
            0,
            9,
            244,
            80,
            7,
            5,
            0,
            8,
            86,
            0,
            8,
            22,
            192,
            8,
            0,
            83,
            7,
            51,
            0,
            8,
            118,
            0,
            8,
            54,
            0,
            9,
            204,
            81,
            7,
            15,
            0,
            8,
            102,
            0,
            8,
            38,
            0,
            9,
            172,
            0,
            8,
            6,
            0,
            8,
            134,
            0,
            8,
            70,
            0,
            9,
            236,
            80,
            7,
            9,
            0,
            8,
            94,
            0,
            8,
            30,
            0,
            9,
            156,
            84,
            7,
            99,
            0,
            8,
            126,
            0,
            8,
            62,
            0,
            9,
            220,
            82,
            7,
            27,
            0,
            8,
            110,
            0,
            8,
            46,
            0,
            9,
            188,
            0,
            8,
            14,
            0,
            8,
            142,
            0,
            8,
            78,
            0,
            9,
            252,
            96,
            7,
            256,
            0,
            8,
            81,
            0,
            8,
            17,
            85,
            8,
            131,
            82,
            7,
            31,
            0,
            8,
            113,
            0,
            8,
            49,
            0,
            9,
            194,
            80,
            7,
            10,
            0,
            8,
            97,
            0,
            8,
            33,
            0,
            9,
            162,
            0,
            8,
            1,
            0,
            8,
            129,
            0,
            8,
            65,
            0,
            9,
            226,
            80,
            7,
            6,
            0,
            8,
            89,
            0,
            8,
            25,
            0,
            9,
            146,
            83,
            7,
            59,
            0,
            8,
            121,
            0,
            8,
            57,
            0,
            9,
            210,
            81,
            7,
            17,
            0,
            8,
            105,
            0,
            8,
            41,
            0,
            9,
            178,
            0,
            8,
            9,
            0,
            8,
            137,
            0,
            8,
            73,
            0,
            9,
            242,
            80,
            7,
            4,
            0,
            8,
            85,
            0,
            8,
            21,
            80,
            8,
            258,
            83,
            7,
            43,
            0,
            8,
            117,
            0,
            8,
            53,
            0,
            9,
            202,
            81,
            7,
            13,
            0,
            8,
            101,
            0,
            8,
            37,
            0,
            9,
            170,
            0,
            8,
            5,
            0,
            8,
            133,
            0,
            8,
            69,
            0,
            9,
            234,
            80,
            7,
            8,
            0,
            8,
            93,
            0,
            8,
            29,
            0,
            9,
            154,
            84,
            7,
            83,
            0,
            8,
            125,
            0,
            8,
            61,
            0,
            9,
            218,
            82,
            7,
            23,
            0,
            8,
            109,
            0,
            8,
            45,
            0,
            9,
            186,
            0,
            8,
            13,
            0,
            8,
            141,
            0,
            8,
            77,
            0,
            9,
            250,
            80,
            7,
            3,
            0,
            8,
            83,
            0,
            8,
            19,
            85,
            8,
            195,
            83,
            7,
            35,
            0,
            8,
            115,
            0,
            8,
            51,
            0,
            9,
            198,
            81,
            7,
            11,
            0,
            8,
            99,
            0,
            8,
            35,
            0,
            9,
            166,
            0,
            8,
            3,
            0,
            8,
            131,
            0,
            8,
            67,
            0,
            9,
            230,
            80,
            7,
            7,
            0,
            8,
            91,
            0,
            8,
            27,
            0,
            9,
            150,
            84,
            7,
            67,
            0,
            8,
            123,
            0,
            8,
            59,
            0,
            9,
            214,
            82,
            7,
            19,
            0,
            8,
            107,
            0,
            8,
            43,
            0,
            9,
            182,
            0,
            8,
            11,
            0,
            8,
            139,
            0,
            8,
            75,
            0,
            9,
            246,
            80,
            7,
            5,
            0,
            8,
            87,
            0,
            8,
            23,
            192,
            8,
            0,
            83,
            7,
            51,
            0,
            8,
            119,
            0,
            8,
            55,
            0,
            9,
            206,
            81,
            7,
            15,
            0,
            8,
            103,
            0,
            8,
            39,
            0,
            9,
            174,
            0,
            8,
            7,
            0,
            8,
            135,
            0,
            8,
            71,
            0,
            9,
            238,
            80,
            7,
            9,
            0,
            8,
            95,
            0,
            8,
            31,
            0,
            9,
            158,
            84,
            7,
            99,
            0,
            8,
            sbyte.MaxValue,
            0,
            8,
            63,
            0,
            9,
            222,
            82,
            7,
            27,
            0,
            8,
            111,
            0,
            8,
            47,
            0,
            9,
            190,
            0,
            8,
            15,
            0,
            8,
            143,
            0,
            8,
            79,
            0,
            9,
            254,
            96,
            7,
            256,
            0,
            8,
            80,
            0,
            8,
            16,
            84,
            8,
            115,
            82,
            7,
            31,
            0,
            8,
            112,
            0,
            8,
            48,
            0,
            9,
            193,
            80,
            7,
            10,
            0,
            8,
            96,
            0,
            8,
            32,
            0,
            9,
            161,
            0,
            8,
            0,
            0,
            8,
            128,
            0,
            8,
            64,
            0,
            9,
            225,
            80,
            7,
            6,
            0,
            8,
            88,
            0,
            8,
            24,
            0,
            9,
            145,
            83,
            7,
            59,
            0,
            8,
            120,
            0,
            8,
            56,
            0,
            9,
            209,
            81,
            7,
            17,
            0,
            8,
            104,
            0,
            8,
            40,
            0,
            9,
            177,
            0,
            8,
            8,
            0,
            8,
            136,
            0,
            8,
            72,
            0,
            9,
            241,
            80,
            7,
            4,
            0,
            8,
            84,
            0,
            8,
            20,
            85,
            8,
            227,
            83,
            7,
            43,
            0,
            8,
            116,
            0,
            8,
            52,
            0,
            9,
            201,
            81,
            7,
            13,
            0,
            8,
            100,
            0,
            8,
            36,
            0,
            9,
            169,
            0,
            8,
            4,
            0,
            8,
            132,
            0,
            8,
            68,
            0,
            9,
            233,
            80,
            7,
            8,
            0,
            8,
            92,
            0,
            8,
            28,
            0,
            9,
            153,
            84,
            7,
            83,
            0,
            8,
            124,
            0,
            8,
            60,
            0,
            9,
            217,
            82,
            7,
            23,
            0,
            8,
            108,
            0,
            8,
            44,
            0,
            9,
            185,
            0,
            8,
            12,
            0,
            8,
            140,
            0,
            8,
            76,
            0,
            9,
            249,
            80,
            7,
            3,
            0,
            8,
            82,
            0,
            8,
            18,
            85,
            8,
            163,
            83,
            7,
            35,
            0,
            8,
            114,
            0,
            8,
            50,
            0,
            9,
            197,
            81,
            7,
            11,
            0,
            8,
            98,
            0,
            8,
            34,
            0,
            9,
            165,
            0,
            8,
            2,
            0,
            8,
            130,
            0,
            8,
            66,
            0,
            9,
            229,
            80,
            7,
            7,
            0,
            8,
            90,
            0,
            8,
            26,
            0,
            9,
            149,
            84,
            7,
            67,
            0,
            8,
            122,
            0,
            8,
            58,
            0,
            9,
            213,
            82,
            7,
            19,
            0,
            8,
            106,
            0,
            8,
            42,
            0,
            9,
            181,
            0,
            8,
            10,
            0,
            8,
            138,
            0,
            8,
            74,
            0,
            9,
            245,
            80,
            7,
            5,
            0,
            8,
            86,
            0,
            8,
            22,
            192,
            8,
            0,
            83,
            7,
            51,
            0,
            8,
            118,
            0,
            8,
            54,
            0,
            9,
            205,
            81,
            7,
            15,
            0,
            8,
            102,
            0,
            8,
            38,
            0,
            9,
            173,
            0,
            8,
            6,
            0,
            8,
            134,
            0,
            8,
            70,
            0,
            9,
            237,
            80,
            7,
            9,
            0,
            8,
            94,
            0,
            8,
            30,
            0,
            9,
            157,
            84,
            7,
            99,
            0,
            8,
            126,
            0,
            8,
            62,
            0,
            9,
            221,
            82,
            7,
            27,
            0,
            8,
            110,
            0,
            8,
            46,
            0,
            9,
            189,
            0,
            8,
            14,
            0,
            8,
            142,
            0,
            8,
            78,
            0,
            9,
            253,
            96,
            7,
            256,
            0,
            8,
            81,
            0,
            8,
            17,
            85,
            8,
            131,
            82,
            7,
            31,
            0,
            8,
            113,
            0,
            8,
            49,
            0,
            9,
            195,
            80,
            7,
            10,
            0,
            8,
            97,
            0,
            8,
            33,
            0,
            9,
            163,
            0,
            8,
            1,
            0,
            8,
            129,
            0,
            8,
            65,
            0,
            9,
            227,
            80,
            7,
            6,
            0,
            8,
            89,
            0,
            8,
            25,
            0,
            9,
            147,
            83,
            7,
            59,
            0,
            8,
            121,
            0,
            8,
            57,
            0,
            9,
            211,
            81,
            7,
            17,
            0,
            8,
            105,
            0,
            8,
            41,
            0,
            9,
            179,
            0,
            8,
            9,
            0,
            8,
            137,
            0,
            8,
            73,
            0,
            9,
            243,
            80,
            7,
            4,
            0,
            8,
            85,
            0,
            8,
            21,
            80,
            8,
            258,
            83,
            7,
            43,
            0,
            8,
            117,
            0,
            8,
            53,
            0,
            9,
            203,
            81,
            7,
            13,
            0,
            8,
            101,
            0,
            8,
            37,
            0,
            9,
            171,
            0,
            8,
            5,
            0,
            8,
            133,
            0,
            8,
            69,
            0,
            9,
            235,
            80,
            7,
            8,
            0,
            8,
            93,
            0,
            8,
            29,
            0,
            9,
            155,
            84,
            7,
            83,
            0,
            8,
            125,
            0,
            8,
            61,
            0,
            9,
            219,
            82,
            7,
            23,
            0,
            8,
            109,
            0,
            8,
            45,
            0,
            9,
            187,
            0,
            8,
            13,
            0,
            8,
            141,
            0,
            8,
            77,
            0,
            9,
            251,
            80,
            7,
            3,
            0,
            8,
            83,
            0,
            8,
            19,
            85,
            8,
            195,
            83,
            7,
            35,
            0,
            8,
            115,
            0,
            8,
            51,
            0,
            9,
            199,
            81,
            7,
            11,
            0,
            8,
            99,
            0,
            8,
            35,
            0,
            9,
            167,
            0,
            8,
            3,
            0,
            8,
            131,
            0,
            8,
            67,
            0,
            9,
            231,
            80,
            7,
            7,
            0,
            8,
            91,
            0,
            8,
            27,
            0,
            9,
            151,
            84,
            7,
            67,
            0,
            8,
            123,
            0,
            8,
            59,
            0,
            9,
            215,
            82,
            7,
            19,
            0,
            8,
            107,
            0,
            8,
            43,
            0,
            9,
            183,
            0,
            8,
            11,
            0,
            8,
            139,
            0,
            8,
            75,
            0,
            9,
            247,
            80,
            7,
            5,
            0,
            8,
            87,
            0,
            8,
            23,
            192,
            8,
            0,
            83,
            7,
            51,
            0,
            8,
            119,
            0,
            8,
            55,
            0,
            9,
            207,
            81,
            7,
            15,
            0,
            8,
            103,
            0,
            8,
            39,
            0,
            9,
            175,
            0,
            8,
            7,
            0,
            8,
            135,
            0,
            8,
            71,
            0,
            9,
            239,
            80,
            7,
            9,
            0,
            8,
            95,
            0,
            8,
            31,
            0,
            9,
            159,
            84,
            7,
            99,
            0,
            8,
            sbyte.MaxValue,
            0,
            8,
            63,
            0,
            9,
            223,
            82,
            7,
            27,
            0,
            8,
            111,
            0,
            8,
            47,
            0,
            9,
            191,
            0,
            8,
            15,
            0,
            8,
            143,
            0,
            8,
            79,
            0,
            9,
            byte.MaxValue
        };

        internal static readonly int[] fixed_td = new int[96]
        {
            80,
            5,
            1,
            87,
            5,
            257,
            83,
            5,
            17,
            91,
            5,
            4097,
            81,
            5,
            5,
            89,
            5,
            1025,
            85,
            5,
            65,
            93,
            5,
            16385,
            80,
            5,
            3,
            88,
            5,
            513,
            84,
            5,
            33,
            92,
            5,
            8193,
            82,
            5,
            9,
            90,
            5,
            2049,
            86,
            5,
            129,
            192,
            5,
            24577,
            80,
            5,
            2,
            87,
            5,
            385,
            83,
            5,
            25,
            91,
            5,
            6145,
            81,
            5,
            7,
            89,
            5,
            1537,
            85,
            5,
            97,
            93,
            5,
            24577,
            80,
            5,
            4,
            88,
            5,
            769,
            84,
            5,
            49,
            92,
            5,
            12289,
            82,
            5,
            13,
            90,
            5,
            3073,
            86,
            5,
            193,
            192,
            5,
            24577
        };

        internal static readonly int[] cplens = new int[31]
        {
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            13,
            15,
            17,
            19,
            23,
            27,
            31,
            35,
            43,
            51,
            59,
            67,
            83,
            99,
            115,
            131,
            163,
            195,
            227,
            258,
            0,
            0
        };

        internal static readonly int[] cplext = new int[31]
        {
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            1,
            1,
            1,
            1,
            2,
            2,
            2,
            2,
            3,
            3,
            3,
            3,
            4,
            4,
            4,
            4,
            5,
            5,
            5,
            5,
            0,
            112,
            112
        };

        internal static readonly int[] cpdist = new int[30]
        {
            1,
            2,
            3,
            4,
            5,
            7,
            9,
            13,
            17,
            25,
            33,
            49,
            65,
            97,
            129,
            193,
            257,
            385,
            513,
            769,
            1025,
            1537,
            2049,
            3073,
            4097,
            6145,
            8193,
            12289,
            16385,
            24577
        };

        internal static readonly int[] cpdext = new int[30]
        {
            0,
            0,
            0,
            0,
            1,
            1,
            2,
            2,
            3,
            3,
            4,
            4,
            5,
            5,
            6,
            6,
            7,
            7,
            8,
            8,
            9,
            9,
            10,
            10,
            11,
            11,
            12,
            12,
            13,
            13
        };

        internal int[] c;
        internal int[] hn;
        internal int[] r;
        internal int[] u;
        internal int[] v;
        internal int[] x;

        private int huft_build(int[] b, int bindex, int n, int s, int[] d, int[] e, int[] t, int[] m, int[] hp, int[] hn, int[] v)
        {
            var num1 = 0;
            var num2 = n;
            do
            {
                ++c[b[bindex + num1]];
                ++num1;
                --num2;
            } while (num2 != 0);
            if (c[0] == n)
            {
                t[0] = -1;
                m[0] = 0;
                return 0;
            }
            var num3 = m[0];
            var index1 = 1;
            while (index1 <= 15 && c[index1] == 0)
                ++index1;
            var index2 = index1;
            if (num3 < index1)
                num3 = index1;
            var index3 = 15;
            while (index3 != 0 && c[index3] == 0)
                --index3;
            var index4 = index3;
            if (num3 > index3)
                num3 = index3;
            m[0] = num3;
            var num4 = 1 << index1;
            while (index1 < index3)
            {
                int num5;
                if ((num5 = num4 - c[index1]) < 0)
                    return -3;
                ++index1;
                num4 = num5 << 1;
            }
            int num6;
            if ((num6 = num4 - c[index3]) < 0)
                return -3;
            c[index3] += num6;
            int num7;
            x[1] = num7 = 0;
            var index5 = 1;
            var index6 = 2;
            while (--index3 != 0)
            {
                x[index6] = (num7 += c[index5]);
                ++index6;
                ++index5;
            }
            var num8 = 0;
            var num9 = 0;
            do
            {
                int index7;
                if ((index7 = b[bindex + num9]) != 0)
                    v[x[index7]++] = num8;
                ++num9;
            } while (++num8 < n);
            n = x[index4];
            int number1;
            x[0] = number1 = 0;
            var index8 = 0;
            var index9 = -1;
            var bits = -num3;
            u[0] = 0;
            var num10 = 0;
            var num11 = 0;
            for (; index2 <= index4; ++index2)
            {
                var num5 = c[index2];
                while (num5-- != 0)
                {
                    while (index2 > bits + num3)
                    {
                        ++index9;
                        bits += num3;
                        var num12 = index4 - bits;
                        var num13 = num12 > num3 ? num3 : num12;
                        int num14;
                        int num15;
                        if ((num15 = 1 << (num14 = index2 - bits)) > num5 + 1)
                        {
                            var num16 = num15 - (num5 + 1);
                            var index7 = index2;
                            if (num14 < num13)
                            {
                                int num17;
                                while (++num14 < num13 && (num17 = num16 << 1) > c[++index7])
                                    num16 = num17 - c[index7];
                            }
                        }
                        num11 = 1 << num14;
                        if (hn[0] + num11 > 1440)
                            return -3;
                        u[index9] = num10 = hn[0];
                        hn[0] += num11;
                        if (index9 != 0)
                        {
                            x[index9] = number1;
                            r[0] = (sbyte) num14;
                            r[1] = (sbyte) num3;
                            var num16 = SharedUtils.URShift(number1, bits - num3);
                            r[2] = num10 - u[index9 - 1] - num16;
                            Array.Copy(r, 0, hp, (u[index9 - 1] + num16)*3, 3);
                        }
                        else
                            t[0] = num10;
                    }
                    r[1] = (sbyte) (index2 - bits);
                    if (index8 >= n)
                        r[0] = 192;
                    else if (v[index8] < s)
                    {
                        r[0] = v[index8] < 256 ? 0 : 96;
                        r[2] = v[index8++];
                    }
                    else
                    {
                        r[0] = (sbyte) (e[v[index8] - s] + 16 + 64);
                        r[2] = d[v[index8++] - s];
                    }
                    var num18 = 1 << index2 - bits;
                    var num19 = SharedUtils.URShift(number1, bits);
                    while (num19 < num11)
                    {
                        Array.Copy(r, 0, hp, (num10 + num19)*3, 3);
                        num19 += num18;
                    }
                    int number2;
                    for (number2 = 1 << index2 - 1; (number1 & number2) != 0; number2 = SharedUtils.URShift(number2, 1))
                        number1 ^= number2;
                    number1 ^= number2;
                    for (var index7 = (1 << bits) - 1; (number1 & index7) != x[index9]; index7 = (1 << bits) - 1)
                    {
                        --index9;
                        bits -= num3;
                    }
                }
            }
            return num6 == 0 || index4 == 1 ? 0 : -5;
        }

        internal int inflate_trees_bits(int[] c, int[] bb, int[] tb, int[] hp, ZlibCodec z)
        {
            initWorkArea(19);
            hn[0] = 0;
            var num1 = huft_build(c, 0, 19, 19, null, null, tb, bb, hp, hn, v);
            int num2;
            switch (num1)
            {
                case -3:
                    z.Message = "oversubscribed dynamic bit lengths tree";
                    goto label_6;
                case -5:
                    num2 = 0;
                    break;
                default:
                    num2 = bb[0] != 0 ? 1 : 0;
                    break;
            }
            if (num2 == 0)
            {
                z.Message = "incomplete dynamic bit lengths tree";
                num1 = -3;
            }
            label_6:
            return num1;
        }

        internal int inflate_trees_dynamic(int nl, int nd, int[] c, int[] bl, int[] bd, int[] tl, int[] td, int[] hp, ZlibCodec z)
        {
            initWorkArea(288);
            hn[0] = 0;
            var num1 = huft_build(c, 0, nl, 257, cplens, cplext, tl, bl, hp, hn, v);
            if (num1 != 0 || bl[0] == 0)
            {
                if (num1 == -3)
                    z.Message = "oversubscribed literal/length tree";
                else if (num1 != -4)
                {
                    z.Message = "incomplete literal/length tree";
                    num1 = -3;
                }
                return num1;
            }
            initWorkArea(288);
            var num2 = huft_build(c, nl, nd, 0, cpdist, cpdext, td, bd, hp, hn, v);
            if (num2 == 0 && (bd[0] != 0 || nl <= 257))
                return 0;
            if (num2 == -3)
                z.Message = "oversubscribed distance tree";
            else if (num2 == -5)
            {
                z.Message = "incomplete distance tree";
                num2 = -3;
            }
            else if (num2 != -4)
            {
                z.Message = "empty distance tree with lengths";
                num2 = -3;
            }
            return num2;
        }

        internal static int inflate_trees_fixed(int[] bl, int[] bd, int[][] tl, int[][] td, ZlibCodec z)
        {
            bl[0] = 9;
            bd[0] = 5;
            tl[0] = fixed_tl;
            td[0] = fixed_td;
            return 0;
        }

        private void initWorkArea(int vsize)
        {
            if (hn == null)
            {
                hn = new int[1];
                v = new int[vsize];
                c = new int[16];
                r = new int[3];
                u = new int[15];
                x = new int[16];
            }
            else
            {
                if (v.Length < vsize)
                    v = new int[vsize];
                Array.Clear(v, 0, vsize);
                Array.Clear(c, 0, 16);
                r[0] = 0;
                r[1] = 0;
                r[2] = 0;
                Array.Clear(u, 0, 15);
                Array.Clear(x, 0, 16);
            }
        }
    }
} 
 
﻿namespace Universe.TinyGZip.InternalImplementation
{
    internal static class InternalConstants
    {
        internal static readonly int MAX_BITS = 15;
        internal static readonly int BL_CODES = 19;
        internal static readonly int D_CODES = 30;
        internal static readonly int LITERALS = 256;
        internal static readonly int LENGTH_CODES = 29;
        internal static readonly int L_CODES = LITERALS + 1 + LENGTH_CODES;
        internal static readonly int MAX_BL_BITS = 7;
        internal static readonly int REP_3_6 = 16;
        internal static readonly int REPZ_3_10 = 17;
        internal static readonly int REPZ_11_138 = 18;
    }
} 
 
﻿namespace Universe.TinyGZip.InternalImplementation
{
    internal static class InternalInflateConstants
    {
        internal static readonly int[] InflateMask = new int[17]
        {
            0,
            1,
            3,
            7,
            15,
            31,
            63,
            sbyte.MaxValue,
            byte.MaxValue,
            511,
            1023,
            2047,
            4095,
            8191,
            16383,
            short.MaxValue,
            ushort.MaxValue
        };
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    using System;
    using System.Text;

    public class Iso8859Dash1Encoding : Encoding
    {
        public override string WebName
        {
            get { return "iso-8859-1"; }
        }

        public static int CharacterCount
        {
            get { return 256; }
        }

        public override int GetBytes(char[] chars, int start, int count, byte[] bytes, int byteIndex)
        {
            if (chars == null)
                throw new ArgumentNullException("chars", "null array");
            if (bytes == null)
                throw new ArgumentNullException("bytes", "null array");
            if (start < 0)
                throw new ArgumentOutOfRangeException("start");
            if (count < 0)
                throw new ArgumentOutOfRangeException("charCount");
            if (chars.Length - start < count)
                throw new ArgumentOutOfRangeException("chars");
            if (byteIndex < 0 || byteIndex > bytes.Length)
                throw new ArgumentOutOfRangeException("byteIndex");
            for (var index = 0; index < count; ++index)
            {
                var ch = chars[start + index];
                bytes[byteIndex + index] = (int) ch < (int) byte.MaxValue ? (byte) ch : (byte) 63;
            }
            return count;
        }

        public override int GetChars(byte[] bytes, int start, int count, char[] chars, int charIndex)
        {
            if (chars == null)
                throw new ArgumentNullException("chars", "null array");
            if (bytes == null)
                throw new ArgumentNullException("bytes", "null array");
            if (start < 0)
                throw new ArgumentOutOfRangeException("start");
            if (count < 0)
                throw new ArgumentOutOfRangeException("charCount");
            if (bytes.Length - start < count)
                throw new ArgumentOutOfRangeException("bytes");
            if (charIndex < 0 || charIndex > chars.Length)
                throw new ArgumentOutOfRangeException("charIndex");
            for (var index = 0; index < count; ++index)
                chars[charIndex + index] = (char) bytes[index + start];
            return count;
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
    }
} 
 

namespace Universe.TinyGZip
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using InternalImplementation;

    #pragma warning disable 642, 219
    public class ParallelDeflateOutputStream : Stream
    {
        private static readonly int IO_BUFFER_SIZE_DEFAULT = 65536;
        private static readonly int BufferPairsPerCore = 4;
        private int _bufferSize = IO_BUFFER_SIZE_DEFAULT;
        private readonly CompressionLevel _compressLevel;
        private int _currentlyFilling;

        private readonly TraceBits _DesiredTrace = TraceBits.EmitAll | TraceBits.EmitEnter | TraceBits.Session | TraceBits.Compress |
                                                   TraceBits.WriteEnter | TraceBits.WriteTake;

        private readonly object _eLock = new object();
        private bool _firstWriteDone;
        private bool _handlingException;
        private bool _isClosed;
        private int _lastFilled;
        private int _lastWritten;
        private int _latestCompressed;
        private readonly object _latestLock = new object();
        private readonly bool _leaveOpen;
        private int _maxBufferPairs;
        private AutoResetEvent _newlyCompressedBlob;
        private readonly object _outputLock = new object();
        private Stream _outStream;
        private volatile Exception _pendingException;
        private List<WorkItem> _pool;
        private CRC32 _runningCrc;
        private Queue<int> _toFill;
        private Queue<int> _toWrite;
        private bool emitting;

        public ParallelDeflateOutputStream(Stream stream)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, false)
        {
        }

        public ParallelDeflateOutputStream(Stream stream, CompressionLevel level)
            : this(stream, level, CompressionStrategy.Default, false)
        {
        }

        public ParallelDeflateOutputStream(Stream stream, bool leaveOpen)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
        {
        }

        public ParallelDeflateOutputStream(Stream stream, CompressionLevel level, bool leaveOpen)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
        {
        }

        public ParallelDeflateOutputStream(Stream stream, CompressionLevel level, CompressionStrategy strategy, bool leaveOpen)
        {
            _outStream = stream;
            _compressLevel = level;
            Strategy = strategy;
            _leaveOpen = leaveOpen;
            MaxBufferPairs = 16;
        }

        public CompressionStrategy Strategy { get; private set; }

        public int MaxBufferPairs
        {
            get { return _maxBufferPairs; }
            set
            {
                if (value < 4)
                    throw new ArgumentException("MaxBufferPairs", "Value must be 4 or greater.");
                _maxBufferPairs = value;
            }
        }

        public int BufferSize
        {
            get { return _bufferSize; }
            set
            {
                if (value < 1024)
                    throw new ArgumentOutOfRangeException("BufferSize", "BufferSize must be greater than 1024 bytes");
                _bufferSize = value;
            }
        }

        public int Crc32 { get; private set; }

        public long BytesProcessed { get; private set; }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return _outStream.CanWrite; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { return _outStream.Position; }
            set { throw new NotSupportedException(); }
        }

        private void _InitializePoolOfWorkItems()
        {
            _toWrite = new Queue<int>();
            _toFill = new Queue<int>();
            _pool = new List<WorkItem>();
            var num = Math.Min(BufferPairsPerCore*Environment.ProcessorCount, _maxBufferPairs);
            for (var ix = 0; ix < num; ++ix)
            {
                _pool.Add(new WorkItem(_bufferSize, _compressLevel, Strategy, ix));
                _toFill.Enqueue(ix);
            }
            _newlyCompressedBlob = new AutoResetEvent(false);
            _runningCrc = new CRC32();
            _currentlyFilling = -1;
            _lastFilled = -1;
            _lastWritten = -1;
            _latestCompressed = -1;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var mustWait = false;
            if (_isClosed)
                throw new InvalidOperationException();
            if (_pendingException != null)
            {
                _handlingException = true;
                var exception = _pendingException;
                _pendingException = null;
                throw exception;
            }
            if (count == 0)
                return;
            if (!_firstWriteDone)
            {
                _InitializePoolOfWorkItems();
                _firstWriteDone = true;
            }
            do
            {
                EmitPendingBuffers(false, mustWait);
                mustWait = false;
                int index;
                if (_currentlyFilling >= 0)
                    index = _currentlyFilling;
                else if (_toFill.Count == 0)
                {
                    mustWait = true;
                    goto label_20;
                }
                else
                {
                    index = _toFill.Dequeue();
                    ++_lastFilled;
                }
                var workItem = _pool[index];
                var count1 = workItem.buffer.Length - workItem.inputBytesAvailable > count
                    ? count
                    : workItem.buffer.Length - workItem.inputBytesAvailable;
                workItem.ordinal = _lastFilled;
                Buffer.BlockCopy(buffer, offset, workItem.buffer, workItem.inputBytesAvailable, count1);
                count -= count1;
                offset += count1;
                workItem.inputBytesAvailable += count1;
                if (workItem.inputBytesAvailable == workItem.buffer.Length)
                {
                    if (!ThreadPool.QueueUserWorkItem(_DeflateOne, workItem))
                        throw new Exception("Cannot enqueue workitem");
                    _currentlyFilling = -1;
                }
                else
                    _currentlyFilling = index;
                if (count <= 0)
                    ;
                label_20:
                ;
            } while (count > 0);
        }

        private void _FlushFinish()
        {
            var buffer = new byte[128];
            var zlibCodec = new ZlibCodec();
            zlibCodec.InitializeDeflate(_compressLevel, false);
            zlibCodec.InputBuffer = null;
            zlibCodec.NextIn = 0;
            zlibCodec.AvailableBytesIn = 0;
            zlibCodec.OutputBuffer = buffer;
            zlibCodec.NextOut = 0;
            zlibCodec.AvailableBytesOut = buffer.Length;
            var num = zlibCodec.Deflate(FlushType.Finish);
            if (num != 1 && num != 0)
                throw new Exception("deflating: " + zlibCodec.Message);
            if (buffer.Length - zlibCodec.AvailableBytesOut > 0)
                _outStream.Write(buffer, 0, buffer.Length - zlibCodec.AvailableBytesOut);
            zlibCodec.EndDeflate();
            Crc32 = _runningCrc.Crc32Result;
        }

        private void _Flush(bool lastInput)
        {
            if (_isClosed)
                throw new InvalidOperationException();
            if (emitting)
                return;
            if (_currentlyFilling >= 0)
            {
                _DeflateOne(_pool[_currentlyFilling]);
                _currentlyFilling = -1;
            }
            if (lastInput)
            {
                EmitPendingBuffers(true, false);
                _FlushFinish();
            }
            else
                EmitPendingBuffers(false, false);
        }

        public override void Flush()
        {
            if (_pendingException != null)
            {
                _handlingException = true;
                var exception = _pendingException;
                _pendingException = null;
                throw exception;
            }
            if (_handlingException)
                return;
            _Flush(false);
        }

        public override void Close()
        {
            if (_pendingException != null)
            {
                _handlingException = true;
                var exception = _pendingException;
                _pendingException = null;
                throw exception;
            }
            if (_handlingException || _isClosed)
                return;
            _Flush(true);
            if (!_leaveOpen)
                _outStream.Close();
            _isClosed = true;
        }

        public new void Dispose()
        {
            Close();
            _pool = null;
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public void Reset(Stream stream)
        {
            if (!_firstWriteDone)
                return;
            _toWrite.Clear();
            _toFill.Clear();
            foreach (var workItem in _pool)
            {
                _toFill.Enqueue(workItem.index);
                workItem.ordinal = -1;
            }
            _firstWriteDone = false;
            BytesProcessed = 0L;
            _runningCrc = new CRC32();
            _isClosed = false;
            _currentlyFilling = -1;
            _lastFilled = -1;
            _lastWritten = -1;
            _latestCompressed = -1;
            _outStream = stream;
        }

        private void EmitPendingBuffers(bool doAll, bool mustWait)
        {
            if (emitting)
                return;
            emitting = true;
            if (doAll || mustWait)
                _newlyCompressedBlob.WaitOne();
            do
            {
                var num = -1;
                var millisecondsTimeout = doAll ? 200 : (mustWait ? -1 : 0);
                int index;
                do
                {
                    if (Monitor.TryEnter(_toWrite, millisecondsTimeout))
                    {
                        index = -1;
                        try
                        {
                            if (_toWrite.Count > 0)
                                index = _toWrite.Dequeue();
                        }
                        finally
                        {
                            Monitor.Exit(_toWrite);
                        }
                        if (index >= 0)
                        {
                            var workItem = _pool[index];
                            if (workItem.ordinal != _lastWritten + 1)
                            {
                                lock (_toWrite)
                                    _toWrite.Enqueue(index);
                                if (num == index)
                                {
                                    _newlyCompressedBlob.WaitOne();
                                    num = -1;
                                }
                                else if (num == -1)
                                {
                                    num = index;
                                }
                                else
                                    goto label_24;
                            }
                            else
                            {
                                num = -1;
                                _outStream.Write(workItem.compressed, 0, workItem.compressedBytesAvailable);
                                _runningCrc.Combine(workItem.crc, workItem.inputBytesAvailable);
                                BytesProcessed += workItem.inputBytesAvailable;
                                workItem.inputBytesAvailable = 0;
                                _lastWritten = workItem.ordinal;
                                _toFill.Enqueue(workItem.index);
                                if (millisecondsTimeout == -1)
                                    millisecondsTimeout = 0;
                            }
                        }
                    }
                    else
                        index = -1;
                    label_24:
                    ;
                } while (index >= 0);
            } while (doAll && _lastWritten != _latestCompressed);
            emitting = false;
        }

        private void _DeflateOne(object wi)
        {
            var workitem = (WorkItem) wi;
            try
            {
                var num = workitem.index;
                var crC32 = new CRC32();
                crC32.SlurpBlock(workitem.buffer, 0, workitem.inputBytesAvailable);
                DeflateOneSegment(workitem);
                workitem.crc = crC32.Crc32Result;
                lock (_latestLock)
                {
                    if (workitem.ordinal > _latestCompressed)
                        _latestCompressed = workitem.ordinal;
                }
                lock (_toWrite)
                    _toWrite.Enqueue(workitem.index);
                _newlyCompressedBlob.Set();
            }
            catch (Exception ex)
            {
                lock (_eLock)
                {
                    if (_pendingException == null)
                        return;
                    _pendingException = ex;
                }
            }
        }

        private bool DeflateOneSegment(WorkItem workitem)
        {
            var zlibCodec = workitem.compressor;
            var num = 0;
            zlibCodec.ResetDeflate();
            zlibCodec.NextIn = 0;
            zlibCodec.AvailableBytesIn = workitem.inputBytesAvailable;
            zlibCodec.NextOut = 0;
            zlibCodec.AvailableBytesOut = workitem.compressed.Length;
            do
            {
                zlibCodec.Deflate(FlushType.None);
            } while (zlibCodec.AvailableBytesIn > 0 || zlibCodec.AvailableBytesOut == 0);
            num = zlibCodec.Deflate(FlushType.Sync);
            workitem.compressedBytesAvailable = (int) zlibCodec.TotalBytesOut;
            return true;
        }

        [Conditional("Trace")]
        private void TraceOutput(TraceBits bits, string format, params object[] varParams)
        {
            if ((bits & _DesiredTrace) == TraceBits.None)
                return;
            lock (_outputLock)
            {
                var local_0 = Thread.CurrentThread.GetHashCode();
                Console.ForegroundColor = (ConsoleColor) (local_0%8 + 8);
                Console.Write("{0:000} PDOS ", local_0);
                Console.WriteLine(format, varParams);
                Console.ResetColor();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        [Flags]
        private enum TraceBits : uint
        {
            None = 0U,
            NotUsed1 = 1U,
            EmitLock = 2U,
            EmitEnter = 4U,
            EmitBegin = 8U,
            EmitDone = 16U,
            EmitSkip = 32U,
            EmitAll = EmitSkip | EmitDone | EmitBegin | EmitLock,
            Flush = 64U,
            Lifecycle = 128U,
            Session = 256U,
            Synch = 512U,
            Instance = 1024U,
            Compress = 2048U,
            Write = 4096U,
            WriteEnter = 8192U,
            WriteTake = 16384U,
            All = 4294967295U
        }
    }
} 
 
namespace Universe.TinyGZip.InternalImplementation
{
    using System.IO;
    using System.Text;

    internal class SharedUtils
    {
        public static int URShift(int number, int bits)
        {
            return (int) ((uint) number >> bits);
        }

        public static int ReadInput(TextReader sourceTextReader, byte[] target, int start, int count)
        {
            if (target.Length == 0)
                return 0;
            var buffer = new char[target.Length];
            var num = sourceTextReader.Read(buffer, start, count);
            if (num == 0)
                return -1;
            for (var index = start; index < start + num; ++index)
                target[index] = (byte) buffer[index];
            return num;
        }

        internal static byte[] ToByteArray(string sourceString)
        {
            return Encoding.UTF8.GetBytes(sourceString);
        }

        internal static char[] ToCharArray(byte[] byteArray)
        {
            return Encoding.UTF8.GetChars(byteArray);
        }
    }
} 
 
namespace Universe.TinyGZip.InternalImplementation
{
    internal sealed class StaticTree
    {
        internal static readonly short[] lengthAndLiteralsTreeCodes = new short[576]
        {
            12,
            8,
            140,
            8,
            76,
            8,
            204,
            8,
            44,
            8,
            172,
            8,
            108,
            8,
            236,
            8,
            28,
            8,
            156,
            8,
            92,
            8,
            220,
            8,
            60,
            8,
            188,
            8,
            124,
            8,
            252,
            8,
            2,
            8,
            130,
            8,
            66,
            8,
            194,
            8,
            34,
            8,
            162,
            8,
            98,
            8,
            226,
            8,
            18,
            8,
            146,
            8,
            82,
            8,
            210,
            8,
            50,
            8,
            178,
            8,
            114,
            8,
            242,
            8,
            10,
            8,
            138,
            8,
            74,
            8,
            202,
            8,
            42,
            8,
            170,
            8,
            106,
            8,
            234,
            8,
            26,
            8,
            154,
            8,
            90,
            8,
            218,
            8,
            58,
            8,
            186,
            8,
            122,
            8,
            250,
            8,
            6,
            8,
            134,
            8,
            70,
            8,
            198,
            8,
            38,
            8,
            166,
            8,
            102,
            8,
            230,
            8,
            22,
            8,
            150,
            8,
            86,
            8,
            214,
            8,
            54,
            8,
            182,
            8,
            118,
            8,
            246,
            8,
            14,
            8,
            142,
            8,
            78,
            8,
            206,
            8,
            46,
            8,
            174,
            8,
            110,
            8,
            238,
            8,
            30,
            8,
            158,
            8,
            94,
            8,
            222,
            8,
            62,
            8,
            190,
            8,
            126,
            8,
            254,
            8,
            1,
            8,
            129,
            8,
            65,
            8,
            193,
            8,
            33,
            8,
            161,
            8,
            97,
            8,
            225,
            8,
            17,
            8,
            145,
            8,
            81,
            8,
            209,
            8,
            49,
            8,
            177,
            8,
            113,
            8,
            241,
            8,
            9,
            8,
            137,
            8,
            73,
            8,
            201,
            8,
            41,
            8,
            169,
            8,
            105,
            8,
            233,
            8,
            25,
            8,
            153,
            8,
            89,
            8,
            217,
            8,
            57,
            8,
            185,
            8,
            121,
            8,
            249,
            8,
            5,
            8,
            133,
            8,
            69,
            8,
            197,
            8,
            37,
            8,
            165,
            8,
            101,
            8,
            229,
            8,
            21,
            8,
            149,
            8,
            85,
            8,
            213,
            8,
            53,
            8,
            181,
            8,
            117,
            8,
            245,
            8,
            13,
            8,
            141,
            8,
            77,
            8,
            205,
            8,
            45,
            8,
            173,
            8,
            109,
            8,
            237,
            8,
            29,
            8,
            157,
            8,
            93,
            8,
            221,
            8,
            61,
            8,
            189,
            8,
            125,
            8,
            253,
            8,
            19,
            9,
            275,
            9,
            147,
            9,
            403,
            9,
            83,
            9,
            339,
            9,
            211,
            9,
            467,
            9,
            51,
            9,
            307,
            9,
            179,
            9,
            435,
            9,
            115,
            9,
            371,
            9,
            243,
            9,
            499,
            9,
            11,
            9,
            267,
            9,
            139,
            9,
            395,
            9,
            75,
            9,
            331,
            9,
            203,
            9,
            459,
            9,
            43,
            9,
            299,
            9,
            171,
            9,
            427,
            9,
            107,
            9,
            363,
            9,
            235,
            9,
            491,
            9,
            27,
            9,
            283,
            9,
            155,
            9,
            411,
            9,
            91,
            9,
            347,
            9,
            219,
            9,
            475,
            9,
            59,
            9,
            315,
            9,
            187,
            9,
            443,
            9,
            123,
            9,
            379,
            9,
            251,
            9,
            507,
            9,
            7,
            9,
            263,
            9,
            135,
            9,
            391,
            9,
            71,
            9,
            327,
            9,
            199,
            9,
            455,
            9,
            39,
            9,
            295,
            9,
            167,
            9,
            423,
            9,
            103,
            9,
            359,
            9,
            231,
            9,
            487,
            9,
            23,
            9,
            279,
            9,
            151,
            9,
            407,
            9,
            87,
            9,
            343,
            9,
            215,
            9,
            471,
            9,
            55,
            9,
            311,
            9,
            183,
            9,
            439,
            9,
            119,
            9,
            375,
            9,
            247,
            9,
            503,
            9,
            15,
            9,
            271,
            9,
            143,
            9,
            399,
            9,
            79,
            9,
            335,
            9,
            207,
            9,
            463,
            9,
            47,
            9,
            303,
            9,
            175,
            9,
            431,
            9,
            111,
            9,
            367,
            9,
            239,
            9,
            495,
            9,
            31,
            9,
            287,
            9,
            159,
            9,
            415,
            9,
            95,
            9,
            351,
            9,
            223,
            9,
            479,
            9,
            63,
            9,
            319,
            9,
            191,
            9,
            447,
            9,
            sbyte.MaxValue,
            9,
            383,
            9,
            byte.MaxValue,
            9,
            511,
            9,
            0,
            7,
            64,
            7,
            32,
            7,
            96,
            7,
            16,
            7,
            80,
            7,
            48,
            7,
            112,
            7,
            8,
            7,
            72,
            7,
            40,
            7,
            104,
            7,
            24,
            7,
            88,
            7,
            56,
            7,
            120,
            7,
            4,
            7,
            68,
            7,
            36,
            7,
            100,
            7,
            20,
            7,
            84,
            7,
            52,
            7,
            116,
            7,
            3,
            8,
            131,
            8,
            67,
            8,
            195,
            8,
            35,
            8,
            163,
            8,
            99,
            8,
            227,
            8
        };

        internal static readonly short[] distTreeCodes = new short[60]
        {
            0,
            5,
            16,
            5,
            8,
            5,
            24,
            5,
            4,
            5,
            20,
            5,
            12,
            5,
            28,
            5,
            2,
            5,
            18,
            5,
            10,
            5,
            26,
            5,
            6,
            5,
            22,
            5,
            14,
            5,
            30,
            5,
            1,
            5,
            17,
            5,
            9,
            5,
            25,
            5,
            5,
            5,
            21,
            5,
            13,
            5,
            29,
            5,
            3,
            5,
            19,
            5,
            11,
            5,
            27,
            5,
            7,
            5,
            23,
            5
        };

        internal static readonly StaticTree Literals = new StaticTree(lengthAndLiteralsTreeCodes, Tree.ExtraLengthBits, InternalConstants.LITERALS + 1,
            InternalConstants.L_CODES, InternalConstants.MAX_BITS);

        internal static readonly StaticTree Distances = new StaticTree(distTreeCodes, Tree.ExtraDistanceBits, 0, InternalConstants.D_CODES,
            InternalConstants.MAX_BITS);

        internal static readonly StaticTree BitLengths = new StaticTree(null, Tree.extra_blbits, 0, InternalConstants.BL_CODES,
            InternalConstants.MAX_BL_BITS);

        internal int elems;
        internal int extraBase;
        internal int[] extraBits;
        internal int maxLength;
        internal short[] treeCodes;

        private StaticTree(short[] treeCodes, int[] extraBits, int extraBase, int elems, int maxLength)
        {
            this.treeCodes = treeCodes;
            this.extraBits = extraBits;
            this.extraBase = extraBase;
            this.elems = elems;
            this.maxLength = maxLength;
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    using System;

    internal sealed class Tree
    {
        internal const int Buf_size = 16;
        private static readonly int HEAP_SIZE = 2*InternalConstants.L_CODES + 1;

        internal static readonly int[] ExtraLengthBits = new int[29]
        {
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            1,
            1,
            1,
            1,
            2,
            2,
            2,
            2,
            3,
            3,
            3,
            3,
            4,
            4,
            4,
            4,
            5,
            5,
            5,
            5,
            0
        };

        internal static readonly int[] ExtraDistanceBits = new int[30]
        {
            0,
            0,
            0,
            0,
            1,
            1,
            2,
            2,
            3,
            3,
            4,
            4,
            5,
            5,
            6,
            6,
            7,
            7,
            8,
            8,
            9,
            9,
            10,
            10,
            11,
            11,
            12,
            12,
            13,
            13
        };

        internal static readonly int[] extra_blbits = new int[19]
        {
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            2,
            3,
            7
        };

        internal static readonly sbyte[] bl_order = new sbyte[19]
        {
            16,
            17,
            18,
            0,
            8,
            7,
            9,
            6,
            10,
            5,
            11,
            4,
            12,
            3,
            13,
            2,
            14,
            1,
            15
        };

        private static readonly sbyte[] _dist_code = new sbyte[512]
        {
            0,
            1,
            2,
            3,
            4,
            4,
            5,
            5,
            6,
            6,
            6,
            6,
            7,
            7,
            7,
            7,
            8,
            8,
            8,
            8,
            8,
            8,
            8,
            8,
            9,
            9,
            9,
            9,
            9,
            9,
            9,
            9,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            10,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            11,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            12,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            13,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            14,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            15,
            0,
            0,
            16,
            17,
            18,
            18,
            19,
            19,
            20,
            20,
            20,
            20,
            21,
            21,
            21,
            21,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            28,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29,
            29
        };

        internal static readonly sbyte[] LengthCode = new sbyte[256]
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            8,
            9,
            9,
            10,
            10,
            11,
            11,
            12,
            12,
            12,
            12,
            13,
            13,
            13,
            13,
            14,
            14,
            14,
            14,
            15,
            15,
            15,
            15,
            16,
            16,
            16,
            16,
            16,
            16,
            16,
            16,
            17,
            17,
            17,
            17,
            17,
            17,
            17,
            17,
            18,
            18,
            18,
            18,
            18,
            18,
            18,
            18,
            19,
            19,
            19,
            19,
            19,
            19,
            19,
            19,
            20,
            20,
            20,
            20,
            20,
            20,
            20,
            20,
            20,
            20,
            20,
            20,
            20,
            20,
            20,
            20,
            21,
            21,
            21,
            21,
            21,
            21,
            21,
            21,
            21,
            21,
            21,
            21,
            21,
            21,
            21,
            21,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            22,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            23,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            24,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            25,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            26,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            27,
            28
        };

        internal static readonly int[] LengthBase = new int[29]
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            10,
            12,
            14,
            16,
            20,
            24,
            28,
            32,
            40,
            48,
            56,
            64,
            80,
            96,
            112,
            128,
            160,
            192,
            224,
            0
        };

        internal static readonly int[] DistanceBase = new int[30]
        {
            0,
            1,
            2,
            3,
            4,
            6,
            8,
            12,
            16,
            24,
            32,
            48,
            64,
            96,
            128,
            192,
            256,
            384,
            512,
            768,
            1024,
            1536,
            2048,
            3072,
            4096,
            6144,
            8192,
            12288,
            16384,
            24576
        };

        internal short[] dyn_tree;
        internal int max_code;
        internal StaticTree staticTree;

        internal static int DistanceCode(int dist)
        {
            return dist < 256 ? _dist_code[dist] : _dist_code[256 + SharedUtils.URShift(dist, 7)];
        }

        internal void gen_bitlen(DeflateManager s)
        {
            var numArray1 = dyn_tree;
            var numArray2 = staticTree.treeCodes;
            var numArray3 = staticTree.extraBits;
            var num1 = staticTree.extraBase;
            var index1 = staticTree.maxLength;
            var num2 = 0;
            for (var index2 = 0; index2 <= InternalConstants.MAX_BITS; ++index2)
                s.bl_count[index2] = 0;
            numArray1[s.heap[s.heap_max]*2 + 1] = 0;
            int index3;
            for (index3 = s.heap_max + 1; index3 < HEAP_SIZE; ++index3)
            {
                var num3 = s.heap[index3];
                var index2 = numArray1[numArray1[num3*2 + 1]*2 + 1] + 1;
                if (index2 > index1)
                {
                    index2 = index1;
                    ++num2;
                }
                numArray1[num3*2 + 1] = (short) index2;
                if (num3 <= max_code)
                {
                    ++s.bl_count[index2];
                    var num4 = 0;
                    if (num3 >= num1)
                        num4 = numArray3[num3 - num1];
                    var num5 = numArray1[num3*2];
                    s.opt_len += num5*(index2 + num4);
                    if (numArray2 != null)
                        s.static_len += num5*(numArray2[num3*2 + 1] + num4);
                }
            }
            if (num2 == 0)
                return;
            do
            {
                var index2 = index1 - 1;
                while (s.bl_count[index2] == 0)
                    --index2;
                --s.bl_count[index2];
                s.bl_count[index2 + 1] = (short) (s.bl_count[index2 + 1] + 2);
                --s.bl_count[index1];
                num2 -= 2;
            } while (num2 > 0);
            for (var index2 = index1; index2 != 0; --index2)
            {
                int num3 = s.bl_count[index2];
                while (num3 != 0)
                {
                    var num4 = s.heap[--index3];
                    if (num4 <= max_code)
                    {
                        if (numArray1[num4*2 + 1] != index2)
                        {
                            s.opt_len = (int) (s.opt_len + (index2 - (long) numArray1[num4*2 + 1])*numArray1[num4*2]);
                            numArray1[num4*2 + 1] = (short) index2;
                        }
                        --num3;
                    }
                }
            }
        }

        internal void build_tree(DeflateManager s)
        {
            var tree = dyn_tree;
            var numArray1 = staticTree.treeCodes;
            var num1 = staticTree.elems;
            var max_code = -1;
            s.heap_len = 0;
            s.heap_max = HEAP_SIZE;
            for (var index = 0; index < num1; ++index)
            {
                if (tree[index*2] != 0)
                {
                    s.heap[++s.heap_len] = max_code = index;
                    s.depth[index] = 0;
                }
                else
                    tree[index*2 + 1] = 0;
            }
            while (s.heap_len < 2)
            {
                var numArray2 = s.heap;
                var index1 = ++s.heap_len;
                int num2;
                if (max_code >= 2)
                    num2 = 0;
                else
                    max_code = num2 = max_code + 1;
                var num3 = num2;
                numArray2[index1] = num2;
                var index2 = num3;
                tree[index2*2] = 1;
                s.depth[index2] = 0;
                --s.opt_len;
                if (numArray1 != null)
                    s.static_len -= numArray1[index2*2 + 1];
            }
            this.max_code = max_code;
            for (var k = s.heap_len/2; k >= 1; --k)
                s.pqdownheap(tree, k);
            var index3 = num1;
            do
            {
                var index1 = s.heap[1];
                s.heap[1] = s.heap[s.heap_len--];
                s.pqdownheap(tree, 1);
                var index2 = s.heap[1];
                s.heap[--s.heap_max] = index1;
                s.heap[--s.heap_max] = index2;
                tree[index3*2] = (short) (tree[index1*2] + tree[index2*2]);
                s.depth[index3] = (sbyte) (Math.Max((byte) s.depth[index1], (byte) s.depth[index2]) + 1);
                tree[index1*2 + 1] = tree[index2*2 + 1] = (short) index3;
                s.heap[1] = index3++;
                s.pqdownheap(tree, 1);
            } while (s.heap_len >= 2);
            s.heap[--s.heap_max] = s.heap[1];
            gen_bitlen(s);
            gen_codes(tree, max_code, s.bl_count);
        }

        internal static void gen_codes(short[] tree, int max_code, short[] bl_count)
        {
            var numArray = new short[InternalConstants.MAX_BITS + 1];
            short num = 0;
            for (var index = 1; index <= InternalConstants.MAX_BITS; ++index)
                numArray[index] = num = (short) (num + bl_count[index - 1] << 1);
            for (var index = 0; index <= max_code; ++index)
            {
                int len = tree[index*2 + 1];
                if (len != 0)
                    tree[index*2] = (short) bi_reverse(numArray[len]++, len);
            }
        }

        internal static int bi_reverse(int code, int len)
        {
            var num1 = 0;
            do
            {
                var num2 = num1 | code & 1;
                code >>= 1;
                num1 = num2 << 1;
            } while (--len > 0);
            return num1 >> 1;
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    internal class WorkItem
    {
        public byte[] buffer;
        public byte[] compressed;
        public int compressedBytesAvailable;
        public ZlibCodec compressor;
        public int crc;
        public int index;
        public int inputBytesAvailable;
        public int ordinal;

        public WorkItem(int size, CompressionLevel compressLevel, CompressionStrategy strategy, int ix)
        {
            buffer = new byte[size];
            compressed = new byte[size + (size/32768 + 1)*5*2];
            compressor = new ZlibCodec();
            compressor.InitializeDeflate(compressLevel, false);
            compressor.OutputBuffer = compressed;
            compressor.InputBuffer = buffer;
            index = ix;
        }
    }
} 
 

namespace Universe.TinyGZip
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("ebc25cf6-9120-4283-b972-0e5520d0000E")]
    public class ZlibException : Exception
    {
        public ZlibException()
        {
        }

        public ZlibException(string s)
            : base(s)
        {
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    #pragma warning disable 642, 219
    internal class ZlibBaseStream : Stream
    {
        protected internal byte[] _buf1 = new byte[1];
        protected internal int _bufferSize = 16384;
        protected internal CompressionMode _compressionMode;
        protected internal ZlibStreamFlavor _flavor;
        protected internal FlushType _flushMode;
        protected internal string _GzipComment;
        protected internal string _GzipFileName;
        protected internal int _gzipHeaderByteCount;
        protected internal DateTime _GzipMtime;
        protected internal bool _leaveOpen;
        protected internal CompressionLevel _level;
        protected internal Stream _stream;
        protected internal StreamMode _streamMode = StreamMode.Undefined;
        protected internal byte[] _workingBuffer;
        protected internal ZlibCodec _z;
        private readonly CRC32 crc;
        private bool nomoreinput;
        protected internal CompressionStrategy Strategy = CompressionStrategy.Default;

        public ZlibBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level, ZlibStreamFlavor flavor, bool leaveOpen)
        {
            _flushMode = FlushType.None;
            _stream = stream;
            _leaveOpen = leaveOpen;
            _compressionMode = compressionMode;
            _flavor = flavor;
            _level = level;
            if (flavor != ZlibStreamFlavor.GZIP)
                return;
            crc = new CRC32();
        }

        internal int Crc32
        {
            get
            {
                if (crc == null)
                    return 0;
                return crc.Crc32Result;
            }
        }

        protected internal bool _wantCompress
        {
            get { return _compressionMode == CompressionMode.Compress; }
        }

        private ZlibCodec z
        {
            get
            {
                if (_z == null)
                {
                    var flag = _flavor == ZlibStreamFlavor.ZLIB;
                    _z = new ZlibCodec();
                    if (_compressionMode == CompressionMode.Decompress)
                    {
                        _z.InitializeInflate(flag);
                    }
                    else
                    {
                        _z.Strategy = Strategy;
                        _z.InitializeDeflate(_level, flag);
                    }
                }
                return _z;
            }
        }

        private byte[] workingBuffer
        {
            get
            {
                if (_workingBuffer == null)
                    _workingBuffer = new byte[_bufferSize];
                return _workingBuffer;
            }
        }

        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        public override long Length
        {
            get { return _stream.Length; }
        }

        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (crc != null)
                crc.SlurpBlock(buffer, offset, count);
            if (_streamMode == StreamMode.Undefined)
                _streamMode = StreamMode.Writer;
            else if (_streamMode != StreamMode.Writer)
                throw new ZlibException("Cannot Write after Reading.");
            if (count == 0)
                return;
            z.InputBuffer = buffer;
            _z.NextIn = offset;
            _z.AvailableBytesIn = count;
            bool flag;
            do
            {
                _z.OutputBuffer = workingBuffer;
                _z.NextOut = 0;
                _z.AvailableBytesOut = _workingBuffer.Length;
                var num = _wantCompress ? _z.Deflate(_flushMode) : _z.Inflate(_flushMode);
                if (num != 0 && num != 1)
                    throw new ZlibException((_wantCompress ? "de" : "in") + "flating: " + _z.Message);
                _stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);
                flag = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;
                if (_flavor == ZlibStreamFlavor.GZIP && !_wantCompress)
                    flag = _z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0;
            } while (!flag);
        }

        private void finish()
        {
            if (_z == null)
                return;
            if (_streamMode == StreamMode.Writer)
            {
                bool flag;
                do
                {
                    _z.OutputBuffer = workingBuffer;
                    _z.NextOut = 0;
                    _z.AvailableBytesOut = _workingBuffer.Length;
                    var num = _wantCompress ? _z.Deflate(FlushType.Finish) : _z.Inflate(FlushType.Finish);
                    if (num != 1 && num != 0)
                    {
                        var str = (_wantCompress ? "de" : "in") + "flating";
                        if (_z.Message == null)
                            throw new ZlibException(string.Format("{0}: (rc = {1})", str, num));
                        throw new ZlibException(str + ": " + _z.Message);
                    }
                    if (_workingBuffer.Length - _z.AvailableBytesOut > 0)
                        _stream.Write(_workingBuffer, 0, _workingBuffer.Length - _z.AvailableBytesOut);
                    flag = _z.AvailableBytesIn == 0 && _z.AvailableBytesOut != 0;
                    if (_flavor == ZlibStreamFlavor.GZIP && !_wantCompress)
                        flag = _z.AvailableBytesIn == 8 && _z.AvailableBytesOut != 0;
                } while (!flag);
                Flush();
                if (_flavor != ZlibStreamFlavor.GZIP)
                    return;
                if (!_wantCompress)
                    throw new ZlibException("Writing with decompression is not supported.");
                _stream.Write(BitConverter.GetBytes(crc.Crc32Result), 0, 4);
                _stream.Write(BitConverter.GetBytes((int) (crc.TotalBytesRead & uint.MaxValue)), 0, 4);
            }
            else
            {
                if (_streamMode != StreamMode.Reader || _flavor != ZlibStreamFlavor.GZIP)
                    return;
                if (_wantCompress)
                    throw new ZlibException("Reading with compression is not supported.");
                if (_z.TotalBytesOut == 0L)
                    return;
                var buffer = new byte[8];
                if (_z.AvailableBytesIn < 8)
                {
                    Array.Copy(_z.InputBuffer, _z.NextIn, buffer, 0, _z.AvailableBytesIn);
                    var count = 8 - _z.AvailableBytesIn;
                    var num = _stream.Read(buffer, _z.AvailableBytesIn, count);
                    if (count != num)
                        throw new ZlibException(string.Format("Missing or incomplete GZIP trailer. Expected 8 bytes, got {0}.",
                            _z.AvailableBytesIn + num));
                }
                else
                    Array.Copy(_z.InputBuffer, _z.NextIn, buffer, 0, buffer.Length);
                var num1 = BitConverter.ToInt32(buffer, 0);
                var crc32Result = crc.Crc32Result;
                var num2 = BitConverter.ToInt32(buffer, 4);
                var num3 = (int) (_z.TotalBytesOut & uint.MaxValue);
                if (crc32Result != num1)
                    throw new ZlibException(string.Format("Bad CRC32 in GZIP trailer. (actual({0:X8})!=expected({1:X8}))", crc32Result, num1));
                if (num3 != num2)
                    throw new ZlibException(string.Format("Bad size in GZIP trailer. (actual({0})!=expected({1}))", num3, num2));
            }
        }

        private void end()
        {
            if (z == null)
                return;
            if (_wantCompress)
                _z.EndDeflate();
            else
                _z.EndInflate();
            _z = null;
        }

        public override void Close()
        {
            if (_stream == null)
                return;
            try
            {
                finish();
            }
            finally
            {
                end();
                if (!_leaveOpen)
                    _stream.Close();
                _stream = null;
            }
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        private string ReadZeroTerminatedString()
        {
            var list = new List<byte>();
            var flag = false;
            while (_stream.Read(_buf1, 0, 1) == 1)
            {
                if (_buf1[0] == 0)
                    flag = true;
                else
                    list.Add(_buf1[0]);
                if (flag)
                {
                    var bytes = list.ToArray();
                    return GZipStream.iso8859dash1.GetString(bytes, 0, bytes.Length);
                }
            }
            throw new ZlibException("Unexpected EOF reading GZIP header.");
        }

        private int _ReadAndValidateGzipHeader()
        {
            var num1 = 0;
            var buffer1 = new byte[10];
            var num2 = _stream.Read(buffer1, 0, buffer1.Length);
            switch (num2)
            {
                case 0:
                    return 0;
                case 10:
                    if (buffer1[0] != 31 || buffer1[1] != 139 || buffer1[2] != 8)
                        throw new ZlibException("Bad GZIP header.");
                    var num3 = BitConverter.ToInt32(buffer1, 4);
                    _GzipMtime = GZipStream._unixEpoch.AddSeconds(num3);
                    var num4 = num1 + num2;
                    if ((buffer1[3] & 4) == 4)
                    {
                        var num5 = _stream.Read(buffer1, 0, 2);
                        var num6 = num4 + num5;
                        var num7 = (short) (buffer1[0] + buffer1[1]*256);
                        var buffer2 = new byte[num7];
                        var num8 = _stream.Read(buffer2, 0, buffer2.Length);
                        if (num8 != num7)
                            throw new ZlibException("Unexpected end-of-file reading GZIP header.");
                        num4 = num6 + num8;
                    }
                    if ((buffer1[3] & 8) == 8)
                        _GzipFileName = ReadZeroTerminatedString();
                    if ((buffer1[3] & 16) == 16)
                        _GzipComment = ReadZeroTerminatedString();
                    if ((buffer1[3] & 2) == 2)
                        Read(_buf1, 0, 1);
                    return num4;
                default:
                    throw new ZlibException("Not a valid GZIP stream.");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_streamMode == StreamMode.Undefined)
            {
                if (!_stream.CanRead)
                    throw new ZlibException("The stream is not readable.");
                _streamMode = StreamMode.Reader;
                z.AvailableBytesIn = 0;
                if (_flavor == ZlibStreamFlavor.GZIP)
                {
                    _gzipHeaderByteCount = _ReadAndValidateGzipHeader();
                    if (_gzipHeaderByteCount == 0)
                        return 0;
                }
            }
            if (_streamMode != StreamMode.Reader)
                throw new ZlibException("Cannot Read after Writing.");
            if (count == 0 || nomoreinput && _wantCompress)
                return 0;
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (offset < buffer.GetLowerBound(0))
                throw new ArgumentOutOfRangeException("offset");
            if (offset + count > buffer.GetLength(0))
                throw new ArgumentOutOfRangeException("count");
            _z.OutputBuffer = buffer;
            _z.NextOut = offset;
            _z.AvailableBytesOut = count;
            _z.InputBuffer = workingBuffer;
            int num1;
            do
            {
                if (_z.AvailableBytesIn == 0 && !nomoreinput)
                {
                    _z.NextIn = 0;
                    _z.AvailableBytesIn = _stream.Read(_workingBuffer, 0, _workingBuffer.Length);
                    if (_z.AvailableBytesIn == 0)
                        nomoreinput = true;
                }
                num1 = _wantCompress ? _z.Deflate(_flushMode) : _z.Inflate(_flushMode);
                if (nomoreinput && num1 == -5)
                    return 0;
                if (num1 != 0 && num1 != 1)
                    throw new ZlibException(string.Format("{0}flating:  rc={1}  msg={2}", _wantCompress ? "de" : "in", num1, _z.Message));
            } while ((!nomoreinput && num1 != 1 || _z.AvailableBytesOut != count) && (_z.AvailableBytesOut > 0 && !nomoreinput && num1 == 0));
            if (_z.AvailableBytesOut > 0)
            {
                if (num1 != 0 || _z.AvailableBytesIn != 0)
                    ;
                if (nomoreinput && _wantCompress)
                {
                    var num2 = _z.Deflate(FlushType.Finish);
                    if (num2 != 0 && num2 != 1)
                        throw new ZlibException(string.Format("Deflating:  rc={0}  msg={1}", num2, _z.Message));
                }
            }
            var count1 = count - _z.AvailableBytesOut;
            if (crc != null)
                crc.SlurpBlock(buffer, offset, count1);
            return count1;
        }

        public static void CompressString(string s, Stream compressor)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            using (compressor)
                compressor.Write(bytes, 0, bytes.Length);
        }

        public static void CompressBuffer(byte[] b, Stream compressor)
        {
            using (compressor)
                compressor.Write(b, 0, b.Length);
        }

        public static string UncompressString(byte[] compressed, Stream decompressor)
        {
            var buffer = new byte[1024];
            var utF8 = Encoding.UTF8;
            using (var memoryStream = new MemoryStream())
            {
                using (decompressor)
                {
                    int count;
                    while ((count = decompressor.Read(buffer, 0, buffer.Length)) != 0)
                        memoryStream.Write(buffer, 0, count);
                }
                memoryStream.Seek(0L, SeekOrigin.Begin);
                return new StreamReader(memoryStream, utF8).ReadToEnd();
            }
        }

        public static byte[] UncompressBuffer(byte[] compressed, Stream decompressor)
        {
            var buffer = new byte[1024];
            using (var memoryStream = new MemoryStream())
            {
                using (decompressor)
                {
                    int count;
                    while ((count = decompressor.Read(buffer, 0, buffer.Length)) != 0)
                        memoryStream.Write(buffer, 0, count);
                }
                return memoryStream.ToArray();
            }
        }

        internal enum StreamMode
        {
            Writer,
            Reader,
            Undefined
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    using System;
    using System.Runtime.InteropServices;

    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    [ComVisible(true)]
    [Guid("ebc25cf6-9120-4283-b972-0e5520d0000D")]
    public sealed class ZlibCodec
    {
        internal uint _Adler32;
        public int AvailableBytesIn;
        public int AvailableBytesOut;
        public CompressionLevel CompressLevel = CompressionLevel.Default;
        internal DeflateManager dstate;
        public byte[] InputBuffer;
        internal InflateManager istate;
        public string Message;
        public int NextIn;
        public int NextOut;
        public byte[] OutputBuffer;
        public CompressionStrategy Strategy = CompressionStrategy.Default;
        public long TotalBytesIn;
        public long TotalBytesOut;
        public int WindowBits = 15;

        public ZlibCodec()
        {
        }

        public ZlibCodec(CompressionMode mode)
        {
            if (mode == CompressionMode.Compress)
            {
                if (InitializeDeflate() != 0)
                    throw new ZlibException("Cannot initialize for deflate.");
            }
            else
            {
                if (mode != CompressionMode.Decompress)
                    throw new ZlibException("Invalid ZlibStreamFlavor.");
                if (InitializeInflate() != 0)
                    throw new ZlibException("Cannot initialize for inflate.");
            }
        }

        public int Adler32
        {
            get { return (int) _Adler32; }
        }

        public int InitializeInflate()
        {
            return InitializeInflate(WindowBits);
        }

        public int InitializeInflate(bool expectRfc1950Header)
        {
            return InitializeInflate(WindowBits, expectRfc1950Header);
        }

        public int InitializeInflate(int windowBits)
        {
            WindowBits = windowBits;
            return InitializeInflate(windowBits, true);
        }

        public int InitializeInflate(int windowBits, bool expectRfc1950Header)
        {
            WindowBits = windowBits;
            if (dstate != null)
                throw new ZlibException("You may not call InitializeInflate() after calling InitializeDeflate().");
            istate = new InflateManager(expectRfc1950Header);
            return istate.Initialize(this, windowBits);
        }

        public int Inflate(FlushType flush)
        {
            if (istate == null)
                throw new ZlibException("No Inflate State!");
            return istate.Inflate(flush);
        }

        public int EndInflate()
        {
            if (istate == null)
                throw new ZlibException("No Inflate State!");
            var num = istate.End();
            istate = null;
            return num;
        }

        public int SyncInflate()
        {
            if (istate == null)
                throw new ZlibException("No Inflate State!");
            return istate.Sync();
        }

        public int InitializeDeflate()
        {
            return _InternalInitializeDeflate(true);
        }

        public int InitializeDeflate(CompressionLevel level)
        {
            CompressLevel = level;
            return _InternalInitializeDeflate(true);
        }

        public int InitializeDeflate(CompressionLevel level, bool wantRfc1950Header)
        {
            CompressLevel = level;
            return _InternalInitializeDeflate(wantRfc1950Header);
        }

        public int InitializeDeflate(CompressionLevel level, int bits)
        {
            CompressLevel = level;
            WindowBits = bits;
            return _InternalInitializeDeflate(true);
        }

        public int InitializeDeflate(CompressionLevel level, int bits, bool wantRfc1950Header)
        {
            CompressLevel = level;
            WindowBits = bits;
            return _InternalInitializeDeflate(wantRfc1950Header);
        }

        private int _InternalInitializeDeflate(bool wantRfc1950Header)
        {
            if (istate != null)
                throw new ZlibException("You may not call InitializeDeflate() after calling InitializeInflate().");
            dstate = new DeflateManager();
            dstate.WantRfc1950HeaderBytes = wantRfc1950Header;
            return dstate.Initialize(this, CompressLevel, WindowBits, Strategy);
        }

        public int Deflate(FlushType flush)
        {
            if (dstate == null)
                throw new ZlibException("No Deflate State!");
            return dstate.Deflate(flush);
        }

        public int EndDeflate()
        {
            if (dstate == null)
                throw new ZlibException("No Deflate State!");
            dstate = null;
            return 0;
        }

        public void ResetDeflate()
        {
            if (dstate == null)
                throw new ZlibException("No Deflate State!");
            dstate.Reset();
        }

        public int SetDeflateParams(CompressionLevel level, CompressionStrategy strategy)
        {
            if (dstate == null)
                throw new ZlibException("No Deflate State!");
            return dstate.SetParams(level, strategy);
        }

        public int SetDictionary(byte[] dictionary)
        {
            if (istate != null)
                return istate.SetDictionary(dictionary);
            if (dstate != null)
                return dstate.SetDictionary(dictionary);
            throw new ZlibException("No Inflate or Deflate state!");
        }

        internal void flush_pending()
        {
            var length = dstate.pendingCount;
            if (length > AvailableBytesOut)
                length = AvailableBytesOut;
            if (length == 0)
                return;
            if (dstate.pending.Length <= dstate.nextPending || OutputBuffer.Length <= NextOut || dstate.pending.Length < dstate.nextPending + length ||
                OutputBuffer.Length < NextOut + length)
                throw new ZlibException(string.Format("Invalid State. (pending.Length={0}, pendingCount={1})", dstate.pending.Length,
                    dstate.pendingCount));
            Array.Copy(dstate.pending, dstate.nextPending, OutputBuffer, NextOut, length);
            NextOut += length;
            dstate.nextPending += length;
            TotalBytesOut += length;
            AvailableBytesOut -= length;
            dstate.pendingCount -= length;
            if (dstate.pendingCount != 0)
                return;
            dstate.nextPending = 0;
        }

        internal int read_buf(byte[] buf, int start, int size)
        {
            var num = AvailableBytesIn;
            if (num > size)
                num = size;
            if (num == 0)
                return 0;
            AvailableBytesIn -= num;
            if (dstate.WantRfc1950HeaderBytes)
                _Adler32 = Adler.Adler32(_Adler32, InputBuffer, NextIn, num);
            Array.Copy(InputBuffer, NextIn, buf, start, num);
            NextIn += num;
            TotalBytesIn += num;
            return num;
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    public static class ZlibConstants
    {
        public const int WindowBitsMax = 15;
        public const int WindowBitsDefault = 15;
        public const int Z_OK = 0;
        public const int Z_STREAM_END = 1;
        public const int Z_NEED_DICT = 2;
        public const int Z_STREAM_ERROR = -2;
        public const int Z_DATA_ERROR = -3;
        public const int Z_BUF_ERROR = -5;
        public const int WorkingBufferSizeDefault = 16384;
        public const int WorkingBufferSizeMin = 1024;
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    using System;
    using System.IO;

    public class ZlibStream : Stream
    {
        internal ZlibBaseStream _baseStream;
        private bool _disposed;

        public ZlibStream(Stream stream, CompressionMode mode)
            : this(stream, mode, CompressionLevel.Default, false)
        {
        }

        public ZlibStream(Stream stream, CompressionMode mode, CompressionLevel level)
            : this(stream, mode, level, false)
        {
        }

        public ZlibStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : this(stream, mode, CompressionLevel.Default, leaveOpen)
        {
        }

        public ZlibStream(Stream stream, CompressionMode mode, CompressionLevel level, bool leaveOpen)
        {
            _baseStream = new ZlibBaseStream(stream, mode, level, ZlibStreamFlavor.ZLIB, leaveOpen);
        }

        public virtual FlushType FlushMode
        {
            get { return _baseStream._flushMode; }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("ZlibStream");
                _baseStream._flushMode = value;
            }
        }

        public int BufferSize
        {
            get { return _baseStream._bufferSize; }
            set
            {
                if (_disposed)
                    throw new ObjectDisposedException("ZlibStream");
                if (_baseStream._workingBuffer != null)
                    throw new ZlibException("The working buffer is already set.");
                if (value < 1024)
                    throw new ZlibException(string.Format("Don't be silly. {0} bytes?? Use a bigger buffer, at least {1}.", value, 1024));
                _baseStream._bufferSize = value;
            }
        }

        public virtual long TotalIn
        {
            get { return _baseStream._z.TotalBytesIn; }
        }

        public virtual long TotalOut
        {
            get { return _baseStream._z.TotalBytesOut; }
        }

        public override bool CanRead
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("ZlibStream");
                return _baseStream._stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("ZlibStream");
                return _baseStream._stream.CanWrite;
            }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Writer)
                    return _baseStream._z.TotalBytesOut;
                if (_baseStream._streamMode == ZlibBaseStream.StreamMode.Reader)
                    return _baseStream._z.TotalBytesIn;
                return 0L;
            }
            set { throw new NotSupportedException(); }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (_disposed)
                    return;
                if (disposing && _baseStream != null)
                    _baseStream.Close();
                _disposed = true;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            if (_disposed)
                throw new ObjectDisposedException("ZlibStream");
            _baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException("ZlibStream");
            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException("ZlibStream");
            _baseStream.Write(buffer, offset, count);
        }

        public static byte[] CompressString(string s)
        {
            using (var memoryStream = new MemoryStream())
            {
                Stream compressor = new ZlibStream(memoryStream, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZlibBaseStream.CompressString(s, compressor);
                return memoryStream.ToArray();
            }
        }

        public static byte[] CompressBuffer(byte[] b)
        {
            using (var memoryStream = new MemoryStream())
            {
                Stream compressor = new ZlibStream(memoryStream, CompressionMode.Compress, CompressionLevel.BestCompression);
                ZlibBaseStream.CompressBuffer(b, compressor);
                return memoryStream.ToArray();
            }
        }

        public static string UncompressString(byte[] compressed)
        {
            using (var memoryStream = new MemoryStream(compressed))
            {
                Stream decompressor = new ZlibStream(memoryStream, CompressionMode.Decompress);
                return ZlibBaseStream.UncompressString(compressed, decompressor);
            }
        }

        public static byte[] UncompressBuffer(byte[] compressed)
        {
            using (var memoryStream = new MemoryStream(compressed))
            {
                Stream decompressor = new ZlibStream(memoryStream, CompressionMode.Decompress);
                return ZlibBaseStream.UncompressBuffer(compressed, decompressor);
            }
        }
    }
} 
 

namespace Universe.TinyGZip.InternalImplementation
{
    internal enum ZlibStreamFlavor
    {
        ZLIB = 1950,
        DEFLATE = 1951,
        GZIP = 1952
    }
} 
 
