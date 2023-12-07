
using System.Security.Cryptography.X509Certificates;
using Microsoft.Diagnostics.Runtime.Utilities;

namespace Day7;


enum Strength
{
    High,
    OnePair,
    TwoPair,
    ThreeKind,
    FullHouse,
    FourKind,
    FiveKind
}


static class Helpers
{

    public static readonly char[] Part1Labels = ['2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A'];
    public static readonly char[] Part2Labels = ['J', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'Q', 'K', 'A'];


	public static bool CardsAreEqual(Card first, Card second, bool useJoker) =>
		useJoker
			? first == second || first.IsJoker || second.IsJoker
			: first == second;


	public static int CompareCards(Card first, Card second, char[] order, bool useJoker) =>
		CardsAreEqual(first, second, useJoker)
			? 0
			: order.IndexOf(first.Label).CompareTo(order.IndexOf(second.Label));


    public static T Self<T>(T self) => self;

	public static bool Negate(bool value) => !value;

    public static int IndexOf<T>(this T[] @this, T value) where T : IComparable<T>
	{
		for (int i = 0; i < @this.Length; i++)
			if (@this[i].CompareTo(value) == 0) return i;
		return -1;
	}


	public static TResult Pipe<T, TResult>(this T @this, Func<T, TResult> func) => func(@this);


	public static IEnumerable<T> Log<T> (this IEnumerable<T> sequence, Func<T, string> formatter)
	{
		foreach (T element in sequence)
			Console.WriteLine(formatter(element));

		return sequence;
	}

    public static IEnumerable<T> Log<T>(this IEnumerable<T> sequence)
    {
        foreach (T element in sequence)
            Console.WriteLine(element?.ToString());

        return sequence;
    }
}