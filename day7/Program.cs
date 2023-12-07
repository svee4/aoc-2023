
#if DEBUG
new Day7.Benchmark().GlobalSetup();
#else
BenchmarkDotNet.Running.BenchmarkRunner.Run<Day7.Benchmark>();
#endif


