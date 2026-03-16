using System;
using System.Collections.Generic;
using Wizards.Mtga.FrontDoorModels;

namespace Wizards.Mtga.PlayBlade;

public class EventBladeData
{
	private readonly IBladeModel _model;

	private List<BladeEventInfo> Events => _model.Events ?? new List<BladeEventInfo>();

	public List<BladeEventFilter> FiltersList => _model.EventFilters ?? new List<BladeEventFilter>();

	public EventBladeData(IBladeModel model)
	{
		_model = model;
	}

	public List<BladeEventInfo> GetFilteredEvents(BladeEventFilter filter)
	{
		return filter.FilterType switch
		{
			EventFilterType.Tag => FilterEventsByTags(Events, filter.FilterCriteria), 
			EventFilterType.InProgress => FilterInProgressEvents(Events), 
			EventFilterType.Constructed => FilterConstructedEvents(Events), 
			EventFilterType.Limited => FilterLimitedEvents(Events), 
			EventFilterType.Format => FilterEventsByFormat(Events, filter.FilterCriteria), 
			EventFilterType.New => FilterEventsByAge(Events, TimeSpan.FromDays(14.0)), 
			EventFilterType.All => Events, 
			_ => new List<BladeEventInfo>(), 
		};
	}

	private List<BladeEventInfo> FilterInProgressEvents(List<BladeEventInfo> initialList)
	{
		List<BladeEventInfo> list = new List<BladeEventInfo>();
		foreach (BladeEventInfo initial in initialList)
		{
			if (initial.IsInProgress)
			{
				list.Add(initial);
			}
		}
		return list;
	}

	private List<BladeEventInfo> FilterEventsByAge(List<BladeEventInfo> initialList, TimeSpan maxAge)
	{
		DateTime utcNow = DateTime.UtcNow;
		List<BladeEventInfo> list = new List<BladeEventInfo>();
		foreach (BladeEventInfo initial in initialList)
		{
			if (utcNow < initial.StartTime || (utcNow >= initial.StartTime && utcNow.Subtract(initial.StartTime) < maxAge))
			{
				list.Add(initial);
			}
		}
		return list;
	}

	private List<BladeEventInfo> FilterLimitedEvents(List<BladeEventInfo> initialList)
	{
		List<BladeEventInfo> list = new List<BladeEventInfo>();
		foreach (BladeEventInfo initial in initialList)
		{
			if (initial.IsLimited)
			{
				list.Add(initial);
			}
		}
		return list;
	}

	private List<BladeEventInfo> FilterConstructedEvents(List<BladeEventInfo> initialList)
	{
		List<BladeEventInfo> list = new List<BladeEventInfo>();
		foreach (BladeEventInfo initial in initialList)
		{
			DeckFormat format = initial.Format;
			if (format != null && format.FormatType == MDNEFormatType.Constructed)
			{
				list.Add(initial);
			}
		}
		return list;
	}

	private List<BladeEventInfo> FilterEventsByFormat(List<BladeEventInfo> initialList, string formatName)
	{
		List<BladeEventInfo> list = new List<BladeEventInfo>();
		foreach (BladeEventInfo initial in initialList)
		{
			if (initial.Format?.FormatName == formatName)
			{
				list.Add(initial);
			}
		}
		return list;
	}

	private List<BladeEventInfo> FilterEventsByTags(List<BladeEventInfo> initialList, string tag)
	{
		List<BladeEventInfo> list = new List<BladeEventInfo>();
		foreach (BladeEventInfo initial in initialList)
		{
			if (initial.DynamicFilterTagIds != null && initial.DynamicFilterTagIds.Contains(tag))
			{
				list.Add(initial);
			}
		}
		return list;
	}
}
