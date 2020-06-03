using System;
using System.Linq;
using System.Text;
using Xunit;

namespace Numeral.UnitTests
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

            Span<byte> buffer = stackalloc byte[HexConverter.GetBytesCount(hexText)];
            int count = HexConverter.GetBytes(hexText, buffer);

            Assert.Equal(expectedTextInBytes.Length, count);
            for (var index = 0; index < expectedTextInBytes.Length; index++)
            {
               Assert.Equal(expectedTextInBytes[index], buffer[index]);
            }
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

            string actual = HexConverter.GetString(hexInBytes);
            
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

           Span<char> buffer = stackalloc char[HexConverter.GetCharsCount(hexInBytes)];
            int count = HexConverter.GetChars(hexInBytes, buffer);
            
            Assert.Equal(expectedHexText.Length, count);
            Assert.Equal(expectedHexText, buffer.Slice(0, count).ToString());
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

            using RentedArraySegmentWrapper<char> actual = HexConverter.GetCharsPooled(hexInBytes);
            
            Assert.Equal(expectedHexText.Length, actual.ArraySegment.Count);
            Assert.Equal(expectedHexText, actual.ArraySegment.AsSpan().ToString());
        }
    }
}