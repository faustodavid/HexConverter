using BenchmarkDotNet.Running;

namespace Numeral.Benchmarks
{
    class Program
    {
        public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}