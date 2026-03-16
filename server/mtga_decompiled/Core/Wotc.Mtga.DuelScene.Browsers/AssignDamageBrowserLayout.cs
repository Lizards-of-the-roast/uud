using System;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.Browsers;

public class AssignDamageBrowserLayout : ICardLayout, IDisposable
{
	private readonly Vector3 _attackerCenter;

	private readonly Vector3 _blockerCenter;

	private readonly Vector3 _attackQuarryCenter;

	private readonly CardLayout_ScrollableBrowser _scrollLayout;

	private DuelScene_CDC _attacker;

	private readonly List<DuelScene_CDC> _blockers = new List<DuelScene_CDC>();

	private DuelScene_CDC _attackQuarry;

	private readonly List<CardLayoutData> _results = new List<CardLayoutData>();

	public float ScrollValue
	{
		set
		{
			_scrollLayout.ScrollPosition = value;
		}
	}

	public int FrontCount => _scrollLayout.FrontCount;

	public AssignDamageBrowserLayout(Vector3 attackerCenter, Vector3 blockerCenter, Vector3 attackQuarryCenter, CardLayout_ScrollableBrowser scrollLayout)
	{
		_attackerCenter = attackerCenter;
		_blockerCenter = blockerCenter;
		_attackQuarryCenter = attackQuarryCenter;
		_scrollLayout = scrollLayout;
	}

	public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		if (AllCardsExistInLayout(allCardViews))
		{
			_results.Clear();
			if (_attacker != null)
			{
				allData.Add(new CardLayoutData
				{
					Card = _attacker,
					Position = _attackerCenter,
					Rotation = Quaternion.identity
				});
			}
			_scrollLayout.GenerateData(_blockers, ref allData, _blockerCenter, Quaternion.identity);
			if (_attackQuarry != null)
			{
				allData.Add(new CardLayoutData
				{
					Card = _attackQuarry,
					Position = _attackQuarryCenter,
					Rotation = Quaternion.identity
				});
			}
			allData.AddRange(_results);
		}
	}

	private bool AllCardsExistInLayout(IReadOnlyList<DuelScene_CDC> allCards)
	{
		if (allCards.Count > 0)
		{
			return allCards.TrueForAll(_attacker, _attackQuarry, _blockers, (DuelScene_CDC x, DuelScene_CDC attacker, DuelScene_CDC quarry, List<DuelScene_CDC> blockers) => x == attacker || x == quarry || blockers.Contains(x));
		}
		return false;
	}

	public void SetAttacker(DuelScene_CDC attacker)
	{
		_attacker = attacker;
	}

	public void SetBlockers(IEnumerable<DuelScene_CDC> blockers)
	{
		_blockers.Clear();
		_blockers.AddRange(blockers);
	}

	public void SetAttackQuarry(DuelScene_CDC attackQuarry)
	{
		_attackQuarry = attackQuarry;
	}

	public bool CardIsInView(DuelScene_CDC card)
	{
		if (_attackQuarry == card)
		{
			return true;
		}
		int num = _blockers.IndexOf(card);
		if (num == -1)
		{
			return false;
		}
		if (num >= _scrollLayout.PiledLeft)
		{
			return num < _blockers.Count - _scrollLayout.PiledRight;
		}
		return false;
	}

	public void ReorderBlocker(DuelScene_CDC blocker, int newIdx)
	{
		if (_blockers.Contains(blocker))
		{
			int num = _blockers.IndexOf(blocker);
			if (num != newIdx)
			{
				_blockers.RemoveAt(num);
				_blockers.Insert(newIdx, blocker);
			}
		}
	}

	public void SetFrontWidth(float frontWidth)
	{
		_scrollLayout.FrontWidth = frontWidth;
	}

	public int SortCompare(DuelScene_CDC x, DuelScene_CDC y)
	{
		if (!(x == _attacker))
		{
			if (!(x == _attackQuarry))
			{
				return _blockers.IndexOf(x).CompareTo(_blockers.IndexOf(y));
			}
			return 1;
		}
		return -1;
	}

	public void Dispose()
	{
		_attacker = null;
		_blockers.Clear();
		_attackQuarry = null;
		_results.Clear();
	}
}
