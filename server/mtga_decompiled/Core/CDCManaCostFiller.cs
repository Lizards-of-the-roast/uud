using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using TMPro;
using UnityEngine;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.CardParts.FieldFillers;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions;

public class CDCManaCostFiller : CDCFillerBase
{
	public enum FieldType
	{
		ManaCost,
		SimpleManaCost
	}

	private const string MAT_KEYWORD_DIMMED_ON = "_USEDIMMED_ON";

	private const string MAT_KEYWORD_DIMMED_OFF = "_USEDIMMED_ON";

	[SerializeField]
	private FieldType _fieldType;

	private TMP_Text _label;

	[SerializeField]
	private Transform _manaHighlightRoot;

	[SerializeField]
	private GameObject _manaHighlightPrefab;

	private GameObject _manaHighlightInstance;

	private static Vector2 ANCHOR_OFFSET_MANA_COST_HAND = new Vector2(0f, 0.3f);

	private Renderer _labelRenderer;

	private Renderer _submeshRenderer;

	private const string DEFAULT_SPRITESHEET_FILENAME = "SpriteSheet_ManaIcons";

	private const string DEFAULT_TINT = "#FFFFFF";

	public float Width => _label.rectTransform.sizeDelta.x;

	public bool IsEmpty => string.IsNullOrEmpty(_label.text);

	public override int RawFieldType => (int)_fieldType;

	public override void Init(ICardDatabaseAdapter cardDatabase, AssetLookupSystem assetLookupSystem, CardMaterialBuilder cardMaterialBuilder, IUnityObjectPool unityObjectPool, CardColorCaches cardColorCaches)
	{
		base.Init(cardDatabase, assetLookupSystem, cardMaterialBuilder, unityObjectPool, cardColorCaches);
		if (!_hasBeenInit)
		{
			_label = GetComponent<TMP_Text>();
			_hasBeenInit = true;
			if (_manaHighlightRoot == null && _label != null)
			{
				_manaHighlightRoot = _label.transform;
			}
		}
		_labelRenderer = _label.GetComponent<Renderer>();
		TMP_SubMesh componentInChildren = base.gameObject.GetComponentInChildren<TMP_SubMesh>();
		if ((bool)componentInChildren)
		{
			_submeshRenderer = componentInChildren.GetComponent<Renderer>();
		}
	}

	public void SetRendererOrder(int sortingOrder)
	{
		_labelRenderer.sortingOrder = sortingOrder;
		if (!_submeshRenderer)
		{
			TMP_SubMesh componentInChildren = base.gameObject.GetComponentInChildren<TMP_SubMesh>();
			if ((bool)componentInChildren)
			{
				_submeshRenderer = componentInChildren.GetComponent<Renderer>();
				_submeshRenderer.sortingOrder = sortingOrder;
			}
		}
		else
		{
			_submeshRenderer.sortingOrder = sortingOrder;
		}
	}

	public override void UpdateField(ICardDataAdapter model, CardHolderType cardHolderType, HashSet<CDCFillerBase> otherFillers, CDCViewMetadata viewMetadata, MtgGameState gameState, WorkflowBase currentInteraction)
	{
		CardTextColorSettings colorSettings = FieldFillerUtils.FindColor(_assetLookupSystem, _cardColorCaches) ?? CardTextColorSettings.DEFAULT;
		bool highlightMana;
		string text = ManaCostFillerUtils.GetText(_fieldType, model, cardHolderType, gameState, currentInteraction, _cardDatabase, _assetLookupSystem, colorSettings, out highlightMana);
		float? num = null;
		string filename;
		string tint;
		switch (_fieldType)
		{
		case FieldType.ManaCost:
			num = 0f;
			if (cardHolderType == CardHolderType.Hand)
			{
				_label.rectTransform.anchoredPosition = ANCHOR_OFFSET_MANA_COST_HAND;
				num = 3.5f;
				_label.enableAutoSizing = false;
				_label.fontSizeMin = 4.25f;
				_label.fontSizeMax = 6.5f;
				_label.fontSize = 6.5f;
			}
			else
			{
				_label.rectTransform.anchoredPosition = Vector2.zero;
				num = 1.8f;
				_label.enableAutoSizing = false;
				_label.fontSizeMin = 2.5f;
				_label.fontSizeMax = 3f;
				_label.fontSize = 3f;
			}
			GetSpriteSheetFileNameAndTintColor(model, cardHolderType, gameState, _assetLookupSystem, out filename, out tint);
			_label.text = ManaUtilities.ConvertManaSymbols(text, filename, tint);
			_label.alpha = 1f;
			_label.enableAutoSizing = _label.preferredWidth > num;
			EnableHighlight(highlightMana);
			break;
		case FieldType.SimpleManaCost:
		{
			string text2 = text;
			if (!string.IsNullOrEmpty(text))
			{
				GetSpriteSheetFileNameAndTintColor(model, cardHolderType, gameState, _assetLookupSystem, out filename, out tint);
				text2 = ManaUtilities.ConvertManaSymbols(text, filename, tint);
			}
			_label.text = text2;
			break;
		}
		}
		if (viewMetadata.IsDimmed)
		{
			_label.material.EnableKeyword("_USEDIMMED_ON");
		}
		else
		{
			_label.material.DisableKeyword("_USEDIMMED_ON");
		}
		Vector2 sizeDelta = _label.rectTransform.sizeDelta;
		if (num.HasValue)
		{
			_label.enableAutoSizing = _label.preferredWidth > num.Value;
			sizeDelta.x = Mathf.Min(num.Value, _label.preferredWidth);
		}
		else if (sizeDelta.x != _label.preferredWidth)
		{
			sizeDelta.x = _label.preferredWidth;
		}
		_label.rectTransform.sizeDelta = sizeDelta;
	}

	private void EnableHighlight(bool enabled)
	{
		if (enabled)
		{
			if (_manaHighlightInstance == null)
			{
				_manaHighlightInstance = _unityObjectPool.PopObject(_manaHighlightPrefab);
				_manaHighlightInstance.transform.parent = _manaHighlightRoot.transform;
				_manaHighlightInstance.transform.localPosition = _manaHighlightPrefab.transform.localPosition;
				_manaHighlightInstance.transform.localRotation = _manaHighlightPrefab.transform.localRotation;
				_manaHighlightInstance.transform.localScale = _manaHighlightPrefab.transform.localScale;
			}
		}
		else if (_manaHighlightInstance != null)
		{
			_unityObjectPool.PushObject(_manaHighlightInstance);
			_manaHighlightInstance = null;
		}
	}

	private void GetSpriteSheetFileNameAndTintColor(ICardDataAdapter model, CardHolderType cardHolderType, MtgGameState gameState, AssetLookupSystem als, out string filename, out string tint)
	{
		filename = "SpriteSheet_ManaIcons";
		tint = "#FFFFFF";
		als.Blackboard.Clear();
		als.Blackboard.SetCardDataExtensive(model);
		als.Blackboard.CardHolderType = cardHolderType;
		als.Blackboard.ManaFillerType = _fieldType;
		als.Blackboard.GameState = gameState;
		if (als.TreeLoader.TryLoadTree(out AssetLookupTree<ManaSymbolSpriteSheet> loadedTree))
		{
			ManaSymbolSpriteSheet payload = loadedTree.GetPayload(als.Blackboard);
			if (payload != null)
			{
				TMP_SpriteAsset tMP_SpriteAsset = AssetLoader.AcquireAndTrackAsset(_assetTracker, "ManaSymbolSpriteSheet", payload.SpriteSheet);
				if ((object)tMP_SpriteAsset != null)
				{
					filename = tMP_SpriteAsset.name;
					tint = "#" + ColorUtility.ToHtmlStringRGB(payload.TintColor);
				}
			}
		}
		als.Blackboard.Clear();
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

	public override void Cleanup()
	{
		base.Cleanup();
		if ((bool)_label)
		{
			_label.text = string.Empty;
		}
	}
}
