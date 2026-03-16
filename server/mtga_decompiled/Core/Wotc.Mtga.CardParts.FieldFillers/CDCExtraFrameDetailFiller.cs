using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.CardParts.FieldFillers;

[RequireComponent(typeof(TMP_Text))]
public class CDCExtraFrameDetailFiller : CDCFillerBase
{
	private TMP_Text _label;

	private TMP_FontAsset _defaultFont;

	private Material _defaultFontMaterial;

	[SerializeField]
	private ExtraFrameDetailType _extraDetail;

	[Tooltip("Include to use detail as parameter, leave blank to print detail by itself")]
	[LocTerm]
	[SerializeField]
	private string _locTermForDetail;

	[SerializeField]
	private string _parameterName;

	[SerializeField]
	private bool _useALTFontAndColor = true;

	public override int RawFieldType => _extraDetail.GetHashCode();

	public override void Init(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardMaterialBuilder cardMaterialBuilder, IUnityObjectPool unityObjectPool, CardColorCaches cardColorCaches)
	{
		base.Init(cardDatabase, assetLookupSystem, cardMaterialBuilder, unityObjectPool, cardColorCaches);
		if (!_hasBeenInit)
		{
			_label = GetComponent<TMP_Text>();
			_defaultFont = _label.font;
			_defaultFontMaterial = _label.fontSharedMaterial;
			_hasBeenInit = true;
		}
	}

	public override void SetDestroyed(bool isDestroyed)
	{
		if ((bool)_label)
		{
			_label.gameObject.SetActive(!isDestroyed);
		}
		if (!isDestroyed)
		{
			Renderer[] componentsInChildren = _label.GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
	}

	public override void Cleanup()
	{
		if ((bool)_label)
		{
			_label.text = string.Empty;
			if (_label.fontSharedMaterial != _defaultFontMaterial)
			{
				Object.Destroy(_label.fontSharedMaterial);
			}
		}
		base.Cleanup();
	}

	public override void UpdateField(ICardDataAdapter model, CardHolderType cardHolderType, HashSet<CDCFillerBase> otherFillers, CDCViewMetadata viewMetadata, MtgGameState gameState, WorkflowBase currentInteraction)
	{
		string text = FindText(model, _cardDatabase.ClientLocProvider);
		if (_useALTFontAndColor)
		{
			text = string.Format(SetFontAndGetColorSettings(model, cardHolderType, viewMetadata, gameState).DefaultFormat, text);
		}
		_label.text = text;
	}

	private CardTextColorSettings SetFontAndGetColorSettings(ICardDataAdapter model, CardHolderType cardHolderType, CDCViewMetadata viewMetadata, MtgGameState gameState)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
		_assetLookupSystem.Blackboard.SetCdcViewMetadata(viewMetadata);
		_assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		_assetLookupSystem.Blackboard.GameState = gameState;
		_assetLookupSystem.Blackboard.Language = Languages.CurrentLanguage;
		_assetLookupSystem.Blackboard.Font = _defaultFont;
		_assetLookupSystem.Blackboard.Material = _defaultFontMaterial;
		bool canSwapMaterial;
		TMP_FontAsset tMP_FontAsset = FieldFillerUtils.FindFont(_assetLookupSystem, _assetTracker, out canSwapMaterial) ?? _defaultFont;
		_assetLookupSystem.Blackboard.Font = tMP_FontAsset;
		_assetLookupSystem.Blackboard.Material = tMP_FontAsset.material;
		_label.font = tMP_FontAsset;
		_label.fontSharedMaterial = tMP_FontAsset.material;
		if (canSwapMaterial)
		{
			Material material = FieldFillerUtils.FindMaterial(_assetLookupSystem, _assetTracker) ?? tMP_FontAsset.material;
			material = FieldFillerUtils.CheckForInvalidMaterial(model, material, tMP_FontAsset);
			_assetLookupSystem.Blackboard.Material = material;
			_label.fontSharedMaterial = material;
		}
		CardTextColorSettings result = FieldFillerUtils.FindColor(_assetLookupSystem, _cardColorCaches) ?? CardTextColorSettings.DEFAULT;
		_assetLookupSystem.Blackboard.Clear();
		return result;
	}

	private string FindText(ICardDataAdapter model, IClientLocProvider clientLoc)
	{
		if (model.Printing.ExtraFrameDetails.TryGetValue(_extraDetail, out var value))
		{
			if (!string.IsNullOrEmpty(_locTermForDetail))
			{
				return clientLoc.GetLocalizedText(_locTermForDetail, (_parameterName, value));
			}
			return value;
		}
		return string.Empty;
	}
}
