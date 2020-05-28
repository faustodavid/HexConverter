using System;
using System.Buffers;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace HexConverter.Benchmarks
{
    [MemoryDiagnoser]
    [MarkdownExporterAttribute.GitHub]
    public class GetBytesBenchmarks
    {
        [Params(4, 20)] 
        public int N { get; set; }
        
        private string s_fakeHex = string.Empty;
        
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
            s_fakeHex = string.Concat(expectedTextInBytes.Select(b => b.ToString("x2")));
        }

        [Benchmark(Baseline = true)]
        public int Linq()
        {
            string hex = s_fakeHex;
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray().Length;
        }

        [Benchmark]
        public int GetBytes()
        {
            string hex = s_fakeHex;
            return HexConverter.GetBytes(hex).Length;
        }
        
        [Benchmark]
        public int GetBytesBuffered()
        {
            ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;
            string hex = s_fakeHex;
            int requiredSize = HexConverter.GetBytesCount(hex);
            byte[]? disposableBuffer = null;
            Span<byte> buffer = requiredSize > 256
                ? (disposableBuffer = arrayPool.Rent(requiredSize))
                : stackalloc byte[requiredSize];
            try
            {
                int count = HexConverter.GetBytes(hex, buffer);
                return count;
            }
            finally
            {
                if(!(disposableBuffer is null)) arrayPool.Return(disposableBuffer);
            }
        }
        
        [Benchmark]
        public int GetBytesPooled()
        {
            string hex = s_fakeHex;
            using var bytes = HexConverter.GetBytesPooled(hex);
            return bytes.ArraySegment.Count;
        }
    }
}