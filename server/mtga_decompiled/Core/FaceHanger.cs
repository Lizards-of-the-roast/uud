using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards.Parts.Textbox;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtgo.Gre.External.Messaging;

public class FaceHanger : HangerBase
{
	public enum FaceHangerArrowType
	{
		None,
		Directional,
		FrontBack
	}

	public readonly struct FaceHangerArrowData
	{
		public FaceHangerArrowType Type { get; }

		public bool Mirrored { get; }

		public FaceHangerArrowData(FaceHangerArrowType type, bool mirrored = false)
		{
			Type = type;
			Mirrored = mirrored;
		}
	}

	public enum HangerType
	{
		None,
		MorphReference,
		CopyReference,
		DFC,
		MDFC,
		NamedCard,
		RelatedObj_Dungeon,
		RelatedObj_DayNight,
		TokenReference,
		ConjureReference,
		Specialized,
		Meld,
		Prototype,
		RoleReference,
		RingTemptsYou
	}

	public class FaceCardInfo
	{
		public struct MetaData
		{
			public CollectionInfo collectionInfo;
		}

		public struct CollectionInfo
		{
			public uint min;

			public uint max;
		}

		public readonly string HeaderText;

		public readonly ICardDataAdapter CardData;

		public readonly FaceHangerArrowData ArrowData;

		public readonly HangerType HangerType;

		public readonly MetaData VisualData;

		private readonly IHighlightProvider _highlightProvider;

		public FaceCardInfo(ICardDataAdapter cardData, string headerText, FaceHangerArrowData arrowData, HangerType hangerType = HangerType.None)
		{
			CardData = cardData;
			HeaderText = headerText;
			ArrowData = arrowData;
			HangerType = hangerType;
		}

		public FaceCardInfo(ICardDataAdapter cardData, string headerText, FaceHangerArrowData arrowData, HangerType hangerType, IHighlightProvider highlightProvider)
			: this(cardData, headerText, arrowData, hangerType)
		{
			_highlightProvider = highlightProvider ?? new NullHighlightProvider();
		}

		public FaceCardInfo(ICardDataAdapter cardData, string headerText, FaceHangerArrowData arrowData, MetaData visualData)
			: this(cardData, headerText, arrowData)
		{
			VisualData = visualData;
		}

		public HighlightType Highlight()
		{
			if (_highlightProvider == null)
			{
				return HighlightType.None;
			}
			if (CardData == null)
			{
				return HighlightType.None;
			}
			return _highlightProvider.GetHighlightForId(CardData.InstanceId);
		}
	}

	[SerializeField]
	private GameObject _hangerRoot;

	[SerializeField]
	private TMP_Text _hangerTypeLabel;

	[SerializeField]
	private Transform _directionalArrow;

	[SerializeField]
	private Transform _frontBackArrow;

	[SerializeField]
	private Transform _arrowFlipRoot;

	[SerializeField]
	private Transform _cardAnchor;

	[SerializeField]
	private GameObject _faceCountRoot;

	[SerializeField]
	private TMP_Text _faceIndexLabel;

	[SerializeField]
	private TMP_Text _faceCountLabel;

	[SerializeField]
	private Transform _hangerItem;

	private IFaceInfoGenerator _faceInfoGenerator;

	private CardViewBuilder _cardViewBuilder;

	private readonly List<FaceCardInfo> _activeFaceInfos = new List<FaceCardInfo>();

	private FaceCardInfo _frontFaceInfo;

	private BASE_CDC _faceCard;

	private const float FACE_CYCLING_TIME = 3f;

	private int _faceIndex;

	private float _faceCyclingTimer;

	public bool ShouldFacesCycle;

	public Transform HangerItem => _hangerItem;

	public override bool IsDisplayedOnLeftSide
	{
		set
		{
			base.IsDisplayedOnLeftSide = value;
			_arrowFlipRoot.localEulerAngles = new Vector3(0f, 0f, value ? 180f : 0f);
			_directionalArrow.localRotation = Quaternion.Euler(value ? 180f : 0f, 0f, 0f);
			_frontBackArrow.localRotation = Quaternion.Euler(value ? 180f : 0f, 0f, 0f);
		}
	}

	public void Init(IFaceInfoGenerator faceInfoGenerator, CardViewBuilder cardViewBuilder)
	{
		_faceInfoGenerator = faceInfoGenerator;
		_cardViewBuilder = cardViewBuilder;
		base.gameObject.UpdateActive(active: false);
		_faceCard = cardViewBuilder.CreateMetaCdc(CardDataExtensions.CreateBlank());
		_faceCard.transform.SetParent(_cardAnchor);
		_faceCard.transform.ZeroOut();
		_faceCard.gameObject.SetLayer(_hangerRoot.layer);
		_faceCard.Collider.enabled = false;
		_faceCard.ModelOverride = new ModelOverride(null, ZoneType.Library, null, null);
		_faceCard.HolderTypeOverride = CardHolderType.Library;
		_faceIndex = 0;
	}

	private void OnEnable()
	{
		if (_frontFaceInfo == null && _activeFaceInfos.Count >= 1)
		{
			_faceIndex = 0;
			SetFace(_activeFaceInfos[_faceIndex]);
		}
	}

	private void Update()
	{
		if (_activeFaceInfos.Count == 0)
		{
			return;
		}
		if (_frontFaceInfo == null)
		{
			_faceIndex = 0;
			SetFace(_activeFaceInfos[_faceIndex]);
		}
		if (ShouldFacesCycle && _activeFaceInfos.Count > 1)
		{
			_faceCyclingTimer += Time.deltaTime;
			if (_faceCyclingTimer >= 3f)
			{
				ShowNextFace();
				_faceCyclingTimer -= 3f;
			}
		}
	}

	public void ShowPrevFace()
	{
		if (_activeFaceInfos.Count > 1)
		{
			_faceIndex = (_faceIndex + _activeFaceInfos.Count - 1) % _activeFaceInfos.Count;
			FaceCardInfo face = _activeFaceInfos[_faceIndex];
			SetFace(face);
		}
	}

	public void ShowNextFace()
	{
		if (_activeFaceInfos.Count > 1)
		{
			_faceIndex = (_faceIndex + 1) % _activeFaceInfos.Count;
			FaceCardInfo face = _activeFaceInfos[_faceIndex];
			SetFace(face);
		}
	}

	public override void ActivateHanger(BASE_CDC cardView, ICardDataAdapter sourceModel, HangerSituation situation, bool delayShow = false)
	{
		ActivateHanger(cardView.Model, sourceModel, situation, delayShow);
	}

	public void ActivateHanger(ICardDataAdapter model, ICardDataAdapter sourceModel, HangerSituation situation, bool delayShow = false)
	{
		_activeFaceInfos.AddRange(_faceInfoGenerator.GenerateFaceCardInfo(model, sourceModel));
		base.Active = _activeFaceInfos.Count > 0;
		base.gameObject.UpdateActive(base.Active && !delayShow);
		ShouldFacesCycle = situation.ShouldCycleFaces;
	}

	public override void DeactivateHanger()
	{
		SetFace(null);
		_activeFaceInfos.Clear();
		base.gameObject.UpdateActive(active: false);
		base.Active = false;
	}

	public void SetColliderEnabled(bool enabled)
	{
		if (_faceCard != null)
		{
			_faceCard.Collider.enabled = enabled;
		}
	}

	public override bool HandleScroll(Vector2 delta)
	{
		if (!base.Active)
		{
			return false;
		}
		if (_frontFaceInfo == null)
		{
			return false;
		}
		if (!base.gameObject.activeSelf)
		{
			return false;
		}
		bool flag = false;
		foreach (CDCPart_TextBox_Rules item in _faceCard.FindAllParts<CDCPart_TextBox_Rules>(AnchorPointType.Invalid))
		{
			flag |= item.ScrollTextbox(delta);
		}
		return flag;
	}

	public override float GetHangerWidth()
	{
		if (_faceCard.ActiveScaffold == null)
		{
			return 0f;
		}
		return _faceCard.ActiveScaffold.GetColliderBounds.size.x * _faceCard.Root.lossyScale.x;
	}

	public void SetFace(FaceCardInfo faceInfo)
	{
		if (_frontFaceInfo == faceInfo)
		{
			return;
		}
		_frontFaceInfo = faceInfo;
		if (_frontFaceInfo != null)
		{
			_faceCard.SetModel(faceInfo.CardData);
			_faceCard.UpdateHighlight(faceInfo.Highlight());
			_faceCard.ImmediateUpdate();
			_faceCard.Collider.enabled = false;
			_hangerTypeLabel.SetText(faceInfo.HeaderText);
			_faceCountRoot.UpdateActive(_activeFaceInfos.Count > 1);
			_faceCountLabel.SetText(_activeFaceInfos.Count.ToString());
			_faceIndexLabel.SetText((_faceIndex + 1).ToString());
			_directionalArrow.gameObject.UpdateActive(faceInfo.ArrowData.Type == FaceHangerArrowType.Directional);
			_frontBackArrow.gameObject.UpdateActive(faceInfo.ArrowData.Type == FaceHangerArrowType.FrontBack);
			MirrorArrows(faceInfo.ArrowData.Mirrored, _directionalArrow, _frontBackArrow);
			if (_faceCard is Meta_CDC meta_CDC)
			{
				if (!_frontFaceInfo.VisualData.Equals(default(FaceCardInfo.MetaData)))
				{
					FaceCardInfo.CollectionInfo collectionInfo = _frontFaceInfo.VisualData.collectionInfo;
					bool active = collectionInfo.min == collectionInfo.max;
					meta_CDC.ShowCollectionInfo(active, (int)collectionInfo.min, (int)collectionInfo.max);
				}
				else
				{
					meta_CDC.ShowCollectionInfo(active: false);
				}
			}
			{
				foreach (CDCPart_Textbox_SuperBase item in _faceCard.FindAllParts<CDCPart_Textbox_SuperBase>(AnchorPointType.Invalid))
				{
					item.EnableTouchScroll();
				}
				return;
			}
		}
		_faceCard.SetModel(CardDataExtensions.CreateBlank());
		_faceCard.UpdateHighlight(HighlightType.None);
		_faceCard.IsDirty = true;
		_faceCard.Collider.enabled = false;
		_hangerTypeLabel.SetText(string.Empty);
		_faceCountRoot.UpdateActive(active: false);
		_directionalArrow.gameObject.UpdateActive(active: false);
		_frontBackArrow.gameObject.UpdateActive(active: false);
		base.gameObject.UpdateActive(active: false);
		base.Active = false;
	}

	private void MirrorArrows(bool mirrored, params Transform[] arrows)
	{
		foreach (Transform transform in arrows)
		{
			if (transform == null)
			{
				break;
			}
			Vector3 localScale = transform.localScale;
			localScale.x = ((!mirrored) ? 1 : (-1));
			transform.localScale = localScale;
		}
	}

	public void Cleanup()
	{
		if (_cardViewBuilder != null)
		{
			_cardViewBuilder.DestroyCDC(_faceCard);
		}
	}

	public static FaceHanger Create(AssetLookupSystem assetLookupSystem, Transform parent, int layer = 0)
	{
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<FaceHangerPrefab> loadedTree))
		{
			FaceHangerPrefab payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				FaceHanger faceHanger = AssetLoader.Instantiate<FaceHanger>(payload.PrefabPath, parent);
				faceHanger.gameObject.SetLayer(layer);
				return faceHanger;
			}
		}
		return null;
	}
}
