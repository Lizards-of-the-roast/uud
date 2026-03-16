using System;
using System.Collections.Generic;
using WorkflowVisuals;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene;

public class HighlightManager : IHighlightManager, IHighlightProvider, IHighlightController, IDisposable
{
	private readonly Dictionary<uint, HighlightType> _highlightsById = new Dictionary<uint, HighlightType>(100);

	private readonly Dictionary<uint, HighlightType> _highlightsByManaId = new Dictionary<uint, HighlightType>(100);

	private readonly Dictionary<IEntityView, HighlightType> _highlightsByEntity = new Dictionary<IEntityView, HighlightType>(100);

	private readonly Dictionary<DuelScene_CDC, HighlightType> _browserHighlights = new Dictionary<DuelScene_CDC, HighlightType>(100);

	private readonly Dictionary<uint, HighlightType> _userHighlights = new Dictionary<uint, HighlightType>(100);

	private bool _forceHighlightsOff;

	private bool _ignoreWorkflowHighlightsInBrowser;

	private bool _dirty;

	public event Action HighlightsUpdated;

	public void SetDirty()
	{
		_dirty = true;
	}

	public void SetForceHighlightsOff(bool forceHighlightsOff)
	{
		_dirty |= _forceHighlightsOff != forceHighlightsOff;
		_forceHighlightsOff = forceHighlightsOff;
	}

	public void SetBrowserHighlights(Dictionary<DuelScene_CDC, HighlightType> highlights, bool ignoreWorkflowHighlights = true)
	{
		_dirty = true;
		_browserHighlights.Clear();
		if (highlights != null)
		{
			foreach (KeyValuePair<DuelScene_CDC, HighlightType> highlight in highlights)
			{
				_browserHighlights[highlight.Key] = highlight.Value;
			}
		}
		_ignoreWorkflowHighlightsInBrowser = ignoreWorkflowHighlights;
	}

	public void SetWorkflowHighlights(Highlights workflowHighlights)
	{
		if (_highlightsById.ContainSame(workflowHighlights.IdToHighlightType_Workflow) && _highlightsByManaId.ContainSame(workflowHighlights.ManaIdToHighlightType) && _userHighlights.ContainSame(workflowHighlights.IdToHighlightType_User) && _highlightsByEntity.ContainSame(workflowHighlights.EntityHighlights))
		{
			return;
		}
		_dirty = true;
		_highlightsById.Clear();
		_highlightsByManaId.Clear();
		_highlightsByEntity.Clear();
		_userHighlights.Clear();
		foreach (KeyValuePair<uint, HighlightType> item in workflowHighlights.IdToHighlightType_Workflow)
		{
			_highlightsById[item.Key] = item.Value;
		}
		foreach (KeyValuePair<uint, HighlightType> item2 in workflowHighlights.ManaIdToHighlightType)
		{
			_highlightsByManaId[item2.Key] = item2.Value;
		}
		foreach (KeyValuePair<uint, HighlightType> item3 in workflowHighlights.IdToHighlightType_User)
		{
			_userHighlights[item3.Key] = item3.Value;
		}
		foreach (KeyValuePair<IEntityView, HighlightType> entityHighlight in workflowHighlights.EntityHighlights)
		{
			_highlightsByEntity[entityHighlight.Key] = entityHighlight.Value;
		}
	}

	public void SetUserHighlights(Dictionary<uint, HighlightType> highlights)
	{
		if (_userHighlights.ContainSame(highlights))
		{
			return;
		}
		_dirty = true;
		_userHighlights.Clear();
		if (highlights == null)
		{
			return;
		}
		foreach (KeyValuePair<uint, HighlightType> highlight in highlights)
		{
			_userHighlights[highlight.Key] = highlight.Value;
		}
	}

	private HighlightType GetHighlightForCDC(DuelScene_CDC cdc)
	{
		if (cdc == null || _forceHighlightsOff)
		{
			return HighlightType.None;
		}
		if (_highlightsByEntity.TryGetValue(cdc, out var value))
		{
			return value;
		}
		if (_browserHighlights.Count > 0)
		{
			if (_browserHighlights.ContainsKey(cdc))
			{
				return _browserHighlights[cdc];
			}
			if (cdc.InstanceId == 0)
			{
				ICardHolder currentCardHolder = cdc.CurrentCardHolder;
				if (currentCardHolder != null && currentCardHolder.CardHolderType == CardHolderType.Command)
				{
					return HighlightType.None;
				}
			}
			if (_userHighlights.TryGetValue(cdc.InstanceId, out var value2))
			{
				return value2;
			}
			if (!_ignoreWorkflowHighlightsInBrowser)
			{
				return GetHighlightForId(cdc.InstanceId);
			}
			return HighlightType.None;
		}
		return GetHighlightForId(cdc.InstanceId);
	}

	private HighlightType GetHighlightForAvatar(DuelScene_AvatarView avatar)
	{
		if (avatar == null)
		{
			return HighlightType.None;
		}
		if (_highlightsByEntity.TryGetValue(avatar, out var value))
		{
			return value;
		}
		return GetHighlightForId(avatar.InstanceId);
	}

	public HighlightType GetHighlightForId(uint id)
	{
		HighlightType value2;
		if (_userHighlights.Count > 0)
		{
			if (_userHighlights.TryGetValue(id, out var value))
			{
				return value;
			}
		}
		else if (_highlightsById.Count > 0 && _highlightsById.TryGetValue(id, out value2))
		{
			return value2;
		}
		return HighlightType.None;
	}

	public void UpdateHighlights(IEnumerable<DuelScene_CDC> allCards, IEnumerable<DuelScene_AvatarView> allAvatars)
	{
		if (!_dirty)
		{
			return;
		}
		foreach (DuelScene_CDC allCard in allCards)
		{
			if (!(allCard == null))
			{
				allCard.UpdateHighlight(GetHighlightForCDC(allCard));
			}
		}
		foreach (DuelScene_AvatarView allAvatar in allAvatars)
		{
			if (!(allAvatar == null))
			{
				allAvatar.UpdateHighlight(GetHighlightForAvatar(allAvatar));
				allAvatar.HighlightMana(_highlightsByManaId);
			}
		}
		_dirty = false;
		this.HighlightsUpdated?.Invoke();
	}

	public void Dispose()
	{
		this.HighlightsUpdated = null;
		_highlightsById.Clear();
		_highlightsByManaId.Clear();
		_highlightsByEntity.Clear();
		_browserHighlights.Clear();
		_userHighlights.Clear();
	}
}
