using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#if DEBUG
Benchmark b = new();
foreach (var f in typeof(Benchmark).GetMethods().Where(m => m.GetCustomAttributes(typeof(BenchmarkAttribute), false).Length > 0))
	f.Invoke(b, null);
#else
BenchmarkDotNet.Running.BenchmarkRunner.Run<Benchmark>();
#endif

[MemoryDiagnoser]
public class Benchmark
{

	[Benchmark]
	public void TestAllocNormalValues()
	{
		long a = 12345678;
		long b = 123456789101112;
		int c = 0;
		for (long i = 0; i < a; i++)
		{
			if (i * (a - i) > b) c++;
		}
	}

	[Benchmark]
	public void TestAllocWithBigValues()
	{
		long a = 99999999;
		long b = 999999999999999;
		int c = 0;
		for (long i = 0; i < a; i++)
		{
			if (i * (a - i) > b) c++;
		}
	}

	[Benchmark]
	public void TestAllocWithInts()
	{
		int a = 99999999;
		int b = int.MaxValue;
		int c = 0;
		for (long i = 0; i < a; i++)
		{
			if (i * (a - i) > b) c++;
		}
	}

}