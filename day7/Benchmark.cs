
using BenchmarkDotNet.Attributes;
using static Day7.Helpers;


namespace Day7;


[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class Benchmark
{

	string Input = null!;


	[GlobalSetup]
	public void GlobalSetup()
	{
		Input = @"32T3K 765
T55J5 684
KK677 28
KTJJT 220
QQQJA 483
";
		Input = File.ReadAllText("/home/alex/code/aoc/23/day7/input1.txt");
		Input = Input.Replace("\r", "");

		AssertResults();
	}


	public void AssertResults()
	{
		if (Part1Solution1() != 251216224) throw new Exception(nameof(Part1Solution1));
		if (Part2Solution1() != 250825971) throw new Exception(nameof(Part2Solution1));
	}


	[Benchmark]
	public long Part1Solution1() => Solve(false);

	[Benchmark]
	public long Part2Solution1() => Solve(true);
	

	long Solve(bool isPart2) =>
		Input
			.Split('\n', StringSplitOptions.RemoveEmptyEntries)
			.Select(line => Hand.FromString(line, isPart2))
			.OrderBy(Self, new Hand.Comparer(isPart2 ? Part2Labels : Part1Labels, isPart2))
			//.Log()
			.Select((cur, index) => (hand: cur, rank: ++index))
			.Aggregate(0, (cum, cur) => cum += cur.rank * cur.hand.Bid);

}


