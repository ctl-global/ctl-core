using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Ctl
{
    /// <summary>
    /// Implements Google's FarmHash.
    /// </summary>
    /// <remarks>
    /// Based on FarmHash implementation from https://github.com/brandondahler/Data.HashFunction
    /// </remarks>
    static class FarmHash
    {
        private const ulong k0 = 0xc3a5c85c97cb3127UL;
        private const ulong k1 = 0xb492b66fbe98f273UL;
        private const ulong k2 = 0x9ae16a3b2f90404fUL;

        public static unsafe ulong Hash64(ReadOnlySpan<char> data)
        {
            if (data.IsEmpty)
            {
                return k2;
            }

            // this stuff is here because GetByteCount/GetBytes to operate on spans does not exist in .NET Standard.

            fixed (char* pstr = data)
            {
                int encodedLen = Encoding.UTF8.GetByteCount(pstr, data.Length);
                Span<byte> utf8 = encodedLen <= 64 ? stackalloc byte[encodedLen] : new byte[encodedLen];

                fixed (byte* putf8 = utf8)
                {
                    Encoding.UTF8.GetBytes(pstr, data.Length, putf8, encodedLen);
                }

                return Hash64(utf8);
            }
        }

        public static ulong Hash64(ReadOnlySpan<byte> dataArray)
        {
            if (dataArray.IsEmpty)
            {
                return k2;
            }

            var dataLength = dataArray.Length;

            if (dataLength <= 32)
            {
                if (dataLength <= 16)
                    return Hash64_16b(dataArray);

                return Hash64_32b(dataArray);
            }

            if (dataLength <= 64)
                return Hash64_64b(dataArray);

            var seed = 81UL;

            var x = seed;
            var y = ((seed * k1) + 113);
            var z = ShiftMix(y * k2 + 113) * k2;

            ulong vLow = 0, vHigh = 0;
            ulong wLow = 0, wHigh = 0;

            x = (x * k2) + BinaryPrimitives.ReadUInt64LittleEndian(dataArray);

            for (var i = 0; i < dataLength - 64; i += 64)
            {
                x = RotateRight(x + y + vLow + BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(i + 8)), 37) * k1;
                y = RotateRight(y + vHigh + BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(i + 48)), 42) * k1;
                x ^= wHigh;
                y += vLow + BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(i + 40));
                z = RotateRight(z + wLow, 33) * k1;
                (vLow, vHigh) = WeakHashLen32WithSeeds(dataArray.Slice(i), vHigh * k1, x + wLow);
                (wLow, wHigh) = WeakHashLen32WithSeeds(dataArray.Slice(i + 32), z + wHigh, y + BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(i + 16)));

                (z, x) = (x, z);
            }


            var mul = k1 + ((z & 0xff) << 1);

            wLow += (ulong)((dataLength - 1) & 63);
            vLow += wLow;
            wLow += vLow;

            x = RotateRight(x + y + vLow + BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 64 + 8)), 37) * mul;
            y = RotateRight(y + vHigh + BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 64 + 48)), 42) * mul;

            x ^= wHigh * 9;
            y += vLow * 9 + BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 64 + 40));
            z = RotateRight(z + wLow, 33) * mul;

            (vLow, vHigh) = WeakHashLen32WithSeeds(dataArray.Slice(dataLength - 64), vHigh * mul, x + wLow);
            (wLow, wHigh) = WeakHashLen32WithSeeds(dataArray.Slice(dataLength - 64 + 32), z + wHigh, y + BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 64 + 16)));

            (z, x) = (x, z);

            return ComputeHash16(
                ComputeHash16(vLow, wLow, mul) + ShiftMix(y) * k0 + z,
                ComputeHash16(vHigh, wHigh, mul) + x,
                mul);
        }

        static (ulong low, ulong high) WeakHashLen32WithSeeds(ReadOnlySpan<byte> dataArray, ulong a, ulong b)
        {
            ulong w = BinaryPrimitives.ReadUInt64LittleEndian(dataArray);
            ulong x = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(8));
            ulong y = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(16));
            ulong z = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(24));

            a += w;
            b = RotateRight(b + a + z, 21);

            var c = a;

            a += x;
            a += y;
            b += RotateRight(a, 44);

            return (a + z, b + c);
        }

        static ulong Hash64_64b(ReadOnlySpan<byte> dataArray)
        {
            var dataLength = dataArray.Length;
            var mul = k2 + (ulong)(dataLength * 2);
            var a = BinaryPrimitives.ReadUInt64LittleEndian(dataArray) * k2;
            var b = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(8));
            var c = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 8)) * mul;
            var d = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 16)) * k2;

            var y = RotateRight(a + b, 43) + RotateRight(c, 30) + d;
            var z = ComputeHash16(y, a + RotateRight(b + k2, 18) + c, mul);

            var e = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(16)) * mul;
            var f = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(24));
            var g = (y + BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 32))) * mul;
            var h = (z + BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 24))) * mul;

            return ComputeHash16(
                RotateRight(e + f, 43) + RotateRight(g, 30) + h,
                e + RotateRight(f + a, 18) + g,
                mul);
        }

        static ulong Hash64_32b(ReadOnlySpan<byte> dataArray)
        {
            var dataLength = dataArray.Length;

            var mul = k2 + ((ulong)dataLength * 2);
            var a = BinaryPrimitives.ReadUInt64LittleEndian(dataArray) * k1;
            var b = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(8));
            var c = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 8)) * mul;
            var d = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 16)) * k2;

            return ComputeHash16(
                RotateRight(a + b, 43) + RotateRight(c, 30) + d,
                a + RotateRight(b + k2, 18) + c,
                mul);
        }

        static ulong Hash64_16b(ReadOnlySpan<byte> dataArray)
        {
            var dataLength = dataArray.Length;

            if (dataLength >= 8)
            {
                var mul = k2 + (ulong)(dataLength * 2);
                var a = BinaryPrimitives.ReadUInt64LittleEndian(dataArray) + k2;
                var b = BinaryPrimitives.ReadUInt64LittleEndian(dataArray.Slice(dataLength - 8));
                var c = (RotateRight(b, 37) * mul) + a;
                var d = (RotateRight(a, 25) + b) * mul;

                return ComputeHash16(c, d, mul);
            }

            if (dataLength >= 4)
            {
                var mul = k2 + ((ulong)dataLength * 2);
                var a = (ulong)BinaryPrimitives.ReadUInt32LittleEndian(dataArray) << 3;

                return ComputeHash16((ulong)dataLength + a, BinaryPrimitives.ReadUInt32LittleEndian(dataArray.Slice(dataLength - 4)), mul);
            }

            if (dataLength > 0)
            {
                var a = dataArray[0];
                var b = dataArray[dataLength >> 1];
                var c = dataArray[dataLength - 1];

                var y = a + (((uint)b) << 8);
                var z = (uint)dataLength + (((uint)c) << 2);

                return ShiftMix(y * k2 ^ z * k0) * k2;
            }

            return k2;
        }

        static ulong ComputeHash16(ulong u, ulong v, ulong mul)
        {
            var a = (u ^ v) * mul;
            a ^= (a >> 47);

            var b = (v ^ a) * mul;
            b ^= (b >> 47);
            b *= mul;

            return b;
        }

        static ulong RotateRight(ulong operand, int shiftCount)
        {
            shiftCount &= 0x3f;

            return
                (operand >> shiftCount) |
                (operand << (64 - shiftCount));
        }

        static ulong ShiftMix(ulong value) =>
            value ^ (value >> 47);
    }
}
