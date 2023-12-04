#!/bin/bash

DAY=$1

dotnet new console -n "day$1"
cd "day$1"
dotnet add package BenchmarkDotNet

echo "
using BenchmarkDotNet.Attributes;

#if DEBUG
new Benchmark().GlobalSetup();
#else
BenchmarkDotNet.Running.BenchmarkRunner.Run<Benchmark>();
#endif


[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
class Benchmark
{
	[GlobalSetup]
	public void GlobalSetup() { AssertResults(); }

	public void AssertResults() { }


	[Benchmark]
	public void Part1Solution1() { }

	[Benchmark]
	public void Part2Solution1() { }

}" > Program.cs

echo "dotnet build -c Release
dotnet /home/alex/code/aoc/23/day$DAY/bin/Release/net8.0/day$DAY.dll" > test.sh
