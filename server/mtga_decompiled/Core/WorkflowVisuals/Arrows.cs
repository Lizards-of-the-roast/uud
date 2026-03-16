using System.Collections.Generic;

namespace WorkflowVisuals;

public class Arrows
{
	public readonly struct LineData
	{
		public readonly uint SourceEntityId;

		public readonly uint TargetEntityId;

		public readonly uint Group;

		public readonly uint GroupCount;

		public LineData(uint sourceEntityId, uint targetEntityId = 0u, uint group = 0u, uint groupCount = 1u)
		{
			SourceEntityId = sourceEntityId;
			TargetEntityId = targetEntityId;
			Group = group;
			GroupCount = groupCount;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			LineData lineData = (LineData)obj;
			if (SourceEntityId == lineData.SourceEntityId && TargetEntityId == lineData.TargetEntityId && Group == lineData.Group)
			{
				return GroupCount == lineData.GroupCount;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(SourceEntityId ^ TargetEntityId ^ Group ^ GroupCount);
		}

		public static bool operator ==(LineData a, LineData b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(LineData a, LineData b)
		{
			return !a.Equals(b);
		}
	}

	private readonly HashSet<LineData> _lineDatas = new HashSet<LineData>();

	private readonly HashSet<LineData> _cardsToMouse = new HashSet<LineData>();

	private readonly HashSet<LineData> _suppressedLines = new HashSet<LineData>();

	public bool Exclusive;

	public IReadOnlyCollection<LineData> LineDatas => _lineDatas;

	public IReadOnlyCollection<LineData> CardsToMouse => _cardsToMouse;

	public IReadOnlyCollection<LineData> SuppressedLines => _suppressedLines;

	public void AddLine(LineData lineData)
	{
		_lineDatas.Add(lineData);
	}

	public void AddCtMLine(LineData lineData)
	{
		_cardsToMouse.Add(lineData);
	}

	public void AddSuppressedLine(LineData lineData)
	{
		_suppressedLines.Add(lineData);
	}

	public void ClearLines()
	{
		_lineDatas.Clear();
	}

	public void ClearCtMLines()
	{
		_cardsToMouse.Clear();
	}

	public void ClearSuppressedLines()
	{
		_suppressedLines.Clear();
	}

	public void Reset()
	{
		Exclusive = false;
		ClearLines();
		ClearCtMLines();
		ClearSuppressedLines();
	}

	public static Arrows GetDefault()
	{
		return new Arrows();
	}

	public static void Merge(Arrows lhs, Arrows rhs)
	{
		lhs._lineDatas.UnionWith(rhs._lineDatas);
		lhs._cardsToMouse.UnionWith(rhs._cardsToMouse);
		lhs._suppressedLines.UnionWith(rhs._suppressedLines);
	}
}
