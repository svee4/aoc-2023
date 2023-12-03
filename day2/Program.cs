
#if DEBUG

var b = new Day2.Benchmark();
b.GlobalSetup();

#else

BenchmarkDotNet.Running.BenchmarkRunner.Run<Day2.Benchmark>();

#endif