using System;
using System.Buffers;
using HexConverter.Exceptions;

namespace HexConverter
{
    public static class HexConverter
    {
        public static int GetBytesCount(ReadOnlySpan<char> src)
        {
            return src.Length / 2;
        }

        public static byte[] GetBytes(ReadOnlySpan<char> source)
        {
            byte[] buffer = new byte[GetBytesCount(source)];
            GetBytes(source, buffer);
            return buffer;
        }

        public static int GetBytes(ReadOnlySpan<char> source, Span<byte> buffer)
        {
            if (source.Length % 2 != 0) throw new HexConverterInvalidLengthException();

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

        public static RentedArraySegmentWrapper<byte> GetBytesPooled(ReadOnlySpan<char> source)
        {
            ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;
            byte[] buffer = arrayPool.Rent(GetBytesCount(source));
            var count = GetBytes(source, buffer);
            return new RentedArraySegmentWrapper<byte>(new ArraySegment<byte>(buffer, 0, count), arrayPool);
        }

        public static int GetCharsCount(ReadOnlySpan<byte> bytes)
        {
            return bytes.Length * 2;
        }

        public static string GetHex(ReadOnlySpan<byte> source)
        {
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

        public static RentedArraySegmentWrapper<char> GetHexPooled(ReadOnlySpan<byte> bytes)
        {
            ArrayPool<char> arrayPool = ArrayPool<char>.Shared;
            var requiredBufferSize = GetCharsCount(bytes);
            char[] buffer = arrayPool.Rent(requiredBufferSize);
            var count = GetHex(bytes, buffer);
            return new RentedArraySegmentWrapper<char>(new ArraySegment<char>(buffer, 0, count), arrayPool);
        }

        public static int GetHex(ReadOnlySpan<byte> source, Span<char> buffer)
        {
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