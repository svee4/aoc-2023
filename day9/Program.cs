
using BenchmarkDotNet.Attributes;

#if DEBUG
new Benchmark().GlobalSetup();
#else
BenchmarkDotNet.Running.BenchmarkRunner.Run<Benchmark>();
#endif


[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
public class Benchmark
{

	string Input = null!;

	[GlobalSetup]
	public void GlobalSetup() {
		Input = File.ReadAllText("/home/alex/code/aoc/23/day9/input1.txt");
//		Input = @"0 3 6 9 12 15
//1 3 6 10 15 21
//10 13 16 21 30 45";
		Input = Input.Replace("\r", "");
		AssertResults(); 
	}

	public void AssertResults() 
	{
		Console.WriteLine(Part1Solution1());
        Console.WriteLine(Part2Solution1());
    }


	[Benchmark]
	public long Part1Solution1() 
	{
		long accumulator = 0;
		foreach (string line in Input.Split('\n', StringSplitOptions.RemoveEmptyEntries))
		{
			int[] numbers = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

			List<int> lastNumbers = [];

			static void Diff(List<int> diffable, List<int> lastNumbers)
			{
				if (diffable.Count == 0) return;

				List<int> newList = [];
				int prev = diffable[0];
				foreach (int num in diffable.Skip(1))
				{
					newList.Add(num - prev);
					prev = num;
				}

				lastNumbers.Add(prev);
				if (newList.Any(n => n != 0)) Diff(newList, lastNumbers);
			}

			Diff(numbers.ToList(), lastNumbers);

			accumulator += lastNumbers.Aggregate((cum, cur) => cum + cur);
        }
		return accumulator;
	}


    [Benchmark]
    public long Part2Solution1()
    {
        // 364717934
        long accumulator = 0;
        foreach (string line in Input.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            int[] numbers = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

            List<int> firstNumbers = [];

            static void Diff(List<int> diffable, List<int> firstNumbers)
            {
                if (diffable.Count == 0) return;

                List<int> newList = [];
				int prev = diffable[0];
                foreach (int num in diffable.Skip(1))
                {
                    newList.Add(num - prev);
                    prev = num;
                }

				firstNumbers.Add(diffable[0]);
                if (newList.Any(n => n != 0)) Diff(newList, firstNumbers);
            }

            Diff(numbers.ToList(), firstNumbers);

			firstNumbers.Reverse();
			int num = firstNumbers.Aggregate((cum, cur) => cur - cum);
			accumulator += num;

        }
        return accumulator;
    }

}
