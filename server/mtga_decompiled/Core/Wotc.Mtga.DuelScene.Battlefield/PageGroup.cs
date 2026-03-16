using System.Collections.Generic;
using Pooling;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.Battlefield;

public class PageGroup
{
	public readonly int GroupIndex;

	public List<int> IndexOffsetByRow = new List<int>();

	private readonly List<List<BattlefieldCardHolder.BattlefieldStack>> _visibleStacksByRow = new List<List<BattlefieldCardHolder.BattlefieldStack>>();

	private readonly List<List<BattlefieldCardHolder.BattlefieldStack>> _pagedLeftByRow = new List<List<BattlefieldCardHolder.BattlefieldStack>>();

	private readonly List<List<BattlefieldCardHolder.BattlefieldStack>> _pagedRightByRow = new List<List<BattlefieldCardHolder.BattlefieldStack>>();

	private readonly IObjectPool _objectPool;

	public int VisibleCardCount { get; private set; }

	public int PagedLeftCardCount { get; private set; }

	public int PagedRightCardCount { get; private set; }

	public PageGroup(IObjectPool pool, int index)
	{
		_objectPool = pool;
		GroupIndex = index;
	}

	public bool TryPageLeft()
	{
		if (PagedLeftCardCount == 0)
		{
			return false;
		}
		if (_pagedLeftByRow.Count == 0)
		{
			return false;
		}
		SetPageOffsets(-1, _pagedLeftByRow);
		return true;
	}

	public bool TryPageRight()
	{
		if (PagedRightCardCount == 0)
		{
			return false;
		}
		if (_pagedRightByRow.Count == 0)
		{
			return false;
		}
		SetPageOffsets(1, _pagedRightByRow);
		return true;
	}

	private void SetPageOffsets(int direction, List<List<BattlefieldCardHolder.BattlefieldStack>> pagedByRow)
	{
		for (int i = 0; i < IndexOffsetByRow.Count && _visibleStacksByRow.Count > i; i++)
		{
			int a = Mathf.Max(1, _visibleStacksByRow[i].Count);
			int count = pagedByRow[i].Count;
			int num = Mathf.Min(a, count);
			IndexOffsetByRow[i] += num * direction;
		}
	}

	public bool IsCardVisible(DuelScene_CDC cdc)
	{
		return IsContainedInStacksByRow(cdc, _visibleStacksByRow);
	}

	public bool IsCardPagedLeft(DuelScene_CDC cdc, int currentPageGroup)
	{
		if (GroupIndex == currentPageGroup)
		{
			return IsContainedInStacksByRow(cdc, _pagedLeftByRow);
		}
		if (GroupIndex < currentPageGroup)
		{
			if (!IsContainedInStacksByRow(cdc, _pagedLeftByRow))
			{
				return IsCardVisible(cdc);
			}
			return true;
		}
		return false;
	}

	public bool IsCardPagedRight(DuelScene_CDC cdc, int currentPageGroup)
	{
		if (GroupIndex == currentPageGroup)
		{
			return IsContainedInStacksByRow(cdc, _pagedRightByRow);
		}
		if (GroupIndex > currentPageGroup)
		{
			if (!IsContainedInStacksByRow(cdc, _pagedRightByRow))
			{
				return IsCardVisible(cdc);
			}
			return true;
		}
		return false;
	}

	private bool IsContainedInStacksByRow(DuelScene_CDC cardView, List<List<BattlefieldCardHolder.BattlefieldStack>> stacksByRow)
	{
		foreach (List<BattlefieldCardHolder.BattlefieldStack> item in stacksByRow)
		{
			foreach (BattlefieldCardHolder.BattlefieldStack item2 in item)
			{
				if (item2.AllCards.Contains(cardView))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddRow(int rowIdx)
	{
		if (IndexOffsetByRow.Count <= rowIdx)
		{
			IndexOffsetByRow.Add(0);
		}
		if (_visibleStacksByRow.Count <= rowIdx)
		{
			_visibleStacksByRow.Add(_objectPool.PopObject<List<BattlefieldCardHolder.BattlefieldStack>>());
			_pagedLeftByRow.Add(_objectPool.PopObject<List<BattlefieldCardHolder.BattlefieldStack>>());
			_pagedRightByRow.Add(_objectPool.PopObject<List<BattlefieldCardHolder.BattlefieldStack>>());
		}
	}

	public void AddToVisible(BattlefieldCardHolder.BattlefieldStack stack, int rowIdx)
	{
		_visibleStacksByRow[rowIdx].Add(stack);
		VisibleCardCount += stack.AllCards.Count;
	}

	public void AddToPageLeft(BattlefieldCardHolder.BattlefieldStack stack, int rowIdx)
	{
		_pagedLeftByRow[rowIdx].Add(stack);
		PagedLeftCardCount += stack.AllCards.Count;
	}

	public void AddToPageRight(BattlefieldCardHolder.BattlefieldStack stack, int rowIdx)
	{
		_pagedRightByRow[rowIdx].Add(stack);
		PagedRightCardCount += stack.AllCards.Count;
	}

	public void Clear(bool clearRowOffsets)
	{
		ClearStacksByRow(_visibleStacksByRow);
		ClearStacksByRow(_pagedLeftByRow);
		ClearStacksByRow(_pagedRightByRow);
		if (clearRowOffsets)
		{
			IndexOffsetByRow.Clear();
		}
		VisibleCardCount = 0;
		PagedLeftCardCount = 0;
		PagedRightCardCount = 0;
	}

	private void ClearStacksByRow(List<List<BattlefieldCardHolder.BattlefieldStack>> stacksByRow)
	{
		foreach (List<BattlefieldCardHolder.BattlefieldStack> item in stacksByRow)
		{
			item.Clear();
			_objectPool.PushObject(item, tryClear: false);
		}
		stacksByRow.Clear();
	}
}
