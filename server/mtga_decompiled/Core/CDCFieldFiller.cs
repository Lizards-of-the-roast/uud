using System;
using System.Collections.Generic;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.CardParts.FieldFillers;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

[RequireComponent(typeof(TMP_Text))]
public class CDCFieldFiller : CDCFillerBase
{
	[Serializable]
	public struct LangSizeKvp
	{
		public string LangCode;

		public float MaxSize;
	}

	[SerializeField]
	private CDCFieldFillerFieldType _fieldType;

	[SerializeField]
	private bool tryParseHirigana;

	[SerializeField]
	private List<LangSizeKvp> _langMaxSizes = new List<LangSizeKvp>();

	[SerializeField]
	private uint _abilityId;

	[SerializeField]
	private bool _shrinkMarginsForManaCost = true;

	private TMP_Text _label;

	private bool _isTextTruncated;

	private TMP_FontAsset _defaultFont;

	private Material _defaultFontMaterial;

	private Vector4 _defaultMargin = Vector4.zero;

	private float _defaultMaxSize;

	public override int RawFieldType => (int)_fieldType;

	public override void Init(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardMaterialBuilder cardMaterialBuilder, IUnityObjectPool unityObjectPool, CardColorCaches cardColorCaches)
	{
		base.Init(cardDatabase, assetLookupSystem, cardMaterialBuilder, unityObjectPool, cardColorCaches);
		if (!_hasBeenInit)
		{
			_label = GetComponent<TMP_Text>();
			_hasBeenInit = true;
			_defaultFont = _label.font;
			_defaultFontMaterial = _label.fontSharedMaterial;
			_defaultMargin = _label.margin;
			_defaultMaxSize = (_label.enableAutoSizing ? _label.fontSizeMax : _label.fontSize);
		}
	}

	public bool IsTextTruncated()
	{
		return _isTextTruncated;
	}

	public override void SetDestroyed(bool isDestroyed)
	{
		if (!_label)
		{
			return;
		}
		_label.gameObject.SetActive(!isDestroyed);
		if (!isDestroyed)
		{
			Renderer[] componentsInChildren = _label.GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
	}

	public override void UpdateField(ICardDataAdapter model, CardHolderType cardHolderType, HashSet<CDCFillerBase> otherFillers, CDCViewMetadata viewMetadata, MtgGameState gameState, WorkflowBase currentInteraction)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
		_assetLookupSystem.Blackboard.SetCdcViewMetadata(viewMetadata);
		_assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		_assetLookupSystem.Blackboard.FieldFillerType = _fieldType;
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
		CardTextColorSettings colorSettings = FieldFillerUtils.FindColor(_assetLookupSystem, _cardColorCaches) ?? CardTextColorSettings.DEFAULT;
		_assetLookupSystem.Blackboard.Clear();
		MouseOverType mouseOverType = (viewMetadata.IsMouseOver ? MouseOverType.MouseOver : MouseOverType.None);
		bool canShowFrameLanguage = FrameLanguageUtilities.CanShowFrameLanguage(model, gameState, mouseOverType, cardHolderType, viewMetadata.IsHoverCopy, _fieldType, Languages.CurrentLanguage);
		ShrinkRightMarginBasedOnManaFillerWidth(otherFillers, cardHolderType, model.Subtypes);
		bool determineIfTruncated;
		string text = FieldFillerUtils.FindText(model, gameState, _cardDatabase, _fieldType, cardHolderType, viewMetadata.IsMouseOver, colorSettings, canShowFrameLanguage, _assetLookupSystem, tryParseHirigana, _abilityId, out determineIfTruncated);
		SetLabelText(text, determineIfTruncated);
	}

	private void SetLabelText(string text, bool determineIfTruncated)
	{
		if (_label.enableAutoSizing)
		{
			_label.fontSizeMax = _defaultMaxSize;
		}
		else
		{
			_label.fontSize = _defaultMaxSize;
		}
		foreach (LangSizeKvp langMaxSize in _langMaxSizes)
		{
			if (string.Equals(langMaxSize.LangCode, Languages.CurrentLanguage))
			{
				if (_label.enableAutoSizing)
				{
					_label.fontSizeMax = langMaxSize.MaxSize;
				}
				else
				{
					_label.fontSize = langMaxSize.MaxSize;
				}
				break;
			}
		}
		if (_label.text != text)
		{
			if (determineIfTruncated)
			{
				_label.text = string.Empty;
			}
			_label.text = text;
			_label.alpha = 1f;
			if (determineIfTruncated)
			{
				_label.ForceMeshUpdate(ignoreActiveState: true);
			}
		}
		_isTextTruncated = determineIfTruncated && _label.isTextTruncated;
		LayoutRebuilder.MarkLayoutForRebuild(_label.rectTransform);
	}

	private void ShrinkRightMarginBasedOnManaFillerWidth(HashSet<CDCFillerBase> otherFillers, CardHolderType cardHolderType, IReadOnlyList<SubType> subTypes)
	{
		bool flag = _shrinkMarginsForManaCost;
		if (_fieldType == CDCFieldFillerFieldType.LinkedMDFCManaAbility)
		{
			flag = false;
		}
		else if (cardHolderType == CardHolderType.Hand)
		{
			if (_fieldType == CDCFieldFillerFieldType.Title || _fieldType == CDCFieldFillerFieldType.AltTitle)
			{
				flag = false;
			}
			if (_fieldType == CDCFieldFillerFieldType.Title && subTypes.Contains(SubType.Adventure))
			{
				flag = true;
			}
		}
		float? num = null;
		if (!flag)
		{
			return;
		}
		foreach (CDCFillerBase otherFiller in otherFillers)
		{
			if (otherFiller is CDCManaCostFiller { IsEmpty: false } cDCManaCostFiller)
			{
				num = cDCManaCostFiller.Width;
				break;
			}
			if (otherFiller is CDCFieldFiller { _fieldType: CDCFieldFillerFieldType.LinkedMDFCManaAbility } cDCFieldFiller && !string.IsNullOrEmpty(cDCFieldFiller._label.text))
			{
				num = cDCFieldFiller._label.rectTransform.sizeDelta.x;
				break;
			}
		}
		if (!num.HasValue)
		{
			_label.margin = _defaultMargin;
		}
		else
		{
			_label.margin = new Vector4(0f, 0f, num.Value, 0f);
		}
	}

	public override void Cleanup()
	{
		if ((bool)_label)
		{
			_label.text = string.Empty;
			_label.font = _defaultFont;
			_label.fontSharedMaterial = _defaultFontMaterial;
		}
		base.Cleanup();
	}
}
