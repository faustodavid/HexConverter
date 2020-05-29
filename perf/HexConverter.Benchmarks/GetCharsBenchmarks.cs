using System;
using System.Buffers;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace HexConverter.Benchmarks
{
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class GetCharsBenchmarks
    {
        private byte[] s_fakeHexBytes = Array.Empty<byte>();

        [Params(4, 20)] public int N { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            StringBuilder sb = new StringBuilder(32 * N);
            for (var i = 0; i < N; i++) sb.Append(Guid.NewGuid().ToString());

            string text = sb.ToString();
            byte[] expectedTextInBytes = Encoding.UTF8.GetBytes(text);
            string hex = HexConverter.GetString(expectedTextInBytes);
            s_fakeHexBytes = HexConverter.GetBytes(hex);
        }

        [Benchmark(Baseline = true)]
        public int Linq()
        {
            return string.Concat(s_fakeHexBytes.Select(b => b.ToString("x2"))).Length;
        }

        [Benchmark]
        public int GetString()
        {
            return HexConverter.GetString(s_fakeHexBytes).Length;
        }

        [Benchmark]
        public int BitConverterGetString()
        {
            return BitConverter.ToString(s_fakeHexBytes).Replace("-", "").Length;
        }

        [Benchmark]
        public int GetCharsBuffered()
        {
            ArrayPool<char> arrayPool = ArrayPool<char>.Shared;
            var requiredBufferSize = HexConverter.GetCharsCount(s_fakeHexBytes);
            char[]? disposableBuffer = null;
            var buffer = requiredBufferSize > 256
                ? disposableBuffer = arrayPool.Rent(requiredBufferSize)
                : stackalloc char[requiredBufferSize];
            try
            {
                return HexConverter.GetChars(s_fakeHexBytes, buffer);
            }
            finally
            {
                if (!(disposableBuffer is null)) arrayPool.Return(disposableBuffer);
            }
        }

        [Benchmark]
        public int GetCharsPooled()
        {
            using var pooledHex = HexConverter.GetCharsPooled(s_fakeHexBytes);
            return pooledHex.ArraySegment.Count;
        }
    }
}