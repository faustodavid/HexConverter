using System;
using System.Buffers;
using System.Linq;
using System.Text;
using Xunit;

namespace HexConverter.UnitTests
{
    public class HexConverterTests
    {
        [Fact]
        public void GetBytes_from_valid_hex_returns_base_16()
        {
            string text = Guid.NewGuid().ToString();
            byte[] expectedTextInBytes = Encoding.UTF8.GetBytes(text);
            string hexText = string.Concat(expectedTextInBytes.Select(b => b.ToString("x2")));

            byte[] actual = HexConverter.GetBytes(hexText);

            Assert.Equal(expectedTextInBytes, actual);
        }

        [Fact]
        public void GetBytes_using_buffer_from_valid_hex_returns_base_16()
        {
            string text = Guid.NewGuid().ToString();
            byte[] expectedTextInBytes = Encoding.UTF8.GetBytes(text);
            string hexText = string.Concat(expectedTextInBytes.Select(b => b.ToString("x2")));

            byte[] buffer = ArrayPool<byte>.Shared.Rent(HexConverter.GetBytesCount(hexText));
            int count = HexConverter.GetBytes(hexText, buffer);

            Assert.Equal(expectedTextInBytes.Length, count);
            Assert.Equal(expectedTextInBytes, new ArraySegment<byte>(buffer, 0, count));
        }

        [Fact]
        public void GetBytesPooled_from_valid_hex_returns_base_16()
        {
            string text = Guid.NewGuid().ToString();
            byte[] expectedTextInBytes = Encoding.UTF8.GetBytes(text);
            string hexText = string.Concat(expectedTextInBytes.Select(b => b.ToString("x2")));

            using RentedArraySegmentWrapper<byte> actual = HexConverter.GetBytesPooled(hexText);

            Assert.Equal(expectedTextInBytes, actual.ArraySegment);
        }

        [Fact]
        public void GetHex_from_valid_bytes_base_16_returns_hex()
        {
            string text = Guid.NewGuid().ToString();
            byte[] expectedTextInBytes = Encoding.UTF8.GetBytes(text);
            string expectedHexText = string.Concat(expectedTextInBytes.Select(b => b.ToString("x2")));
            byte[] hexInBytes = Enumerable.Range(0, expectedHexText.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(expectedHexText.Substring(x, 2), 16))
                .ToArray();

            string actual = HexConverter.GetHex(hexInBytes);
            
            Assert.Equal(expectedHexText, actual);
        }
        
        [Fact]
        public void GetHex_using_buffer_from_valid_bytes_base_16_returns_hex()
        {
            string text = Guid.NewGuid().ToString();
            byte[] expectedTextInBytes = Encoding.UTF8.GetBytes(text);
            string expectedHexText = string.Concat(expectedTextInBytes.Select(b => b.ToString("x2")));
            byte[] hexInBytes = Enumerable.Range(0, expectedHexText.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(expectedHexText.Substring(x, 2), 16))
                .ToArray();

            char[] buffer = ArrayPool<char>.Shared.Rent(HexConverter.GetCharsCount(hexInBytes));
            int count = HexConverter.GetHex(hexInBytes, buffer);
            
            Assert.Equal(expectedHexText.Length, count);
            Assert.Equal(expectedHexText, buffer.AsSpan(0, count).ToString());
        }
        
        [Fact]
        public void GetHexPooled_from_valid_bytes_base_16_returns_hex()
        {
            string text = Guid.NewGuid().ToString();
            byte[] expectedTextInBytes = Encoding.UTF8.GetBytes(text);
            string expectedHexText = string.Concat(expectedTextInBytes.Select(b => b.ToString("x2")));
            byte[] hexInBytes = Enumerable.Range(0, expectedHexText.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(expectedHexText.Substring(x, 2), 16))
                .ToArray();

            using RentedArraySegmentWrapper<char> actual = HexConverter.GetHexPooled(hexInBytes);
            
            Assert.Equal(expectedHexText.Length, actual.ArraySegment.Count);
            Assert.Equal(expectedHexText, actual.ArraySegment.AsSpan().ToString());
        }
    }
}