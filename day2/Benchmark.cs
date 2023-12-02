using BenchmarkDotNet.Attributes;

namespace Day2;

[MemoryDiagnoser]
[RankColumn]
public class Benchmark
{

	static readonly string InputData = File.ReadAllText("/home/alex/code/aoc/23/day2/input1.txt");


	[Benchmark]
	public void BenchmarkPart1()
	{
		long result = Part1.Solve(InputData.AsSpan());
		if (result != 2771) throw new Exception();
	}

	[Benchmark]
	public void BenchmarkPart2()
	{
		long result = Part2.Solve(InputData.AsSpan());
		if (result != 70924) throw new Exception();
	}
}