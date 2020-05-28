using System;
using System.Buffers;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace HexConverter.Benchmarks
{
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class GetHexBenchmarks
    {
        private byte[] s_fakeHexBytes = Array.Empty<byte>();
        
        [Params(4, 20)]
        public int N { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            StringBuilder sb = new StringBuilder(32 * N);
            for (int i = 0; i < N; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string text = sb.ToString();
            byte[] expectedTextInBytes = Encoding.UTF8.GetBytes(text);
            string hex = HexConverter.GetHex(expectedTextInBytes);
            s_fakeHexBytes = HexConverter.GetBytes(hex);
        }

        [Benchmark(Baseline = true)]
        public int Linq()
        {
            return string.Concat(s_fakeHexBytes.Select(b => b.ToString("x2"))).Length;
        }

        [Benchmark]
        public int GetHex()
        {
            return HexConverter.GetHex(s_fakeHexBytes).Length;
        }
        
        [Benchmark]
        public int GetHexBuffered()
        {
            ArrayPool<char> arrayPool = ArrayPool<char>.Shared;
            int requiredBufferSize = HexConverter.GetCharsCount(s_fakeHexBytes);
            char[]? disposableBuffer = null;
            Span<char> buffer = requiredBufferSize > 256
                ? (disposableBuffer = arrayPool.Rent(requiredBufferSize))
                : stackalloc char[requiredBufferSize];
            try
            {
                int count = HexConverter.GetHex(s_fakeHexBytes, buffer);
                return count;
            }
            finally
            {
                if(!(disposableBuffer is null)) arrayPool.Return(disposableBuffer);
            }
        }
        
        [Benchmark]
        public int GetHexPooled()
        {
            using var pooledHex = HexConverter.GetHexPooled(s_fakeHexBytes);
            return pooledHex.ArraySegment.Count;
        }
    }
}