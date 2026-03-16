using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using GreClient.CardData;
using Pooling;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Loc;

public class StaticHangerController : MonoBehaviour
{
	private FaceHanger _faceHanger;

	private List<FaceHanger> _faceHangers = new List<FaceHanger>();

	[SerializeField]
	private Transform _faceHangerParent;

	[SerializeField]
	private Transform _faceHangerLayoutElement;

	[SerializeField]
	private AbilityHangerBase _abilityHanger;

	[SerializeField]
	private Transform _abilityHangerLayoutElement;

	[SerializeField]
	private RectTransform _abilityViewTransformSource;

	[SerializeField]
	private float _previewScrollSpeed = 2f;

	[Header("Static Hover Card Elements")]
	[SerializeField]
	private Transform _abilityHangerParentTemplate;

	[SerializeField]
	private Transform _faceHangerParentTemplate;

	[SerializeField]
	private Transform _faceHangerScrollViewContent;

	[SerializeField]
	private Image _faceHangerScrollViewPort;

	[SerializeField]
	private Transform _faceHangerScrollView;

	[SerializeField]
	private int _faceHangerScrollThreshold = 2;

	[SerializeField]
	private ScrollRect _faceHangerScrollRect;

	private GameManager _gameManager;

	private bool initialized;

	private const float ACTIVATION_TIME_DEFAULT = 0f;

	private const float ACTIVATION_TIME_DELAY = 0.5f;

	private float _activationTime = 1f;

	private float _activationTimer;

	private IFaceInfoGenerator _faceInfoGenerator;

	private CardViewBuilder _cardViewBuilder;

	private ICardDatabaseAdapter _cardDatabase;

	private IUnityObjectPool _unityObjectPool;

	private IObjectPool _genericObjectPool;

	private IClientLocProvider _locManager;

	private MatchManager _matchManager;

	private int _layer;

	private string _faceHangerPrefabRelativePath;

	private AbilityHangerBase _abilityHangerTemplate;

	private AbilityHangerBase _scrollingAbilityHanger;

	private BASE_CDC _activeCard { get; set; }

	private HangerSituation _situation { get; set; }

	public bool ScrollViewActive { get; private set; }

	public bool ScrollViewEnabled { get; private set; }

	public void InitDuelscene(ICardDatabaseAdapter cardDatabase, CardViewBuilder cardViewBuilder, GameManager gameManager, IClientLocProvider locManager, IFaceInfoGenerator faceInfoGenerator, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, MatchManager matchManager, int layer = 0)
	{
		_gameManager = gameManager;
		_matchManager = matchManager;
		SharedInit(cardDatabase, cardViewBuilder, gameManager.AssetLookupSystem, locManager, faceInfoGenerator, unityObjectPool, genericObjectPool, layer);
		AbilityHanger abilityHanger = (AbilityHanger)_abilityHanger;
		_abilityHangerTemplate = Object.Instantiate(abilityHanger, _abilityHanger.transform.parent);
		abilityHanger.Init(gameManager.gameObject.transform, gameManager.Context, cardViewBuilder.AssetLookupSystem, faceInfoGenerator, matchManager?.Event?.PlayerEvent?.Format, gameManager.NpeDirector);
		_abilityHanger = abilityHanger;
		_abilityHanger.gameObject.SetLayer(layer);
		_abilityHanger.SetViewDragScroll(isDragScroll: true);
		_abilityHanger.gameObject.UpdateActive(active: false);
		_abilityHanger.CopyHangarViewTransform(_abilityViewTransformSource);
	}

	public void InitWrapper(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, AssetLookupSystem assetLookupSystem, IClientLocProvider locManager, IFaceInfoGenerator faceInfoGenerator, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, DeckFormat currentEventFormat, int layer = 0)
	{
		SharedInit(cardDatabase, cardViewBuilder, assetLookupSystem, locManager, faceInfoGenerator, unityObjectPool, genericObjectPool, layer);
		_abilityHangerTemplate = Object.Instantiate(_abilityHanger, _abilityHanger.transform.parent);
		_abilityHanger.gameObject.SetLayer(layer);
		_abilityHanger.Init(cardDatabase, cardViewBuilder.AssetLookupSystem, unityObjectPool, genericObjectPool, faceInfoGenerator, locManager, currentEventFormat);
		_abilityHanger.SetViewDragScroll(isDragScroll: true);
		_abilityHanger.gameObject.UpdateActive(active: false);
		_abilityHanger.CopyHangarViewTransform(_abilityViewTransformSource);
	}

	private void SharedInit(ICardDatabaseAdapter cardDatabase, CardViewBuilder cardViewBuilder, AssetLookupSystem assetLookupSystem, IClientLocProvider locManager, IFaceInfoGenerator faceInfoGenerator, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, int layer = 0)
	{
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<FaceHangerPrefab> loadedTree))
		{
			FaceHangerPrefab payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_faceHanger = AssetLoader.Instantiate<FaceHanger>(payload.Prefab.RelativePath, _faceHangerParent);
				_faceHangerPrefabRelativePath = payload.Prefab.RelativePath;
			}
		}
		_faceHanger.Init(faceInfoGenerator, cardViewBuilder);
		_faceHanger.transform.localPosition = Vector3.zero;
		_faceHanger.gameObject.SetLayer(layer);
		_faceHanger.gameObject.UpdateActive(active: false);
		_faceInfoGenerator = faceInfoGenerator;
		_cardViewBuilder = cardViewBuilder;
		_cardDatabase = cardDatabase;
		_unityObjectPool = unityObjectPool;
		_genericObjectPool = genericObjectPool;
		_locManager = locManager;
		_layer = layer;
		initialized = true;
	}

	public void ShowHangersDuelscene(DuelScene_CDC cardView, ICardDataAdapter sourceModel, HangerSituation situation)
	{
		if (!cardView.Model.IsDisplayedFaceDown)
		{
			ShowHangersForCard(cardView, sourceModel, situation);
		}
	}

	public void ShowHangersForCard(BASE_CDC cardView, ICardDataAdapter sourceModel, HangerSituation situation)
	{
		if (_activeCard != null)
		{
			ClearHangers();
		}
		_activeCard = cardView;
		_situation = situation;
		IReadOnlyCollection<FaceHanger.FaceCardInfo> readOnlyCollection = _faceInfoGenerator.GenerateFaceCardInfo(cardView.Model, sourceModel);
		if (readOnlyCollection.Count <= 1)
		{
			_faceHanger.ActivateHanger(cardView, sourceModel, _situation);
			_faceHangerLayoutElement.gameObject.SetActive(_faceHanger.Active);
			_faceHanger.SetColliderEnabled(enabled: false);
			_abilityHanger.ActivateHanger(cardView, sourceModel, _situation);
			_abilityHangerLayoutElement.gameObject.SetActive(_abilityHanger.Active);
		}
		else
		{
			_faceHangerLayoutElement.gameObject.SetActive(_faceHanger.Active);
			foreach (FaceHanger.FaceCardInfo item in readOnlyCollection)
			{
				_faceHangers.Add(GenerateFaceHanger(item));
			}
			ScrollViewActive = true;
			ScrollViewEnabled = _faceHangers.Count > _faceHangerScrollThreshold;
			_faceHangerScrollView.gameObject.UpdateActive(ScrollViewActive);
			_faceHangerScrollViewPort.enabled = ScrollViewEnabled;
			_scrollingAbilityHanger = GenerateAbilityHanger();
			_scrollingAbilityHanger.ActivateHanger(cardView, sourceModel, _situation);
			_scrollingAbilityHanger.transform.parent.gameObject.SetActive(_scrollingAbilityHanger.Active);
			_faceHangerScrollRect.horizontalNormalizedPosition = 1f;
			_faceHangerScrollRect.velocity = new Vector2(_faceHangerScrollRect.content.rect.width * _previewScrollSpeed, 0f);
		}
		_activationTimer = 0f;
		_activationTime = (situation.DelayActivation ? 0.5f : 0f);
	}

	private FaceHanger GenerateFaceHanger(FaceHanger.FaceCardInfo faceCardInfo)
	{
		Transform transform = Object.Instantiate(_faceHangerParentTemplate, _faceHangerParentTemplate.parent);
		FaceHanger faceHanger = AssetLoader.Instantiate<FaceHanger>(_faceHangerPrefabRelativePath, transform);
		faceHanger.Init(null, _cardViewBuilder);
		faceHanger.HangerItem.SetParent(transform, worldPositionStays: true);
		faceHanger.HangerItem.ZeroOut();
		faceHanger.HangerItem.gameObject.SetLayer(_layer);
		faceHanger.SetFace(faceCardInfo);
		ScrollFade componentInChildren = faceHanger.HangerItem.GetComponentInChildren<ScrollFade>(includeInactive: true);
		if ((object)componentInChildren != null)
		{
			componentInChildren.ScrollView = componentInChildren.ScrollView ?? _faceHangerScrollRect;
		}
		faceHanger.gameObject.UpdateActive(active: false);
		transform.gameObject.UpdateActive(active: true);
		return faceHanger;
	}

	private void DestroyFaceHanger(FaceHanger faceHanger)
	{
		GameObject obj = faceHanger.transform.parent.gameObject;
		faceHanger.Cleanup();
		Object.Destroy(obj);
	}

	private AbilityHangerBase GenerateAbilityHanger()
	{
		Transform parent = Object.Instantiate(_abilityHangerParentTemplate, _faceHangerScrollViewContent);
		AbilityHangerBase abilityHangerBase = Object.Instantiate(_abilityHangerTemplate, parent);
		if (_gameManager != null)
		{
			(abilityHangerBase as AbilityHanger)?.Init(_gameManager.gameObject.transform, _gameManager.Context, _cardViewBuilder.AssetLookupSystem, _faceInfoGenerator, _matchManager?.Event?.PlayerEvent?.Format, _gameManager.NpeDirector);
		}
		else
		{
			abilityHangerBase.Init(_cardDatabase, _cardViewBuilder.AssetLookupSystem, _unityObjectPool, _genericObjectPool, _faceInfoGenerator, _locManager, _matchManager?.Event?.PlayerEvent?.Format);
		}
		abilityHangerBase.gameObject.SetLayer(_layer);
		abilityHangerBase.SetViewDragScroll(isDragScroll: true);
		abilityHangerBase.transform.ZeroOut();
		abilityHangerBase.CopyHangarViewTransform(_abilityViewTransformSource);
		return abilityHangerBase;
	}

	private void DestroyAbilityHanger(AbilityHangerBase abilityHanger)
	{
		if (abilityHanger != null)
		{
			GameObject obj = abilityHanger.transform.parent.gameObject;
			abilityHanger.DeactivateHanger();
			Object.Destroy(obj);
		}
	}

	public void ClearHangers()
	{
		_activeCard = null;
		_faceHangerLayoutElement.gameObject.SetActive(value: false);
		_abilityHangerLayoutElement.gameObject.SetActive(value: false);
		_faceHanger.DeactivateHanger();
		_abilityHanger.DeactivateHanger();
		ClearScrollViewFaceHangers();
		_faceHangerScrollView.gameObject.SetActive(value: false);
		DestroyAbilityHanger(_scrollingAbilityHanger);
		ScrollViewActive = false;
		ScrollViewEnabled = false;
	}

	private void ClearScrollViewFaceHangers()
	{
		foreach (FaceHanger faceHanger in _faceHangers)
		{
			DestroyFaceHanger(faceHanger);
		}
		_faceHangers.Clear();
	}

	public void Update()
	{
		if (initialized)
		{
			UpdateHangers(Time.deltaTime);
		}
	}

	protected void UpdateHangers(float timeStep)
	{
		if (_activeCard == null)
		{
			if (_faceHanger.gameObject.activeSelf || _abilityHanger.gameObject.activeSelf)
			{
				ClearHangers();
			}
		}
		else if (_activeCard.IsVisible && (_activationTimer < _activationTime || _faceHanger.gameObject.activeSelf != _faceHanger.Active || _abilityHanger.gameObject.activeSelf != _abilityHanger.Active))
		{
			_activationTimer += timeStep;
			if (_activationTimer >= _activationTime)
			{
				_faceHanger.gameObject.UpdateActive(_faceHanger.Active);
				_abilityHanger.gameObject.UpdateActive(_abilityHanger.Active);
			}
		}
	}
}
