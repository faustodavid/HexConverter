﻿using System;
using System.Buffers;
using HexConverter.Exceptions;

namespace HexConverter
{
    /// <summary>
    /// High-performance Hexadecimal converter
    /// </summary>
    public static class HexConverter
    {
        /// <summary>
        /// Get the total amount of bytes required. Useful for buffering.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int GetBytesCount(ReadOnlySpan<char> source)
        {
            return source.Length / 2;
        }

        /// <summary>
        /// Get bytes from hexadecimal source.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="HexConverterInvalidSourceLengthException"></exception>
        /// <exception cref="HexConverterBufferCapacityException"></exception>
        public static byte[] GetBytes(ReadOnlySpan<char> source)
        {
            byte[] buffer = new byte[GetBytesCount(source)];
            GetBytes(source, buffer);

            return buffer;
        }

        /// <summary>
        /// Get bytes from hexadecimal source. Allowing buffering for high-performance code.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="HexConverterInvalidSourceLengthException"></exception>
        /// <exception cref="HexConverterBufferCapacityException"></exception>
        public static int GetBytes(ReadOnlySpan<char> source, Span<byte> buffer)
        {
            if (source.Length % 2 != 0) throw new HexConverterInvalidSourceLengthException();

            var requiredBufferSize = source.Length / 2;
            var indexOnBuffer = requiredBufferSize;
            var indexOnSource = source.Length;

            if (buffer.Length < requiredBufferSize) throw new HexConverterBufferCapacityException();

            while (--indexOnBuffer >= 0)
            {
                var low = source[--indexOnSource];
                var high = source[--indexOnSource];

                buffer[indexOnBuffer] = (byte) (
                    ((
                         high >= '0' && high <= '9' ? high - '0' :
                         high >= 'a' && high <= 'f' ? high - 'a' + 10 :
                         high >= 'A' && high <= 'F' ? high - 'A' + 10 :
                         0)
                     << 4) |
                    (
                        low >= '0' && low <= '9' ? low - '0' :
                        low >= 'a' && low <= 'f' ? low - 'a' + 10 :
                        low >= 'A' && low <= 'F' ? low - 'A' + 10 :
                        0));
            }

            return requiredBufferSize;
        }

        /// <summary>
        /// Get bytes from hexadecimal source using ArrayPool to avoid allocations.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Bytes on a disposable wrapper to return to array to the pool when dispose</returns>
        /// <exception cref="HexConverterInvalidSourceLengthException"></exception>
        public static RentedArraySegmentWrapper<byte> GetBytesPooled(ReadOnlySpan<char> source)
        {
            if (source.Length == 0) throw new HexConverterInvalidSourceLengthException();

            ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;
            byte[] buffer = arrayPool.Rent(GetBytesCount(source));
            var count = GetBytes(source, buffer);

            return new RentedArraySegmentWrapper<byte>(new ArraySegment<byte>(buffer, 0, count), arrayPool);
        }

        /// <summary>
        /// Get the total amount of chars required. Useful for buffering.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static int GetCharsCount(ReadOnlySpan<byte> bytes)
        {
            return bytes.Length * 2;
        }

        /// <summary>
        /// Get hexadecimal from bytes
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="HexConverterInvalidSourceLengthException"></exception>
        public static string GetHex(ReadOnlySpan<byte> source)
        {
            if (source.Length == 0) throw new HexConverterInvalidSourceLengthException();

            ArrayPool<char> arrayPool = ArrayPool<char>.Shared;
            var requiredBufferSize = GetCharsCount(source);
            char[]? disposableBuffer = null;

            var buffer = requiredBufferSize > 128
                ? disposableBuffer = arrayPool.Rent(requiredBufferSize)
                : stackalloc char[requiredBufferSize];

            try
            {
                GetHex(source, buffer);
                return buffer.ToString();
            }
            finally
            {
                if (!(disposableBuffer is null)) arrayPool.Return(disposableBuffer);
            }
        }
        
        /// <summary>
        /// Get hexadecimal chars from bytes source using ArrayPool to avoid allocations.

        /// </summary>
        /// <param name="source"></param>
        /// <returns>Chars on a disposable wrapper to return to array to the pool when dispose</returns>
        /// <exception cref="HexConverterInvalidSourceLengthException"></exception>
        public static RentedArraySegmentWrapper<char> GetHexPooled(ReadOnlySpan<byte> source)
        {
            if (source.Length == 0) throw new HexConverterInvalidSourceLengthException();

            ArrayPool<char> arrayPool = ArrayPool<char>.Shared;
            var requiredBufferSize = GetCharsCount(source);
            char[] buffer = arrayPool.Rent(requiredBufferSize);
            var count = GetHex(source, buffer);

            return new RentedArraySegmentWrapper<char>(new ArraySegment<char>(buffer, 0, count), arrayPool);
        }

        /// <summary>
        /// Get hexadecimal chars from bytes source. Allowing buffering for high-performance code.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="HexConverterInvalidSourceLengthException"></exception>
        /// <exception cref="HexConverterBufferCapacityException"></exception>
        public static int GetHex(ReadOnlySpan<byte> source, Span<char> buffer)
        {
            if (source.Length == 0) throw new HexConverterInvalidSourceLengthException();
            if (buffer.Length < GetCharsCount(source)) throw new HexConverterBufferCapacityException();

            var indexOnBuffer = 0;

            for (var indexOnSource = 0; indexOnSource < source.Length; ++indexOnSource, ++indexOnBuffer)
            {
                var b = (byte) (source[indexOnSource] >> 4);
                buffer[indexOnBuffer] = (char) (b > 9 ? b + 0x37 + 0x20 : b + 0x30);

                b = (byte) (source[indexOnSource] & 0x0F);
                buffer[++indexOnBuffer] = (char) (b > 9 ? b + 0x37 + 0x20 : b + 0x30);
            }

            return indexOnBuffer;
        }
    }
}