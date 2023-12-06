
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

	readonly (int time, int distance)[] Part1Input = Day6.Inputs.Part1Input;
	readonly long Time = Day6.Inputs.Time;
	readonly long Distance = Day6.Inputs.Distance;

	[GlobalSetup]
	public void GlobalSetup() { AssertResults(); }

	public void AssertResults()
	{
		const int Part1Result = 2344708;
		const int Part2Result = 30125202;

		Func<int>[] part1s =
		[
			Part1Solution1,
			Part1Solution2
		];

		Func<int>[] part2s =
		[
			Part2Solution1,
			Part2Solution2,
			Part2Solution3,
			Part2Solution4,
			Part2Threads10,
			Part2Threads50,
			Part2Threads100,
			Part2Threads500,
			Part2Threads1000
		];

		foreach ((int result, Func<int>[] funcs) in new[] { (Part1Result, part1s), (Part2Result, part2s) })
		{
			foreach (var func in funcs)
			{
				int v = func();
				if (v != result) throw new Exception(func.Method.Name);
			}
		}
	}

	[Benchmark]
	public int Part1Solution1()
	{
		var input = Part1Input;
		int accumulator = 1;

		foreach (var (time, distance) in input)
		{
			int waysToBeat = 0;

			for (int i = 0; i < time; i++)
			{
				int timeSpentPressingButton = i;
				int velocity = i;

				int timeForMoving = time - timeSpentPressingButton;
				int distanceTraveled = velocity * timeForMoving;
				if (distanceTraveled > distance) waysToBeat++;
			}

			accumulator *= waysToBeat;
		}

		return accumulator;
	}

	[Benchmark]
	public int Part1Solution2()
	{
		var input = Part1Input;
		int accumulator = 1;

		foreach (var (time, distance) in input)
		{
			int start = -1;
			int end = -1;

			for (int i = 0; i < time; i++)
			{
				if (i * (time - i) > distance)
				{
					start = i;
					break;
				}
			}

			for (int i = time; i >= 0; i--)
			{
				if (i * (time - i) > distance)
				{
					end = i;
					break;
				}
			}

			accumulator *= end - start + 1;
		}

		return accumulator;
	}

	[Benchmark]
	public int Part2Solution1()
	{
		int accumulator = 0;

		for (long i = 0; i < Time; i++)
		{
			long timeSpentPressingButton = i;
			long velocity = i;

			long timeForMoving = Time - timeSpentPressingButton;
			long distanceTraveled = velocity * timeForMoving;
			if (distanceTraveled > Distance) accumulator++;
		}

		return accumulator;
	}


	[Benchmark]
	public int Part2Solution2()
	{
		int accumulator = 0;

		for (long i = 0; i < Time; i++)
		{
			if (i * (Time - i) > Distance) accumulator++;
		}

		return accumulator;
	}

	[Benchmark]
	public int Part2Solution3()
	{
		int accumulator = 0;

		for (long i = 0; i < Time; i++)
		{
			if (i * (Time - i) > Distance) accumulator++;
		}

		return accumulator;
	}

	[Benchmark]
	public int Part2Solution4()
	{
		long startnum = long.MaxValue;
		long endnum = long.MinValue;

		for (long i = 0; i < Time; i++)
		{
			if (i * (Time - i) > Distance)
			{
				startnum = i;
				break;
			}
		}

		for (long i = Time; i >= 0; i--)
		{
			if (i * (Time - i) > Distance)
			{
				endnum = i;
				break;
			}
		}

		return (int)(endnum - startnum) + 1;
	}

	// [Benchmark] 
	public int Part2Threads10() => RunThreaded(10);
	// [Benchmark] 
	public int Part2Threads50() => RunThreaded(50);
	// [Benchmark] 
	public int Part2Threads100() => RunThreaded(100);
	// [Benchmark] 
	public int Part2Threads500() => RunThreaded(500);
	// [Benchmark] 
	public int Part2Threads1000() => RunThreaded(1000);


	static int RunThreaded(int threadCount)
	{
		const int Time = 44899691;
		const long Distance = 277113618901768;
		int accumulator = 0;

		Thread[] threads = new Thread[threadCount];
		Chunk[] chunks = new Chunk[threadCount];
		int rangeSize = Time / threadCount;

		chunks[threadCount - 1] = new(Time - rangeSize, Time);
		for (int i = threadCount - 2; i >= 0; i--)
		{
			chunks[i] = new(i * rangeSize, chunks[i + 1].Start - 1);
		}

		for (int outer = 0; outer < threadCount; outer++)
		{
			int start = chunks[outer].Start;
			int end = chunks[outer].End;

			threads[outer] = new Thread(() =>
			{
				for (long inner = start; inner <= end; inner++)
				{
					if (inner * (Time - inner) > Distance)
					{
						Interlocked.Increment(ref accumulator);
					}
				}
			});
			threads[outer].Start();
		}

		for (int i = 0; i < threadCount; i++)
		{
			threads[i].Join();
		}

		return accumulator;
	}

	record struct Chunk(int Start, int End);
}
