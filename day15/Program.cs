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
	public void GlobalSetup() 
	{
        Console.WriteLine("Vector128: " + Vector128.IsHardwareAccelerated);
        Console.WriteLine("Vector256: " + Vector256.IsHardwareAccelerated);
        Console.WriteLine("Vector512: " + Vector512.IsHardwareAccelerated);

        Input = File.ReadAllText("/home/alex/code/aoc/23/day15/input1.txt");
		//Input = "rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7";
		Input = Input.Replace("\r", "").Replace("\n", "");
        AssertResults();
	}

	public void AssertResults() 
	{

        Console.WriteLine("Part1Solution1: " + Part1Solution1());
        Console.WriteLine("Part1Solution2: " + Part1Solution2());
        Console.WriteLine("Part1Solution3: " + Part1Solution3());
        Console.WriteLine("Part1Solution4: " + Part1Solution4());
        Console.WriteLine("Part1Solution5: " + Part1Solution5());
        Console.WriteLine("Part1Solution6: " + Part1Solution6());
    }


    [Benchmark]
	public int Part1Solution1() 
	{
		int acc = 0;
		int cur = 0;
		string prev = "";
		for (int i = 0; i < Input.Length; i++)
		{
			char c = Input[i];
			if (c == ',')
			{
				//Console.WriteLine($"{prev}: " + cur);
				prev = "";
				acc += cur;
				cur = 0;
				continue;
			}

			prev += c;
			cur += c;
			cur *= 17;
			cur %= 256;
		}

        //Console.WriteLine($"{prev}: " + cur);
        return acc + cur;
	}


	readonly struct SmallRange(int from, int to)
	{
		public readonly int FromInclusive = from;
		public readonly int ToExclusive = to;
	}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector128<uint> SpanToVecUInt(Span<int> span) => Vector128.Create((uint)span[0], (uint)span[1], (uint)span[2], (uint)span[3]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector128<uint> SpanToVecUInt(Span<uint> span) => Vector128.Create(span[0], span[1], span[2], span[3]);
	

	[Benchmark]
	public unsafe int Part1Solution2()
	{
		int acc = 0;

		Span<SmallRange> ranges = stackalloc SmallRange[4];

		for (int i = 0; i < Input.Length; i++)
		{
			if (ranges[0].ToExclusive == 0 && i > Input.Length * 0.25 && Input[i] == ',') ranges[0] = new SmallRange(0, i - 1);
			if (ranges[1].ToExclusive == 0 && i > Input.Length * 0.5 && Input[i] == ',') ranges[1] = new SmallRange(ranges[0].ToExclusive, i - 1);
			if (ranges[2].ToExclusive == 0 && i > Input.Length * 0.75 && Input[i] == ',') ranges[2] = new SmallRange(ranges[1].ToExclusive, i - 1);
        }
		ranges[3] = new SmallRange(ranges[2].ToExclusive, Input.Length);
		Vector128<int> indexes = Vector128.Create(ranges[0].FromInclusive, ranges[1].FromInclusive, ranges[2].FromInclusive, ranges[3].FromInclusive);

		Vector128<uint> multiplier = Vector128.Create(17u);
		Vector128<int> adder = Vector128<int>.One;
		Span<Vector128<int>> skippers =
        [
            Vector128.Create(1, 0, 0, 0),
            Vector128.Create(0, 1, 0, 0),
            Vector128.Create(0, 0, 1, 0),
            Vector128.Create(0, 0, 0, 1),
        ];

        Span<uint> accumulators = stackalloc uint[4];
		Span<int> chars = stackalloc int[4];

        while (true)
        {

			indexes = Vector128.Add(indexes, adder);
			chars[0] = ranges[0].ToExclusive <= indexes[0] ? 0 : Input[indexes[0]];
            chars[1] = ranges[1].ToExclusive <= indexes[1] ? 0 : Input[indexes[1]];
            chars[2] = ranges[2].ToExclusive <= indexes[2] ? 0 : Input[indexes[2]];
            chars[3] = ranges[3].ToExclusive <= indexes[3] ? 0 : Input[indexes[3]];

			if (chars[0] == ',') { acc += (int)accumulators[0]; accumulators[0] = 0; indexes = Vector128.Add(indexes, skippers[0]); chars[0] = Input[indexes[0]]; }
            if (chars[1] == ',') { acc += (int)accumulators[1]; accumulators[1] = 0; indexes = Vector128.Add(indexes, skippers[1]); chars[1] = Input[indexes[1]]; }
            if (chars[2] == ',') { acc += (int)accumulators[2]; accumulators[2] = 0; indexes = Vector128.Add(indexes, skippers[2]); chars[2] = Input[indexes[2]]; }
			if (chars[3] == ',') { acc += (int)accumulators[3]; accumulators[3] = 0; indexes = Vector128.Add(indexes, skippers[3]); chars[3] = Input[indexes[3]]; }


			var vec = SpanToVecUInt(chars);

			if (Vector128.EqualsAll(vec, Vector128<uint>.Zero)) break;

			var vecacc = SpanToVecUInt(accumulators);
            var vec2 = Vector128.Add(vec, vecacc);
			var vec3 = Vector128.Multiply(vec2, multiplier);

            // how does one do modulo with SIMD
			accumulators[0] = vec3[0] % 256;
			accumulators[1] = vec3[1] % 256;
			accumulators[2] = vec3[2] % 256;
			accumulators[3] = vec3[3] % 256;
        }


		return acc;
    }



    [Benchmark]
    public unsafe int Part1Solution3()
    {
        int acc = 0;

        Span<SmallRange> ranges = stackalloc SmallRange[4];

		int x25 = (int)(Input.Length * .25);
		int xlast = 0, xlasti = 0;
        for (int i = 1; i < Input.Length; i++)
        {
			if (i - xlasti > x25 && Input[i] == ',')
			{
				ranges[xlast++] = new(xlasti, i - 1);
				if (xlast == 3) break;
				xlasti = i;
				i += x25;
			}
        }
        ranges[3] = new SmallRange(ranges[2].ToExclusive, Input.Length);
        Vector128<int> indexes = Vector128.Create(ranges[0].FromInclusive, ranges[1].FromInclusive, ranges[2].FromInclusive, ranges[3].FromInclusive);

        Vector128<uint> multiplier = Vector128.Create(17u);
        Vector128<int> adder = Vector128<int>.One;
        Span<Vector128<int>> skippers =
        [
            Vector128.Create(1, 0, 0, 0),
            Vector128.Create(0, 1, 0, 0),
            Vector128.Create(0, 0, 1, 0),
            Vector128.Create(0, 0, 0, 1),
        ];

        Span<uint> accumulators = stackalloc uint[4];
        Span<int> chars = stackalloc int[4];

        while (true)
        {

            indexes = Vector128.Add(indexes, adder);
            chars[0] = ranges[0].ToExclusive <= indexes[0] ? 0 : Input[indexes[0]];
            chars[1] = ranges[1].ToExclusive <= indexes[1] ? 0 : Input[indexes[1]];
            chars[2] = ranges[2].ToExclusive <= indexes[2] ? 0 : Input[indexes[2]];
            chars[3] = ranges[3].ToExclusive <= indexes[3] ? 0 : Input[indexes[3]];

            if (chars[0] == ',') { acc += (int)accumulators[0]; accumulators[0] = 0; indexes = Vector128.Add(indexes, skippers[0]); chars[0] = Input[indexes[0]]; }
            if (chars[1] == ',') { acc += (int)accumulators[1]; accumulators[1] = 0; indexes = Vector128.Add(indexes, skippers[1]); chars[1] = Input[indexes[1]]; }
            if (chars[2] == ',') { acc += (int)accumulators[2]; accumulators[2] = 0; indexes = Vector128.Add(indexes, skippers[2]); chars[2] = Input[indexes[2]]; }
            if (chars[3] == ',') { acc += (int)accumulators[3]; accumulators[3] = 0; indexes = Vector128.Add(indexes, skippers[3]); chars[3] = Input[indexes[3]]; }


            var vec = SpanToVecUInt(chars);

            if (Vector128.EqualsAll(vec, Vector128<uint>.Zero)) break;

            var vecacc = SpanToVecUInt(accumulators);
            var vec2 = Vector128.Add(vec, vecacc);
            var vec3 = Vector128.Multiply(vec2, multiplier);

            accumulators[0] = vec3[0] % 256;
            accumulators[1] = vec3[1] % 256;
            accumulators[2] = vec3[2] % 256;
            accumulators[3] = vec3[3] % 256;
        }


        return acc;
    }


    readonly struct SmallRange2(ushort from, ushort to)
    {
        public readonly ushort FromInclusive = from;
        public readonly ushort ToExclusive = to;

		public override string ToString() => $"({FromInclusive} - {ToExclusive})";
    }


    [Benchmark]
    public unsafe int Part1Solution4()
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector256<ushort> CreateVec(Span<ushort> shorts) =>
			Vector256.Create(shorts[0], shorts[1], shorts[2], shorts[3], shorts[4], shorts[5], shorts[6], shorts[7], shorts[8], shorts[9], shorts[10], shorts[11], shorts[12], shorts[13], shorts[14], shorts[15]);

		// amount of shorts in 256 (im only accelerated for 256)
		const int ElCount = 16;
		int acc = 0;
        Span<SmallRange2> ranges = stackalloc SmallRange2[ElCount];

        int xmultiplier = (int)(Input.Length * (1f / ElCount));
        int xlast = 0, xlasti = 0;
        for (int i = 1; i < Input.Length; i++)
        {
            if (i - xlasti > xmultiplier && Input[i] == ',')
            {
                ranges[xlast++] = new((ushort)xlasti, (ushort)i);
                if (xlast == ElCount - 1) break;
                xlasti = i + 1;
                i += xmultiplier;
            }
        }

        ranges[ElCount - 1] = new SmallRange2(ranges[ElCount - 2].ToExclusive, (ushort)Input.Length);
        Vector256<ushort> indexes = Vector256.Create(
			ranges[0].FromInclusive, ranges[1].FromInclusive, ranges[2].FromInclusive, ranges[3].FromInclusive, ranges[4].FromInclusive, ranges[5].FromInclusive, ranges[6].FromInclusive, ranges[7].FromInclusive, 
			ranges[8].FromInclusive, ranges[9].FromInclusive, ranges[10].FromInclusive, ranges[11].FromInclusive, ranges[12].FromInclusive, ranges[13].FromInclusive, ranges[14].FromInclusive, ranges[15].FromInclusive);

        Vector256<ushort> multiplier = Vector256.Create<ushort>(17);

		Span<ushort> adder = stackalloc ushort[ElCount];
        Span<ushort> accumulators = stackalloc ushort[ElCount];
        Span<ushort> chars = stackalloc ushort[ElCount];

        while (true)
        {

            for (int i = 0; i < ElCount; i++)
            {
				if (indexes[i] == ranges[i].ToExclusive)
				{
                    // save and quit
					if (adder[i] > 0)
					{
						acc += accumulators[i];
						adder[i] = 0;
					}
                }
				else
				{
					chars[i] = Input[indexes[i]];
                    if (chars[i] == ',')
                    {
                        acc += accumulators[i];
                        accumulators[i] = 0;
                        chars[i] = (byte)Input[indexes[i] + 1];
                        adder[i] = 2;
                    }
                    else adder[i] = 1;
                }
            }

			var addervec = CreateVec(adder);
			if (Vector256.EqualsAll(addervec, Vector256<ushort>.Zero)) break;
            indexes = Vector256.Add(indexes, addervec);

            var charvec = CreateVec(chars);

            var accumulatorVec = CreateVec(accumulators);
            var addedVec = Vector256.Add(charvec, accumulatorVec);
            var multipliedVec = Vector256.Multiply(addedVec, multiplier);

            for (int i = 0; i < ElCount; i++)
			{
				accumulators[i] = (ushort)(multipliedVec[i] % 256);
			}
        }


        return acc;
    }


    [Benchmark]
    public unsafe int Part1Solution5()
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector256<ushort> CreateVec(Span<ushort> shorts) =>
            Vector256.Create(shorts[0], shorts[1], shorts[2], shorts[3], shorts[4], shorts[5], shorts[6], shorts[7], shorts[8], shorts[9], shorts[10], shorts[11], shorts[12], shorts[13], shorts[14], shorts[15]);

        // amount of shorts in 256 (im only accelerated for 256)
        const int ElCount = 16;
        int acc = 0;
        Span<SmallRange2> ranges = stackalloc SmallRange2[ElCount];

        int xmultiplier = (int)(Input.Length * (1f / ElCount));
        int xlast = 0, xlasti = 0;
        for (int i = 1; i < Input.Length; i++)
        {
            if (i - xlasti > xmultiplier && Input[i] == ',')
            {
                ranges[xlast++] = new((ushort)xlasti, (ushort)i);
                if (xlast == ElCount - 1) break;
                xlasti = i + 1;
                i += xmultiplier;
            }
        }

        ranges[ElCount - 1] = new SmallRange2(ranges[ElCount - 2].ToExclusive, (ushort)Input.Length);
        Vector256<ushort> indexes = Vector256.Create(
            ranges[0].FromInclusive, ranges[1].FromInclusive, ranges[2].FromInclusive, ranges[3].FromInclusive, ranges[4].FromInclusive, ranges[5].FromInclusive, ranges[6].FromInclusive, ranges[7].FromInclusive,
            ranges[8].FromInclusive, ranges[9].FromInclusive, ranges[10].FromInclusive, ranges[11].FromInclusive, ranges[12].FromInclusive, ranges[13].FromInclusive, ranges[14].FromInclusive, ranges[15].FromInclusive);

        Vector256<ushort> multiplier = Vector256.Create<ushort>(17);

        Span<ushort> adder = stackalloc ushort[ElCount];
        Span<ushort> accumulators = stackalloc ushort[ElCount];
        Span<ushort> chars = stackalloc ushort[ElCount];

        while (true)
        {
            #region unrolled loop
            if (indexes[0] == ranges[0].ToExclusive)
            {
                // save and quit
                if (adder[0] > 0)
                {
                    acc += accumulators[0];
                    adder[0] = 0;
                }
            }
            else
            {
                chars[0] = Input[indexes[0]];
                if (chars[0] == ',')
                {
                    acc += accumulators[0];
                    accumulators[0] = 0;
                    chars[0] = (byte)Input[indexes[0] + 1];
                    adder[0] = 2;
                }
                else adder[0] = 1;
            }
            if (indexes[1] == ranges[1].ToExclusive)
            {
                // save and quit
                if (adder[1] > 0)
                {
                    acc += accumulators[1];
                    adder[1] = 0;
                }
            }
            else
            {
                chars[1] = Input[indexes[1]];
                if (chars[1] == ',')
                {
                    acc += accumulators[1];
                    accumulators[1] = 0;
                    chars[1] = (byte)Input[indexes[1] + 1];
                    adder[1] = 2;
                }
                else adder[1] = 1;
            }
            if (indexes[2] == ranges[2].ToExclusive)
            {
                // save and quit
                if (adder[2] > 0)
                {
                    acc += accumulators[2];
                    adder[2] = 0;
                }
            }
            else
            {
                chars[2] = Input[indexes[2]];
                if (chars[2] == ',')
                {
                    acc += accumulators[2];
                    accumulators[2] = 0;
                    chars[2] = (byte)Input[indexes[2] + 1];
                    adder[2] = 2;
                }
                else adder[2] = 1;
            }
            if (indexes[3] == ranges[3].ToExclusive)
            {
                // save and quit
                if (adder[3] > 0)
                {
                    acc += accumulators[3];
                    adder[3] = 0;
                }
            }
            else
            {
                chars[3] = Input[indexes[3]];
                if (chars[3] == ',')
                {
                    acc += accumulators[3];
                    accumulators[3] = 0;
                    chars[3] = (byte)Input[indexes[3] + 1];
                    adder[3] = 2;
                }
                else adder[3] = 1;
            }
            if (indexes[4] == ranges[4].ToExclusive)
            {
                // save and quit
                if (adder[4] > 0)
                {
                    acc += accumulators[4];
                    adder[4] = 0;
                }
            }
            else
            {
                chars[4] = Input[indexes[4]];
                if (chars[4] == ',')
                {
                    acc += accumulators[4];
                    accumulators[4] = 0;
                    chars[4] = (byte)Input[indexes[4] + 1];
                    adder[4] = 2;
                }
                else adder[4] = 1;
            }
            if (indexes[5] == ranges[5].ToExclusive)
            {
                // save and quit
                if (adder[5] > 0)
                {
                    acc += accumulators[5];
                    adder[5] = 0;
                }
            }
            else
            {
                chars[5] = Input[indexes[5]];
                if (chars[5] == ',')
                {
                    acc += accumulators[5];
                    accumulators[5] = 0;
                    chars[5] = (byte)Input[indexes[5] + 1];
                    adder[5] = 2;
                }
                else adder[5] = 1;
            }
            if (indexes[6] == ranges[6].ToExclusive)
            {
                // save and quit
                if (adder[6] > 0)
                {
                    acc += accumulators[6];
                    adder[6] = 0;
                }
            }
            else
            {
                chars[6] = Input[indexes[6]];
                if (chars[6] == ',')
                {
                    acc += accumulators[6];
                    accumulators[6] = 0;
                    chars[6] = (byte)Input[indexes[6] + 1];
                    adder[6] = 2;
                }
                else adder[6] = 1;
            }
            if (indexes[7] == ranges[7].ToExclusive)
            {
                // save and quit
                if (adder[7] > 0)
                {
                    acc += accumulators[7];
                    adder[7] = 0;
                }
            }
            else
            {
                chars[7] = Input[indexes[7]];
                if (chars[7] == ',')
                {
                    acc += accumulators[7];
                    accumulators[7] = 0;
                    chars[7] = (byte)Input[indexes[7] + 1];
                    adder[7] = 2;
                }
                else adder[7] = 1;
            }
            if (indexes[8] == ranges[8].ToExclusive)
            {
                // save and quit
                if (adder[8] > 0)
                {
                    acc += accumulators[8];
                    adder[8] = 0;
                }
            }
            else
            {
                chars[8] = Input[indexes[8]];
                if (chars[8] == ',')
                {
                    acc += accumulators[8];
                    accumulators[8] = 0;
                    chars[8] = (byte)Input[indexes[8] + 1];
                    adder[8] = 2;
                }
                else adder[8] = 1;
            }
            if (indexes[9] == ranges[9].ToExclusive)
            {
                // save and quit
                if (adder[9] > 0)
                {
                    acc += accumulators[9];
                    adder[9] = 0;
                }
            }
            else
            {
                chars[9] = Input[indexes[9]];
                if (chars[9] == ',')
                {
                    acc += accumulators[9];
                    accumulators[9] = 0;
                    chars[9] = (byte)Input[indexes[9] + 1];
                    adder[9] = 2;
                }
                else adder[9] = 1;
            }
            if (indexes[10] == ranges[10].ToExclusive)
            {
                // save and quit
                if (adder[10] > 0)
                {
                    acc += accumulators[10];
                    adder[10] = 0;
                }
            }
            else
            {
                chars[10] = Input[indexes[10]];
                if (chars[10] == ',')
                {
                    acc += accumulators[10];
                    accumulators[10] = 0;
                    chars[10] = (byte)Input[indexes[10] + 1];
                    adder[10] = 2;
                }
                else adder[10] = 1;
            }
            if (indexes[11] == ranges[11].ToExclusive)
            {
                // save and quit
                if (adder[11] > 0)
                {
                    acc += accumulators[11];
                    adder[11] = 0;
                }
            }
            else
            {
                chars[11] = Input[indexes[11]];
                if (chars[11] == ',')
                {
                    acc += accumulators[11];
                    accumulators[11] = 0;
                    chars[11] = (byte)Input[indexes[11] + 1];
                    adder[11] = 2;
                }
                else adder[11] = 1;
            }
            if (indexes[12] == ranges[12].ToExclusive)
            {
                // save and quit
                if (adder[12] > 0)
                {
                    acc += accumulators[12];
                    adder[12] = 0;
                }
            }
            else
            {
                chars[12] = Input[indexes[12]];
                if (chars[12] == ',')
                {
                    acc += accumulators[12];
                    accumulators[12] = 0;
                    chars[12] = (byte)Input[indexes[12] + 1];
                    adder[12] = 2;
                }
                else adder[12] = 1;
            }
            if (indexes[13] == ranges[13].ToExclusive)
            {
                // save and quit
                if (adder[13] > 0)
                {
                    acc += accumulators[13];
                    adder[13] = 0;
                }
            }
            else
            {
                chars[13] = Input[indexes[13]];
                if (chars[13] == ',')
                {
                    acc += accumulators[13];
                    accumulators[13] = 0;
                    chars[13] = (byte)Input[indexes[13] + 1];
                    adder[13] = 2;
                }
                else adder[13] = 1;
            }
            if (indexes[14] == ranges[14].ToExclusive)
            {
                // save and quit
                if (adder[14] > 0)
                {
                    acc += accumulators[14];
                    adder[14] = 0;
                }
            }
            else
            {
                chars[14] = Input[indexes[14]];
                if (chars[14] == ',')
                {
                    acc += accumulators[14];
                    accumulators[14] = 0;
                    chars[14] = (byte)Input[indexes[14] + 1];
                    adder[14] = 2;
                }
                else adder[14] = 1;
            }
            if (indexes[15] == ranges[15].ToExclusive)
            {
                // save and quit
                if (adder[15] > 0)
                {
                    acc += accumulators[15];
                    adder[15] = 0;
                }
            }
            else
            {
                chars[15] = Input[indexes[15]];
                if (chars[15] == ',')
                {
                    acc += accumulators[15];
                    accumulators[15] = 0;
                    chars[15] = (byte)Input[indexes[15] + 1];
                    adder[15] = 2;
                }
                else adder[15] = 1;
            }
            #endregion

            var addervec = CreateVec(adder);
            if (Vector256.EqualsAll(addervec, Vector256<ushort>.Zero)) break;
            indexes = Vector256.Add(indexes, addervec);

            var charvec = CreateVec(chars);

            var accumulatorVec = CreateVec(accumulators);
            var addedVec = Vector256.Add(charvec, accumulatorVec);
            var multipliedVec = Vector256.Multiply(addedVec, multiplier);

            #region unrolled loop
            accumulators[0] = (ushort)(multipliedVec[0] % 256);
            accumulators[1] = (ushort)(multipliedVec[1] % 256);
            accumulators[2] = (ushort)(multipliedVec[2] % 256);
            accumulators[3] = (ushort)(multipliedVec[3] % 256);
            accumulators[4] = (ushort)(multipliedVec[4] % 256);
            accumulators[5] = (ushort)(multipliedVec[5] % 256);
            accumulators[6] = (ushort)(multipliedVec[6] % 256);
            accumulators[7] = (ushort)(multipliedVec[7] % 256);
            accumulators[8] = (ushort)(multipliedVec[8] % 256);
            accumulators[9] = (ushort)(multipliedVec[9] % 256);
            accumulators[10] = (ushort)(multipliedVec[10] % 256);
            accumulators[11] = (ushort)(multipliedVec[11] % 256);
            accumulators[12] = (ushort)(multipliedVec[12] % 256);
            accumulators[13] = (ushort)(multipliedVec[13] % 256);
            accumulators[14] = (ushort)(multipliedVec[14] % 256);
            accumulators[15] = (ushort)(multipliedVec[15] % 256);
            #endregion

        }


        return acc;
    }


    readonly struct SmallRange3(uint from, uint to)
    {
        public readonly uint FromInclusive = from;
        public readonly uint ToExclusive = to;

        public override string ToString() => $"({FromInclusive} - {ToExclusive})";
    }


    [Benchmark]
    public unsafe int Part1Solution6()
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe T FastIndex<T>(ref Span<T> span, int index) =>
            // what the hell is the right way to do this
            ((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)))[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe ref T FastIndexRef<T>(ref Span<T> span, int index) =>
            ref ((T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)))[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector256<uint> CreateVec(Span<uint> shorts) =>
            Vector256.Create(shorts[0], shorts[1], shorts[2], shorts[3], shorts[4], shorts[5], shorts[6], shorts[7]);

        // amount of uints in 256 (im only accelerated for 256)
        const int ElCount = 8;
        uint acc = 0; // doesnt need to be uint but im too lazy to the unroled loop
        Span<SmallRange3> ranges = stackalloc SmallRange3[ElCount];

        int xmultiplier = (int)(Input.Length * (1f / ElCount));
        int xlast = 0, xlasti = 0;
        for (int i = 1; i < Input.Length; i++)
        {
            if (i - xlasti > xmultiplier && Input[i] == ',')
            {
                //ranges[xlast++] = new((ushort)xlasti, (ushort)i);
                FastIndexRef(ref ranges, xlast++) = new((ushort)xlasti, (ushort)i);
                if (xlast == ElCount - 1) break;
                xlasti = i + 1;
                i += xmultiplier;
            }
        }

        ranges[ElCount - 1] = new SmallRange3(ranges[ElCount - 2].ToExclusive, (ushort)Input.Length);
        Vector256<uint> indexes = Vector256.Create(
            ranges[0].FromInclusive, ranges[1].FromInclusive, ranges[2].FromInclusive, ranges[3].FromInclusive, ranges[4].FromInclusive, ranges[5].FromInclusive, ranges[6].FromInclusive, ranges[7].FromInclusive);

        Vector256<uint> multiplier = Vector256.Create<uint>(17);

        Span<uint> adder = stackalloc uint[ElCount];
        Span<uint> accumulators = stackalloc uint[ElCount];
        Span<uint> chars = stackalloc uint[ElCount];

        while (true)
        {
            for (int i = 0; i < ElCount; i++)
            {
                if (indexes[i] == FastIndex(ref ranges, i).ToExclusive)
                {
                    // save and quit
                    if (FastIndex(ref adder, i) > 0)
                    {
                        acc += FastIndex(ref accumulators, i);
                        FastIndexRef(ref adder, i) = 0;
                    }
                }
                else
                {
                    FastIndexRef(ref chars, i) = Input[(int)indexes[i]];
                    if (FastIndex(ref chars, i) == ',')
                    {
                        acc += FastIndex(ref accumulators, i);
                        FastIndexRef(ref accumulators, i) = 0;
                        FastIndexRef(ref chars, i) = (byte)Input[(int)indexes[i] + 1];
                        FastIndexRef(ref adder, i) = 2;
                    }
                    else FastIndexRef(ref adder, i) = 1;
                }
            }

            var addervec = CreateVec(adder);
            if (Vector256.EqualsAll(addervec, Vector256<uint>.Zero)) break;
            indexes = Vector256.Add(indexes, addervec);

            var charvec = CreateVec(chars);

            var accumulatorVec = CreateVec(accumulators);
            var addedVec = Vector256.Add(charvec, accumulatorVec);
            var multipliedVec = Vector256.Multiply(addedVec, multiplier);

            for (int i = 0; i < ElCount; i++)
                FastIndexRef(ref accumulators, i) = (ushort)(multipliedVec[i] % 256);
        }


        return (int)acc;
    }


    class Lens(int position, int focal)
	{
		public int Position { get; set; } = position;
		public int Focal { get; set; } = focal;
    }

	//[Benchmark]
	public int Part2Solution1() 
	{
        static int GetHash(string s) => 
			s.ToCharArray().TakeWhile(c => c != '=' && c != '-').Aggregate(0, (cum, cur) => ((cum + cur) * 17) % 256);

		var boxes = Enumerable.Range(0, 256).Select(_ => new Dictionary<string, Lens>()).ToArray();


		foreach (string s in Input.Split(','))
		{
			int hash = GetHash(s);
			string label = s[..s.IndexOfAny(['=', '-'])];

			Console.Write($"label: '{label}', hash: '{hash}', result: ");
			if (s.Contains('-'))
			{
				
				if (boxes[hash].TryGetValue(label, out Lens? lens))
				{
                    Console.WriteLine($"Removing label '{label}'");
                    boxes[hash].Remove(label);
					Dictionary<int, int> myDictionary = [];

					foreach (var box in boxes[hash])
					{
						if (box.Value.Position > lens.Position)
							box.Value.Position -= 1;
					}
				}
				else
				{
					Console.WriteLine($"Did not remove label '{label}'");
				}
			}
			else
			{
				int focal = int.Parse([s[^1]]);
				if (boxes[hash].TryGetValue(label, out Lens? lens))
				{
					Console.WriteLine($"Replacing focal length of label '{label}'");
					lens.Focal = focal;
				}
				else
				{
					Console.WriteLine($"Adding label '{label}'");
					int pos = boxes[hash].Count > 0 ? boxes[hash].Max(kvp => kvp.Value.Position) : 0;
					boxes[hash][label] = new Lens(pos + 1, focal);
				}
			}

			Console.WriteLine($"{string.Join("\n", boxes.Index().Where(tup => tup.value.Count > 0).Select((tup) => $"{tup.index,3}: {string.Join(' ',  tup.value.Select(kvp => $"[{kvp.Key}: {kvp.Value.Focal}@{kvp.Value.Position}]"))}"))}\n");

		}

		int focusingPower = 0;
		for (int hash = 0; hash < boxes.Length; hash++)
		{
			int index = hash + 1;
			foreach (var kvp in boxes[hash])
			{
				int slot = kvp.Value.Position;
				int focal = kvp.Value.Focal;
				int weight = index * slot * focal;
				focusingPower += weight;
			}
		}
		return focusingPower;
    }

}

public static class Ex
{

	public static IEnumerable<(T value, int index)> Index<T>(this IEnumerable<T> @this) => @this.Select((a, b) => (a, b));
}
