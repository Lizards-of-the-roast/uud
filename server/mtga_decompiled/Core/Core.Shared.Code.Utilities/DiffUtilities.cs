using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Shared.Code.Utilities;

public static class DiffUtilities
{
	public static IEnumerable<IDiffLine<T>> MyersDiff<T>(this IReadOnlyList<T> pre, IReadOnlyList<T> post, IEqualityComparer<T> comparer = null)
	{
		return pre.MyersDiffBacktrack(post, comparer).Reverse();
	}

	private static List<int[]> MyersDiffShortestTraces<T>(this IReadOnlyList<T> pre, IReadOnlyList<T> post, IEqualityComparer<T> comparer = null)
	{
		if (comparer == null)
		{
			comparer = EqualityComparer<T>.Default;
		}
		int count = pre.Count;
		int count2 = post.Count;
		int num = count;
		int num2 = count2;
		int num3 = num + num2;
		if (num3 == 0)
		{
			return new List<int[]>();
		}
		int[] array = new int[2 * num3 + 1];
		array[num3 + 1] = 0;
		List<int[]> list = new List<int[]>();
		bool flag = false;
		for (int i = 0; i <= num3; i++)
		{
			list.Add(array.ToArray());
			for (int j = num3 - i; j <= num3 + i; j += 2)
			{
				int num4 = j - num3;
				int num5 = ((num4 != -i && (num4 == i || array[j - 1] >= array[j + 1])) ? (array[j - 1] + 1) : array[j + 1]);
				int num6 = num5 - num4;
				while (num5 < num && num6 < num2 && comparer.Equals(pre[num5], post[num6]))
				{
					num5++;
					num6++;
				}
				array[j] = num5;
				if (num5 >= num && num6 >= num2)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		return list;
	}

	private static IEnumerable<IDiffLine<T>> MyersDiffBacktrack<T>(this IReadOnlyList<T> pre, IReadOnlyList<T> post, IEqualityComparer<T> comparer = null)
	{
		List<int[]> source = pre.MyersDiffShortestTraces(post, comparer);
		int count = pre.Count;
		int count2 = post.Count;
		int x = count;
		int y = count2;
		int max = x + y;
		foreach (var item2 in source.Select((int[] item2, int index) => (item: item2, index: index)).Reverse())
		{
			int[] item = item2.item;
			int d = item2.index;
			int num = x - y;
			int num2 = num + max;
			int num3 = ((num != -d && (num == d || item[num2 - 1] >= item[num2 + 1])) ? (num - 1) : (num + 1));
			int prevX = item[num3 + max];
			int prevY = prevX - num3;
			while (prevX < x && prevY < y)
			{
				x--;
				y--;
				yield return new NoChange<T>(x, y, pre[x]);
			}
			if (d > 0)
			{
				if (prevX < x)
				{
					yield return new Deletion<T>(prevX, pre[prevX]);
				}
				else
				{
					yield return new Addition<T>(prevY, post[prevY]);
				}
			}
			x = prevX;
			y = prevY;
		}
	}

	public static string PrintFullDiff<T>(this IEnumerable<IDiffLine<T>> diff)
	{
		IDiffLine<T>[] array = diff.ToArray();
		if (array.Length == 0)
		{
			return "";
		}
		int length = (from _ in array.OfType<IDiffLineWithPreLine>()
			select _.PreLine).LastOrDefault().ToString().Length;
		int length2 = (from _ in array.OfType<IDiffLineWithPostLine>()
			select _.PostLine).LastOrDefault().ToString().Length;
		int totalWidth = length;
		int totalWidth2 = length2;
		string text = "".PadRight(totalWidth);
		string text2 = "".PadRight(totalWidth2);
		string text3 = text;
		string text4 = text2;
		StringBuilder stringBuilder = new StringBuilder();
		IDiffLine<T>[] array2 = array;
		foreach (IDiffLine<T> diffLine in array2)
		{
			string text5 = ((diffLine is IDiffLineWithPreLine { PreLine: var preLine }) ? preLine.ToString().PadRight(totalWidth) : text3);
			string text6 = ((diffLine is IDiffLineWithPostLine { PostLine: var postLine }) ? postLine.ToString().PadRight(totalWidth2) : text4);
			stringBuilder.AppendLine($"{diffLine.Symbol} {text5} {text6} | {diffLine.Content}");
		}
		return stringBuilder.ToString();
	}
}
