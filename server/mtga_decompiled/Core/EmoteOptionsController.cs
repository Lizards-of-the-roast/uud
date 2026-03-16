using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Emotes;
using GreClient.Rules;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Arena.Enums.Cosmetic;

public class EmoteOptionsController : IEmoteController, IDisposable
{
	private enum EmoteTypeOrders
	{
		Unknown,
		PhrasesFirst,
		StickersFirst
	}

	private class TemporaryEmoteOverride
	{
		public float TimeRemaining { get; private set; }

		public bool Complete => TimeRemaining <= 0f;

		public TemporaryEmoteOverride(float timeRemaining)
		{
			TimeRemaining = timeRemaining;
		}

		public void DecrementTimer(float timeStep)
		{
			TimeRemaining -= timeStep;
		}
	}

	private EmoteOptionsView _emoteOptionsView;

	private AssetLookupSystem _assetLookupSystem;

	private List<EmoteData> _equippedEmoteOptions = new List<EmoteData>();

	private Dictionary<string, EmoteData> _emotesByIdMap = new Dictionary<string, EmoteData>();

	private Dictionary<string, EmoteData> _temporaryEmotes = new Dictionary<string, EmoteData>();

	private Dictionary<string, string> _baseToTempIds = new Dictionary<string, string>();

	private Dictionary<string, string> _tempToBaseIds = new Dictionary<string, string>();

	private Dictionary<string, Coroutine> _emoteOverrideCoroutines = new Dictionary<string, Coroutine>();

	private List<EmoteView> _visibleEmoteOptions = new List<EmoteView>();

	private Dictionary<string, EmoteView> _visibleEmotesByEmoteId = new Dictionary<string, EmoteView>();

	private List<EmoteOptionsPage> _emoteOptionsPages = new List<EmoteOptionsPage>();

	private int _emoteOptionsPageIndex;

	private bool _hovered;

	private readonly List<EmoteData> _emotePhraseCache = new List<EmoteData>();

	private readonly List<EmoteData> _emoteStickerCache = new List<EmoteData>();

	public bool IsOpen { get; private set; }

	public bool Hovered
	{
		get
		{
			return _hovered;
		}
		set
		{
			if (value != _hovered)
			{
				_hovered = value;
				_emoteOptionsView?.SetIsHovered(_hovered);
			}
		}
	}

	public bool Disposed { get; private set; }

	public event Action<EmoteData> OnEmoteOptionClicked;

	public EmoteOptionsController(IEnumerable<EmoteData> equippedEmoteOptions, AssetLookupSystem assetLookupSystem, EmoteOptionsView emoteOptionsView)
	{
		_equippedEmoteOptions = new List<EmoteData>(equippedEmoteOptions);
		foreach (EmoteData equippedEmoteOption in _equippedEmoteOptions)
		{
			_emotesByIdMap[equippedEmoteOption.Id] = equippedEmoteOption;
		}
		_assetLookupSystem = assetLookupSystem;
		_emoteOptionsView = emoteOptionsView;
		_emoteOptionsView.OnNextEmotePageClicked += NextEmotePageClicked;
		_emoteOptionsView.OnPreviousEmotePageClicked += PreviousEmotePageClicked;
	}

	public void Dispose()
	{
		OnDisposed(manuallyDisposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void OnDisposed(bool manuallyDisposing)
	{
		if (!Disposed && manuallyDisposing)
		{
			if ((bool)_emoteOptionsView)
			{
				_emoteOptionsView.OnNextEmotePageClicked -= NextEmotePageClicked;
				_emoteOptionsView.OnPreviousEmotePageClicked -= PreviousEmotePageClicked;
			}
			_emoteOptionsView = null;
			Disposed = true;
		}
	}

	~EmoteOptionsController()
	{
		OnDisposed(manuallyDisposing: false);
	}

	public void Open()
	{
		if (_equippedEmoteOptions.Count != 0)
		{
			IsOpen = true;
			_emoteOptionsView.Open();
			ClearVisibleEmoteOptions();
			SetUpEmoteOptionsPages(CurrentEmotes());
			UpdateVisibleEmoteOptions(_emoteOptionsPages[_emoteOptionsPageIndex]);
			_emoteOptionsView.SetPagesEnabled(_emoteOptionsPages.Count > 1);
		}
	}

	private IEnumerable<EmoteData> CurrentEmotes()
	{
		foreach (EmoteData equippedEmoteOption in _equippedEmoteOptions)
		{
			string value;
			string emoteId = (_baseToTempIds.TryGetValue(equippedEmoteOption.Id, out value) ? value : equippedEmoteOption.Id);
			if (TryGetEmoteData(emoteId, out var emoteData))
			{
				yield return emoteData;
			}
			else
			{
				yield return equippedEmoteOption;
			}
		}
	}

	public void Close()
	{
		IsOpen = false;
		_emoteOptionsView.Close();
	}

	public void Toggle()
	{
		if (IsOpen)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	public void OnGameStateUpdated(MtgGameState gameState)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.GameState = gameState;
		UpdateEmoteData();
	}

	public void OnEmoteRecieved(string emoteId)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.IncomingEmoteId = emoteId;
		UpdateEmoteData();
	}

	private void UpdateEmoteData()
	{
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<EmoteOverridePayload> loadedTree) || !_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<EmoteLocKey> loadedTree2) || !_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<EmoteSFX> loadedTree3))
		{
			return;
		}
		IBlackboard blackboard = _assetLookupSystem.Blackboard;
		for (int i = 0; i < _equippedEmoteOptions.Count; i++)
		{
			EmoteData emoteData = _equippedEmoteOptions[i];
			string text = (blackboard.EmoteId = emoteData.Id);
			EmoteOverridePayload payload = loadedTree.GetPayload(blackboard);
			if (payload == null)
			{
				continue;
			}
			string overrideEmoteId = payload.OverrideEmoteId;
			_assetLookupSystem.Blackboard.EmoteId = overrideEmoteId;
			EmoteLocKey payload2 = loadedTree2.GetPayload(blackboard);
			if (payload2 == null)
			{
				continue;
			}
			SfxData sfxData = loadedTree3.GetPayload(blackboard)?.SfxData;
			RemoveTemporaryOverride(text);
			EmoteData emoteData2 = new EmoteData(overrideEmoteId, emoteData.Entry);
			if (payload.IsTemporary)
			{
				_baseToTempIds[text] = overrideEmoteId;
				_tempToBaseIds[overrideEmoteId] = text;
				_temporaryEmotes[overrideEmoteId] = emoteData2;
				_emoteOverrideCoroutines[text] = _emoteOptionsView.StartCoroutine(EmoteOverrideCoroutine(text, emoteData2.Id, payload2.PreviewLocKey, sfxData, new TemporaryEmoteOverride(payload.OverrideDuration)));
				continue;
			}
			_equippedEmoteOptions[i] = emoteData2;
			_emotesByIdMap.Remove(text);
			_emotesByIdMap[overrideEmoteId] = emoteData2;
			if (_visibleEmotesByEmoteId.TryGetValue(text, out var value))
			{
				_visibleEmotesByEmoteId.Remove(text);
				_visibleEmotesByEmoteId[overrideEmoteId] = value;
				value.Init(overrideEmoteId, payload2.PreviewLocKey, sfxData);
			}
		}
	}

	private IEnumerator EmoteOverrideCoroutine(string emoteId, string overrideId, string overrideLocKey, SfxData overrideEmoteSfxData, TemporaryEmoteOverride emoteOverride)
	{
		if (_visibleEmotesByEmoteId.TryGetValue(emoteId, out var value))
		{
			_visibleEmotesByEmoteId.Remove(emoteId);
			_visibleEmotesByEmoteId[overrideId] = value;
			value.Init(overrideId, overrideLocKey, overrideEmoteSfxData);
		}
		while (emoteOverride.TimeRemaining > 0f)
		{
			emoteOverride.DecrementTimer(Time.deltaTime);
			yield return null;
		}
		RemoveTemporaryOverride(emoteId);
	}

	private void EmoteClicked(string emoteId)
	{
		if (TryGetEmoteData(emoteId, out var emoteData))
		{
			this.OnEmoteOptionClicked?.Invoke(emoteData);
			Close();
			RemoveTemporaryOverride(emoteId);
			EventSystem current = EventSystem.current;
			if ((bool)current)
			{
				current.SetSelectedGameObject(null);
			}
		}
	}

	private void RemoveTemporaryOverride(string emoteId)
	{
		string value;
		string text = (_baseToTempIds.ContainsKey(emoteId) ? emoteId : (_tempToBaseIds.TryGetValue(emoteId, out value) ? value : string.Empty));
		string value2;
		string key = (_tempToBaseIds.ContainsKey(emoteId) ? emoteId : (_baseToTempIds.TryGetValue(emoteId, out value2) ? value2 : string.Empty));
		_baseToTempIds.Remove(text);
		_tempToBaseIds.Remove(key);
		_temporaryEmotes.Remove(key);
		if (_emoteOverrideCoroutines.TryGetValue(text, out var value3))
		{
			_emoteOverrideCoroutines.Remove(text);
			_emoteOptionsView.StopCoroutine(value3);
		}
		if (_emotesByIdMap.ContainsKey(text) && _visibleEmotesByEmoteId.TryGetValue(key, out var value4))
		{
			_visibleEmotesByEmoteId.Remove(key);
			_visibleEmotesByEmoteId[text] = value4;
			value4.Init(text, EmoteUtils.GetPreviewLocKey(text, _assetLookupSystem), EmoteUtils.GetEmoteSfxData(text, _assetLookupSystem));
		}
	}

	private void NextEmotePageClicked()
	{
		ClearVisibleEmoteOptions();
		if (_emoteOptionsPageIndex >= _emoteOptionsPages.Count - 1)
		{
			_emoteOptionsPageIndex = 0;
		}
		else
		{
			_emoteOptionsPageIndex++;
		}
		SetUpEmoteOptionsPages(CurrentEmotes());
		UpdateVisibleEmoteOptions(_emoteOptionsPages[_emoteOptionsPageIndex]);
	}

	private void PreviousEmotePageClicked()
	{
		ClearVisibleEmoteOptions();
		if (_emoteOptionsPageIndex <= 0)
		{
			_emoteOptionsPageIndex = _emoteOptionsPages.Count - 1;
		}
		else
		{
			_emoteOptionsPageIndex--;
		}
		SetUpEmoteOptionsPages(CurrentEmotes());
		UpdateVisibleEmoteOptions(_emoteOptionsPages[_emoteOptionsPageIndex]);
	}

	private bool TryGetEmoteData(string emoteId, out EmoteData emoteData)
	{
		if (_temporaryEmotes.TryGetValue(emoteId, out emoteData))
		{
			return true;
		}
		return _emotesByIdMap.TryGetValue(emoteId, out emoteData);
	}

	private void SetUpEmoteOptionsPages(IEnumerable<EmoteData> emoteDatas)
	{
		_emotePhraseCache.Clear();
		_emoteStickerCache.Clear();
		_emoteOptionsPages.Clear();
		EmoteTypeOrders emoteTypeOrders = EmoteTypeOrders.Unknown;
		foreach (EmoteData emoteData in emoteDatas)
		{
			switch (emoteData.Entry.Page)
			{
			case EmotePage.Phrase:
				if (emoteTypeOrders == EmoteTypeOrders.Unknown)
				{
					emoteTypeOrders = EmoteTypeOrders.PhrasesFirst;
				}
				_emotePhraseCache.Add(emoteData);
				break;
			case EmotePage.Sticker:
				if (emoteTypeOrders == EmoteTypeOrders.Unknown)
				{
					emoteTypeOrders = EmoteTypeOrders.StickersFirst;
				}
				_emoteStickerCache.Add(emoteData);
				break;
			}
		}
		List<EmoteOptionsPage> collection = EmoteUtils.CreateEmoteOptionsPages(EmotePage.Phrase, _emotePhraseCache, _emoteOptionsView.PhraseEmoteViewParentCount);
		List<EmoteOptionsPage> collection2 = EmoteUtils.CreateEmoteOptionsPages(EmotePage.Sticker, _emoteStickerCache, _emoteOptionsView.StickerEmoteViewParentCount);
		if (emoteTypeOrders == EmoteTypeOrders.StickersFirst)
		{
			_emoteOptionsPages.AddRange(collection2);
			_emoteOptionsPages.AddRange(collection);
		}
		else
		{
			_emoteOptionsPages.AddRange(collection);
			_emoteOptionsPages.AddRange(collection2);
		}
		_emotePhraseCache.Clear();
		_emoteStickerCache.Clear();
	}

	private void UpdateVisibleEmoteOptions(EmoteOptionsPage emoteOptionsPage)
	{
		_visibleEmoteOptions = InstantiateEmoteOptions(emoteOptionsPage.EmoteData);
		_visibleEmotesByEmoteId.Clear();
		foreach (EmoteView visibleEmoteOption in _visibleEmoteOptions)
		{
			if ((bool)visibleEmoteOption)
			{
				_visibleEmotesByEmoteId[visibleEmoteOption.Id] = visibleEmoteOption;
			}
		}
		EmotePage pageType = emoteOptionsPage.PageType;
		if (pageType != EmotePage.Phrase && pageType == EmotePage.Sticker)
		{
			_emoteOptionsView.SetStickerEmoteViewSelections(_visibleEmoteOptions);
		}
		else
		{
			_emoteOptionsView.SetPhraseEmoteViewSelections(_visibleEmoteOptions);
		}
	}

	private List<EmoteView> InstantiateEmoteOptions(IReadOnlyCollection<EmoteData> emoteDatas)
	{
		List<EmoteView> list = new List<EmoteView>();
		foreach (EmoteData emoteData in emoteDatas)
		{
			EmoteView emoteView = InstantiateEmoteOption(emoteData);
			if (!(emoteView == null))
			{
				list.Add(emoteView);
			}
		}
		return list;
	}

	private EmoteView InstantiateEmoteOption(EmoteData emoteData)
	{
		EmoteView emoteView = EmoteUtils.InstantiateEmoteView(emoteData, _assetLookupSystem);
		if (emoteView != null)
		{
			emoteView.Init(emoteData.Id, EmoteUtils.GetPreviewLocKey(emoteData.Id, _assetLookupSystem), EmoteUtils.GetEmoteSfxData(emoteData.Id, _assetLookupSystem));
			emoteView.SetEquipped(!string.IsNullOrEmpty(emoteData.Id));
			emoteView.SetClickable(!string.IsNullOrEmpty(emoteData.Id));
			emoteView.SetHoverable(!string.IsNullOrEmpty(emoteData.Id));
			emoteView.OnClick += EmoteClicked;
			return emoteView;
		}
		return null;
	}

	private void ClearVisibleEmoteOptions()
	{
		while (_visibleEmoteOptions.Count > 0)
		{
			EmoteView emoteView = _visibleEmoteOptions[0];
			_visibleEmoteOptions.RemoveAt(0);
			emoteView.OnClick -= EmoteClicked;
			UnityEngine.Object.Destroy(emoteView.gameObject);
		}
		_visibleEmoteOptions.Clear();
		_visibleEmotesByEmoteId.Clear();
	}
}
