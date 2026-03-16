using System;
using System.Collections.Generic;
using Pooling;

namespace Wotc.Mtga.DuelScene.Battlefield;

public class PageHandler
{
	private int? _indexedPageGroup;

	private readonly List<PageGroup> _pageGroups = new List<PageGroup>();

	private readonly IObjectPool _objectPool;

	public int CurrentPageGroupIndex => _indexedPageGroup.GetValueOrDefault();

	public PageGroup CurrentGroup => Get(CurrentPageGroupIndex);

	public int GroupCount => _pageGroups.Count;

	public int PagedOutLeftCardCount
	{
		get
		{
			int num = 0;
			foreach (PageGroup pageGroup in _pageGroups)
			{
				if (pageGroup.GroupIndex < CurrentPageGroupIndex)
				{
					num += pageGroup.PagedLeftCardCount;
					num += pageGroup.VisibleCardCount;
				}
			}
			return num + CurrentGroup.PagedLeftCardCount;
		}
	}

	public int PagedOutRightCardCount
	{
		get
		{
			int num = 0;
			foreach (PageGroup pageGroup in _pageGroups)
			{
				if (pageGroup.GroupIndex > CurrentPageGroupIndex)
				{
					num += pageGroup.PagedRightCardCount;
					num += pageGroup.VisibleCardCount;
				}
			}
			return num + CurrentGroup.PagedRightCardCount;
		}
	}

	public PageHandler(IObjectPool objectPool)
	{
		_objectPool = objectPool;
		PageGroup item = new PageGroup(_objectPool, 0);
		_pageGroups.Add(item);
	}

	public PageGroup Get(int index)
	{
		if (_pageGroups.Count > index)
		{
			return _pageGroups[index];
		}
		return _pageGroups[0];
	}

	public void SetupGroups(int pageGroups, bool clearPageTracking, int? startingGroup = null)
	{
		for (int i = 0; i < pageGroups; i++)
		{
			if (_pageGroups.Count <= i)
			{
				_pageGroups.Add(new PageGroup(_objectPool, i));
			}
			_pageGroups[i].Clear(clearPageTracking);
		}
		if (!_indexedPageGroup.HasValue)
		{
			_indexedPageGroup = startingGroup;
		}
		else if (clearPageTracking)
		{
			_indexedPageGroup = null;
		}
		if (_pageGroups.Count <= pageGroups)
		{
			return;
		}
		for (int num = _pageGroups.Count - pageGroups; num > 0; num--)
		{
			if (_pageGroups.Count > num)
			{
				_pageGroups[num].Clear(clearRowOffsets: true);
				_pageGroups.RemoveAt(num);
			}
		}
	}

	public void PageRight()
	{
		if (!CurrentGroup.TryPageRight() && _pageGroups.Count > 1)
		{
			_indexedPageGroup++;
			_indexedPageGroup = Math.Min(_indexedPageGroup.Value, _pageGroups.Count - 1);
		}
	}

	public void PageLeft()
	{
		if (!CurrentGroup.TryPageLeft() && _pageGroups.Count > 1)
		{
			_indexedPageGroup--;
			_indexedPageGroup = Math.Max(_indexedPageGroup.Value, 0);
		}
	}

	public bool IsCardVisible(DuelScene_CDC cardView)
	{
		return CurrentGroup.IsCardVisible(cardView);
	}

	public bool IsCardPagedLeft(DuelScene_CDC cardView)
	{
		foreach (PageGroup pageGroup in _pageGroups)
		{
			if (pageGroup.IsCardPagedLeft(cardView, CurrentPageGroupIndex))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCardPagedRight(DuelScene_CDC cardView)
	{
		foreach (PageGroup pageGroup in _pageGroups)
		{
			if (pageGroup.IsCardPagedRight(cardView, CurrentPageGroupIndex))
			{
				return true;
			}
		}
		return false;
	}
}
