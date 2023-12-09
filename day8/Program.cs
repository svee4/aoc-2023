
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
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
		Input = File.ReadAllText("/home/alex/code/aoc/23/day8/input1.txt");
		Input = Input.Replace("\r", "");
		AssertResults();
	}

	public void AssertResults() 
	{
		TestStringToId();
		TestP2NodeCount();
        TestUlongNode();


        const long Part1 = 21409;
		const long Part2 = 21165830176709;

		if (Part1Solution1() != Part1) throw new Exception(nameof(Part1Solution1));
        if (Part1Solution2() != Part1) throw new Exception(nameof(Part1Solution2));
        if (Part2Solution1() != Part2) throw new Exception(nameof(Part2Solution1));
        if (Part2Solution2() != Part2) throw new Exception(nameof(Part2Solution2));
    }

	record Node(string Name, string Left, string Right);

	[Benchmark]
	public int Part1Solution1()
	{
		string[] splits = Input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		char[] instructions = splits[0].Trim().ToCharArray();
		Node[] nodes = splits.Skip(1).Select(str => new Node(str[..3], str.Substring("AAA = (".Length, 3), str.Substring("AAA = (BBB, ".Length, 3))).ToArray();

		int nodeIndex = Array.FindIndex(nodes, node => node.Name == "AAA");
		int instructionIndex = 0;
		int hops = 0;
		while (true)
		{
			string nextnode;

			if (instructions[instructionIndex] == 'L')
				nextnode = nodes[nodeIndex].Left;
			else if (instructions[instructionIndex] == 'R')
				nextnode = nodes[nodeIndex].Right;
			else
				throw new UnreachableException();

			instructionIndex++;
			if (instructionIndex >= instructions.Length) instructionIndex = 0;

			nodeIndex = Array.FindIndex(nodes, node => node.Name == nextnode);
			if (nodeIndex == -1) throw new UnreachableException();
            hops++;

			if (nextnode == "ZZZ") break;
        }

		return hops;
	}




    [Benchmark]
    public long Part2Solution1()
    {
        string[] splits = Input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        char[] instructions = splits[0].Trim().ToCharArray();
        Node[] nodes = splits.Skip(1).Select(str => new Node(str[..3], str.Substring("AAA = (".Length, 3), str.Substring("AAA = (BBB, ".Length, 3))).ToArray();


        List<int> startNodesIndexes = [];
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].Name[2] == 'A') startNodesIndexes.Add(i);
        }

        List<long> results = [];

        foreach (int index in startNodesIndexes)
        {
            int nodeIndex = index;
            int instructionIndex = 0;
            long hops = 0;
            while (true)
            {
                string nextnode;

                if (instructions[instructionIndex] == 'L')
                    nextnode = nodes[nodeIndex].Left;
                else if (instructions[instructionIndex] == 'R')
                    nextnode = nodes[nodeIndex].Right;
                else
                    throw new UnreachableException();

                instructionIndex++;
                if (instructionIndex >= instructions.Length) instructionIndex = 0;

                nodeIndex = Array.FindIndex(nodes, node => node.Name == nextnode);
                if (nodeIndex == -1) throw new UnreachableException();
                hops++;

                if (nextnode[2] == 'Z')
                {
                    break;
                }
            }
            results.Add(hops);
        }

        return Helpers.LeastCommonMultiple([.. results]);
    }


    // need 5 bits to represent all 25 possible chars A-Z
    // ids are 3 chars long
    // 5 * 3 == 15 => use short
    // when s = "AHZ"
    // return = 0b0['A' - 'A']['H' - 'A']['Z' - 'A']
    // 0b0AAAAABBBBBCCCCC
    static ushort StringToId(ReadOnlySpan<char> s) => 
		(ushort)((s[0] - 'A') << 10 | (s[1] - 'A') << 5 | (s[2] - 'A'));


	readonly struct P2Node(ushort id, ushort left, ushort right)
	{
		readonly public ushort Id = id;
		readonly public ushort Left = left;
		readonly public ushort Right = right;

		public static P2Node FromStrings(ReadOnlySpan<char> id, ReadOnlySpan<char> left, ReadOnlySpan<char> right) => 
			new(StringToId(id), StringToId(left), StringToId(right));
	}


	[Benchmark]
    public long Part1Solution2()
    {
		const int NodeLineLength = 17; // "AAA = (BBB, CCC)\n".Length;
        ushort AAA = StringToId(['A', 'A', 'A']);
		ushort ZZZ = StringToId(['Z', 'Z', 'Z']);

		int instructionsLength = Input.IndexOf('\n');
		int nodeCount = (Input.Length - instructionsLength) / NodeLineLength;

		Span<bool> instructions = stackalloc bool[instructionsLength];
		for (int i = 0; i < instructionsLength; i++) 
			instructions[i] = Input[i] == 'L';

		Span<P2Node> prenodes = stackalloc P2Node[nodeCount]; 
		ushort maxId = 0;
		for (int i = 0; i < nodeCount; i++)
		{
			P2Node node = P2Node.FromStrings(
					Input.AsSpan(instructionsLength + 2 + (i * NodeLineLength) + 0,  3),
					Input.AsSpan(instructionsLength + 2 + (i * NodeLineLength) + 7,  3), // "AAA = (".Length
					Input.AsSpan(instructionsLength + 2 + (i * NodeLineLength) + 12, 3)); // "AAA = (BBB, ".Length

			if (node.Id > maxId) maxId = node.Id;
			prenodes[i] = node;
		}

		Span<P2Node> nodes = stackalloc P2Node[maxId + 1];
		int nodeIndex = -1;

        for (int i = 0; i < prenodes.Length; i++)
		{
			P2Node node = prenodes[i];
			if (node.Id == AAA) nodeIndex = node.Id;
			nodes[node.Id] = node;
		}
		
        int instructionIndex = 0;
        int hops = 0;

        while (true)
        {
			nodeIndex = instructions[instructionIndex] ? nodes[nodeIndex].Left : nodes[nodeIndex].Right;
            hops++;

            if (nodeIndex == ZZZ) break;

            instructionIndex++;
            if (instructionIndex >= instructions.Length) instructionIndex = 0;
        }

        return hops;
    }

    [Benchmark]
    public long Part2Solution2()
    {
        const int NodeLineLength = 17; // "AAA = (BBB, CCC)\n".Length;

        int instructionsLength = Input.IndexOf('\n');
        int nodeCount = (Input.Length - instructionsLength) / NodeLineLength;

        Span<bool> instructions = stackalloc bool[instructionsLength];
        for (int i = 0; i < instructionsLength; i++)
            instructions[i] = Input[i] == 'L';

        Span<P2Node> prenodes = stackalloc P2Node[nodeCount];
        ushort maxId = 0;
		int startNodeCount = 0;

        for (int i = 0, inslen = instructionsLength + 2; i < nodeCount; i++)
        {
            var node = P2Node.FromStrings(
                    Input.AsSpan(i * NodeLineLength + inslen + 0,  3),
                    Input.AsSpan(i * NodeLineLength + inslen + 7,  3), // "AAA = (".Length
                    Input.AsSpan(i * NodeLineLength + inslen + 12, 3)); // "AAA = (BBB, ".Length

            if (node.Id > maxId) maxId = node.Id;
			if (Helpers.EndsInA(node.Id)) startNodeCount++;
            prenodes[i] = node;
        }

        // node id == index in nodes
        // so we can do nodes[node.id]
        // and also do Helpers.EndsInZ(index)
        Span<P2Node> nodes = stackalloc P2Node[maxId + 1];
        // this array is also where we store the amount of hops after that specific nodeIndex is not needed anymore
        Span<ushort> nodeIndexes = stackalloc ushort[startNodeCount];

        for (int i = 0, nodeIndexesIndex = 0; i < prenodes.Length; i++)
        {
            P2Node node = prenodes[i];
			if (Helpers.EndsInA(node.Id)) nodeIndexes[nodeIndexesIndex++] = node.Id;
            nodes[node.Id] = node;
        }

		for (int i = 0; i < nodeIndexes.Length; i++)
		{
            ushort nodeIndex = nodeIndexes[i];
            int instructionIndex = 0;
            ushort hops = 0; // is ushort enough..? its because we put the hops in nodeIndexes after loop

            while (true)
            {
                nodeIndex = instructions[instructionIndex] ? nodes[nodeIndex].Left : nodes[nodeIndex].Right;
#if DEBUG
                hops = (ushort)checked(hops + 1);
#else
                hops++;
#endif

                if (Helpers.EndsInZ(nodeIndex)) break;

                instructionIndex++;
                if (instructionIndex >= instructions.Length) instructionIndex = 0;
            }

			nodeIndexes[i] = hops;
        }

        return Helpers.LeastCommonMultipleFast(nodeIndexes);
    }


    void TestStringToId()
    {
        string input = "AHZ";
        ushort output = StringToId(input);
        if (output != 0b0_00000_00111_11001) throw new Exception("Invalid result from TestP1S2StringToId");
    }

    void TestP2NodeCount()
    {
        int instructionsLength = Input.IndexOf('\n');

        int temp = Input.Length - instructionsLength;
        // one node is "AAA = (BBB, CCC)\n"
        int nodeCount = temp / "AAA = (BBB, CCC)\n".Length;

        int nodeCount2 = Input.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length - 1;
        if (nodeCount != nodeCount2) throw new Exception("Invalid result from TestP1S2NodeCount");
    }
}

static class Helpers
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsInA(ushort number) => (uint)number << 32 - 5 >> 32 - 5 == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsInZ(ushort number) => (uint)number << 32 - 5 >> 32 - 5 == 25;


    // https://stackoverflow.com/a/29717490
    public static long LeastCommonMultiple(long[] numbers)
    {
        return numbers.Aggregate((cum, cur) => Math.Abs(cum * cur) / GCD(cum, cur));
    }

    static long GCD(long a, long b)
    {
        return b == 0 ? a : GCD(b, a % b);
    }

	public static long LeastCommonMultipleFast(ReadOnlySpan<ushort> numbers)
	{
		long acc = numbers[0];
		for (int i = 1; i < numbers.Length; i++)
			acc = acc * numbers[i] / GCDFast(acc, numbers[i]);
		
		return acc;
	}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static long GCDFast(long a, long b)
	{
		long temp;
Start:
		if (b == 0) return a;
		temp = b;
		b = a % b;
		a = temp;
		goto Start;
	}
}