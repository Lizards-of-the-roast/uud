using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

public class CDCPart_Highlights : CDCPart
{
	private const float SECONDS_TO_SMOOTH_HOVER = 0.25f;

	private const float SECONDS_TO_SMOOTH_HOVER_HAND = 1f;

	private string _activeHighlightAssetPath = string.Empty;

	private GameObject _activeHighlight;

	private string _activeHoverHighlightAssetPath = string.Empty;

	private GameObject _activeHoverHighlight;

	private float _secondsUntilUnhover;

	private HighlightType _currentHighlight;

	private bool _opponentHighlight;

	public HighlightType CurrentHighlight
	{
		get
		{
			return _currentHighlight;
		}
		set
		{
			if (_currentHighlight != value || _activeHighlight == null)
			{
				_currentHighlight = value;
				UpdateHighlightBasedOnCurrentState(_currentHighlight, ref _activeHighlight, ref _activeHighlightAssetPath);
			}
			else if (_currentHighlight == HighlightType.AutoPay)
			{
				base.gameObject.SetActive(value: false);
				base.gameObject.SetActive(value: true);
			}
		}
	}

	public bool OpponentHighlight
	{
		get
		{
			return _opponentHighlight;
		}
		set
		{
			bool flag = value && !_cachedDestroyed;
			if (_opponentHighlight != flag)
			{
				_opponentHighlight = flag;
				if (_opponentHighlight)
				{
					_secondsUntilUnhover = -1f;
					UpdateHighlightBasedOnCurrentState(HighlightType.OpponentHover, ref _activeHoverHighlight, ref _activeHoverHighlightAssetPath);
				}
				else if (_secondsUntilUnhover <= 0f)
				{
					_secondsUntilUnhover = ((_cachedCardHolderType == CardHolderType.Hand) ? 1f : 0.25f);
				}
			}
		}
	}

	private void Update()
	{
		if (_secondsUntilUnhover > 0f)
		{
			_secondsUntilUnhover -= Time.deltaTime;
			if (_secondsUntilUnhover <= 0f)
			{
				UpdateHighlightBasedOnCurrentState(HighlightType.None, ref _activeHoverHighlight, ref _activeHoverHighlightAssetPath);
			}
		}
	}

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		bool flag = _opponentHighlight || _secondsUntilUnhover > 0f;
		UpdateHighlightBasedOnCurrentState(flag ? HighlightType.OpponentHover : HighlightType.None, ref _activeHoverHighlight, ref _activeHoverHighlightAssetPath);
		UpdateHighlightBasedOnCurrentState(_currentHighlight, ref _activeHighlight, ref _activeHighlightAssetPath);
	}

	protected override void HandleDestructionInternal()
	{
		if (_cachedDestroyed)
		{
			CurrentHighlight = HighlightType.None;
			OpponentHighlight = false;
		}
		base.HandleDestructionInternal();
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		CurrentHighlight = HighlightType.None;
		OpponentHighlight = false;
	}

	private void UpdateHighlightBasedOnCurrentState(HighlightType targetHighlightType, ref GameObject currentHighlightInstance, ref string currentHighlightAssetPath)
	{
		string text = null;
		OffsetData offsetData = null;
		if (targetHighlightType != HighlightType.None && _assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Highlight> loadedTree))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(_cachedModel);
			_assetLookupSystem.Blackboard.CardHolderType = _cachedCardHolderType;
			_assetLookupSystem.Blackboard.InDuelScene = !_cachedViewMetadata.IsMeta;
			_assetLookupSystem.Blackboard.HighlightType = targetHighlightType;
			if (_cachedModel.ObjectType == GameObjectType.Ability)
			{
				_assetLookupSystem.Blackboard.Ability = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(_cachedModel.GrpId);
			}
			Highlight payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				text = payload.HighlightRef.RelativePath;
				offsetData = payload.OffsetData;
			}
			_assetLookupSystem.Blackboard.Clear();
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (!(currentHighlightInstance != null) || !(currentHighlightAssetPath == text))
			{
				if (currentHighlightInstance != null)
				{
					_unityObjectPool.PushObject(currentHighlightInstance);
					currentHighlightInstance = null;
					currentHighlightAssetPath = string.Empty;
				}
				currentHighlightAssetPath = text;
				currentHighlightInstance = _unityObjectPool.PopObject(text);
				currentHighlightInstance.transform.SetParent(base.transform);
				currentHighlightInstance.transform.localPosition = offsetData.PositionOffset;
				currentHighlightInstance.transform.localEulerAngles = offsetData.RotationOffset;
				currentHighlightInstance.transform.localScale = offsetData.ScaleMultiplier;
			}
		}
		else if (currentHighlightInstance != null)
		{
			_unityObjectPool.PushObject(currentHighlightInstance);
			currentHighlightInstance = null;
			currentHighlightAssetPath = string.Empty;
		}
	}
}
