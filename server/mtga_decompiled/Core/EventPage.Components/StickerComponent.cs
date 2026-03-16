using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class StickerComponent : EventComponent
{
	[SerializeField]
	private Transform _stickersGrid;

	[SerializeField]
	private Vector3 _stickerEmoteScale = new Vector3(1f, 1f, 1f);

	private AssetLookupSystem _assetLookupSystem;

	private IEmoteDataProvider _emoteDataProvider;

	private AssetLookupTree<EmoteViewPrefab> _emoteViewPrefabTree;

	private readonly Dictionary<string, EmoteView> _instantiatedEmotes = new Dictionary<string, EmoteView>();

	private void OnEnable()
	{
		foreach (KeyValuePair<string, EmoteView> instantiatedEmote in _instantiatedEmotes)
		{
			instantiatedEmote.Deconstruct(out var _, out var value);
			value.SetDisplayOnly(isDisplayOnly: true);
		}
	}

	public void SetStickers(List<string> stickerIds, IEmoteDataProvider emoteDataProvider, AssetLookupSystem assetLookupSystem)
	{
		_emoteDataProvider = emoteDataProvider;
		_assetLookupSystem = assetLookupSystem;
		foreach (string stickerId in stickerIds)
		{
			EmoteData emoteData = _emoteDataProvider.GetEmoteData(stickerId);
			EmoteView emoteView = _instantiateEmote(emoteData);
			_instantiatedEmotes.Add(emoteData.Id, emoteView);
			emoteView.Init(stickerId);
			emoteView.SetDisplayOnly(isDisplayOnly: true);
			emoteView.transform.SetParent(_stickersGrid);
			emoteView.transform.ZeroOut();
			emoteView.SetScale(_stickerEmoteScale);
		}
	}

	private EmoteView _instantiateEmote(EmoteData data)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.EmotePrefabData = new EmotePrefabData
		{
			Id = data.Id,
			Page = data.Entry.Page
		};
		_emoteViewPrefabTree = _assetLookupSystem.TreeLoader.LoadTree<EmoteViewPrefab>();
		EmoteView emoteView = AssetLoader.Instantiate<EmoteView>(_emoteViewPrefabTree.GetPayload(_assetLookupSystem.Blackboard).PrefabPath);
		emoteView.Init(data.Id, EmoteUtils.GetPreviewLocKey(data.Id, _assetLookupSystem));
		return emoteView;
	}
}
