using System;
using System.Collections;
using System.Collections.Generic;
using GreClient.CardData;
using MTGA.KeyboardManager;
using MovementSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Cards.Parts.Textbox;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Examine;
using Wotc.Mtga.DuelScene.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class StaticZoomController : CardHolderBase, IKeyDownSubscriber, IKeySubscriber
{
	private const string ANIMATOR_BOOL_ACTIVE_STATE = "Active";

	[Header("Static Hover Card Elements")]
	[SerializeField]
	private Transform _cardTransformParent;

	[SerializeField]
	protected StaticHangerController _staticHangerController;

	[SerializeField]
	private ViewPrintingButton _examineStateToggle;

	[SerializeField]
	private ViewSimplifiedButton _cardStyleStateToggle;

	[SerializeField]
	protected LayoutElement _cardHolderLayout;

	[SerializeField]
	private float _clickAndHoldDuration = 0.1f;

	[Tooltip("Amount of drag allowed to Zoom as a fraction of screen height")]
	[SerializeField]
	private float _screenDragThreshold = 0.1f;

	[SerializeField]
	private CardZoomModalFade _modalFade;

	[Header("Static Hover Card Elements")]
	[SerializeField]
	private Transform _contextualCDCTemplate;

	[SerializeField]
	private Image _contextualCDCScrollViewPort;

	[SerializeField]
	private Transform _contextualCDCLayoutParent;

	[SerializeField]
	private int _contextualCDCScrollThreshold = 2;

	[Header("Layout Parameters")]
	[SerializeField]
	protected float CardWidth = 600f;

	[SerializeField]
	private HorizontalOrVerticalLayoutGroup _layoutGroup;

	private Animator _animator;

	private KeyboardManager _keyboardManager;

	private IModelConverter _modelConverter = NullConverter.Default;

	private IContextualModelGenerator _contextualModelGenerator = NullGenerator.Default;

	private ICardDataAdapter _sourceModel;

	private DuelScene_CDC _clonedCardView;

	private Coroutine _clickAndHoldTimer;

	private readonly List<DuelScene_CDC> _contextualCdcs = new List<DuelScene_CDC>(3);

	public PriorityLevelEnum Priority => PriorityLevelEnum.Moz;

	private void Awake()
	{
		_examineStateToggle.Clicked += OnExamineButtonClicked;
		_examineStateToggle.SetObjActive(active: false);
		_cardStyleStateToggle.Clicked += OnCardStylesButtonClicked;
		_cardStyleStateToggle.SetObjActive(active: false);
		Languages.LanguageChangedSignal.Listeners += SetButtonText;
	}

	public void Init(GameManager gameManager, EntityViewManager viewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager, IFaceInfoGenerator faceInfoGenerator)
	{
		base.Init(gameManager, viewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
		_keyboardManager = _gameManager.KeyboardManager;
		_keyboardManager?.Subscribe(this);
		_animator = GetComponent<Animator>();
		ICardDatabaseAdapter cardDatabase = _gameManager.CardDatabase;
		_staticHangerController.InitDuelscene(cardDatabase, _cardViewBuilder, _gameManager, _locManager, faceInfoGenerator, gameManager.UnityPool, gameManager.GenericPool, _matchManager, base.gameObject.layer);
		_clonedCardView = viewManager.CreateCardView(CardDataExtensions.CreateBlank());
		_clonedCardView.UpdateVisuals();
		_clonedCardView.Root.SetParent(_cardTransformParent, worldPositionStays: true);
		_clonedCardView.Root.ZeroOut();
		_clonedCardView.gameObject.SetLayer(base.Layer);
		_clonedCardView.Collider.enabled = false;
		_modelConverter = new ModelConverter(cardDatabase);
		_contextualModelGenerator = new ContextualModelGenerator(cardDatabase);
		_animator.SetBool("Active", value: false);
		CardZoomModalFade modalFade = _modalFade;
		modalFade.OnClicked = (System.Action)Delegate.Combine(modalFade.OnClicked, new System.Action(Clear));
		_examineStateToggle.Init(_gameManager.LocManager, _assetLookupSystem, _clonedCardView);
		_cardStyleStateToggle.Init(_gameManager.LocManager, _assetLookupSystem, _clonedCardView);
		Clear();
	}

	protected override void OnDestroy()
	{
		ClearClickAndHoldProperties();
		if (_cardViewBuilder != null)
		{
			_cardViewBuilder.DestroyCDC(_clonedCardView);
		}
		if ((bool)_examineStateToggle)
		{
			_examineStateToggle.Clicked -= OnExamineButtonClicked;
		}
		if ((bool)_cardStyleStateToggle)
		{
			_cardStyleStateToggle.Clicked -= OnCardStylesButtonClicked;
		}
		if (_modalFade != null)
		{
			CardZoomModalFade modalFade = _modalFade;
			modalFade.OnClicked = (System.Action)Delegate.Remove(modalFade.OnClicked, new System.Action(Clear));
		}
		_keyboardManager?.Unsubscribe(this);
		Languages.LanguageChangedSignal.Listeners -= SetButtonText;
		base.OnDestroy();
	}

	public void OnCardDown(DuelScene_CDC cardView, PointerEventData eventData)
	{
		ClearClickAndHoldProperties();
		if (CanLongTap(cardView, eventData))
		{
			_clickAndHoldTimer = StartCoroutine(ProcessLongTap(cardView, eventData));
		}
	}

	private IEnumerator ProcessLongTap(DuelScene_CDC card, PointerEventData eventData)
	{
		yield return new WaitForSecondsRealtime(_clickAndHoldDuration);
		_clickAndHoldTimer = null;
		if (!IsDragging(eventData))
		{
			_gameManager.InteractionSystem.CancelAnyDrag();
			ExamineCard(card);
		}
	}

	private bool CanLongTap(DuelScene_CDC card, PointerEventData eventData)
	{
		if (card != null)
		{
			return !IsDragging(eventData);
		}
		return false;
	}

	private bool IsDragging(PointerEventData eventData)
	{
		if (eventData == null)
		{
			return false;
		}
		if (eventData.dragging)
		{
			return Vector2.Distance(eventData.pressPosition, eventData.position) > _screenDragThreshold * (float)Screen.height;
		}
		return false;
	}

	public void ClearClickAndHoldProperties()
	{
		if (_clickAndHoldTimer != null)
		{
			StopCoroutine(_clickAndHoldTimer);
			_clickAndHoldTimer = null;
		}
	}

	public void ExamineCard(DuelScene_CDC cdc)
	{
		ExamineCard(cdc.VisualModel, cdc.CurrentCardHolder?.CardHolderType ?? CardHolderType.Invalid);
	}

	private void ExamineCard(ICardDataAdapter model, CardHolderType cardHolder)
	{
		if (_sourceModel != model)
		{
			_sourceModel = model;
			_examineStateToggle.UpdateSourceModel(model, cardHolder);
			_cardStyleStateToggle.UpdateSourceModel(model, cardHolder);
			SetExamineState(_examineStateToggle.DefaultState, _examineStateToggle);
			_cardStyleStateToggle.CurrentState = _cardStyleStateToggle.DefaultState;
			_animator.SetBool("Active", value: true);
		}
	}

	private void OnExamineButtonClicked()
	{
		SetExamineState(_examineStateToggle.FindNextExamineState(), _examineStateToggle);
		_examineStateToggle.ButtonCheckmarkOn(!_examineStateToggle.IsButtonCheckmarkOn());
		_cardStyleStateToggle.CurrentState = (_cardStyleStateToggle.IsStyledCard() ? ExamineState.Styled : ExamineState.None);
		_cardStyleStateToggle.ButtonCheckmarkOn(active: false);
	}

	public void OnCardStylesButtonClicked()
	{
		SetExamineState(_cardStyleStateToggle.FindNextExamineState(), _cardStyleStateToggle);
		_cardStyleStateToggle.ButtonCheckmarkOn(!_cardStyleStateToggle.IsButtonCheckmarkOn());
		_staticHangerController.ShowHangersDuelscene(_clonedCardView, (_examineStateToggle.CurrentState == ExamineState.Instance && _sourceModel != null) ? _sourceModel : _clonedCardView.Model, new HangerSituation
		{
			ShowFlavorText = true
		});
	}

	private void SetExamineState(ExamineState state, ViewPrintingButton button)
	{
		button.CurrentState = state;
		ICardDataAdapter sourceModel;
		if (_examineStateToggle.CurrentState == ExamineState.Printing && _cardStyleStateToggle.CurrentState != ExamineState.None)
		{
			sourceModel = _clonedCardView.Model;
			if (state == ExamineState.Styled)
			{
				state = ExamineState.Printing;
			}
		}
		else
		{
			sourceModel = _sourceModel;
		}
		sourceModel = _modelConverter.ConvertModel(sourceModel, state);
		if (sourceModel != null)
		{
			_clonedCardView?.gameObject?.UpdateActive(active: true);
			ExamineCardInternal(sourceModel);
			IReadOnlyList<ICardDataAdapter> readOnlyList = _contextualModelGenerator.GenerateContextualModels(_sourceModel, state);
			foreach (ICardDataAdapter item in readOnlyList)
			{
				_contextualCdcs.Add(GenerateContextualCDC(item));
			}
			while (_contextualCdcs.Count > readOnlyList.Count)
			{
				DestroyContextualCDC(_contextualCdcs[0]);
				_contextualCdcs.RemoveAt(0);
			}
			_contextualCDCLayoutParent.gameObject.UpdateActive(_contextualCdcs.Count > 0 || _staticHangerController.ScrollViewActive);
			_contextualCDCScrollViewPort.enabled = _contextualCdcs.Count > _contextualCDCScrollThreshold || _staticHangerController.ScrollViewEnabled;
		}
		else
		{
			Clear();
		}
	}

	private DuelScene_CDC GenerateContextualCDC(ICardDataAdapter model)
	{
		Transform transform = UnityEngine.Object.Instantiate(_contextualCDCTemplate, _contextualCDCTemplate.parent);
		transform.gameObject.UpdateActive(active: true);
		DuelScene_CDC duelScene_CDC = _cardViewBuilder.CreateDuelSceneCdc(model, _gameManager.GetCurrentGameState, _gameManager.GetCurrentInteraction, _gameManager.VfxProvider);
		duelScene_CDC.Root.SetParent(transform.GetChild(0), worldPositionStays: true);
		duelScene_CDC.Root.ZeroOut();
		duelScene_CDC.gameObject.SetLayer(base.Layer);
		duelScene_CDC.UpdateVisibility(shouldBeVisible: true);
		duelScene_CDC.Collider.enabled = false;
		duelScene_CDC.CurrentCardHolder = this;
		duelScene_CDC.PreviousCardHolder = this;
		return duelScene_CDC;
	}

	private void DestroyContextualCDC(DuelScene_CDC cdc)
	{
		Transform parent = cdc.transform.parent.parent;
		_cardViewBuilder.DestroyCDC(cdc);
		UnityEngine.Object.Destroy(parent.gameObject);
	}

	private void ExamineCardInternal(ICardDataAdapter examineModel)
	{
		_clonedCardView.SetModel(examineModel, updateVisuals: true, CardHolderType.Examine);
		_clonedCardView.UpdateHighlight(HighlightType.None);
		_clonedCardView.SetOpponentHoverState(isMousedOver: false);
		_clonedCardView.SetDimmedState(isDimmed: false);
		_clonedCardView.ImmediateUpdate();
		_clonedCardView.IsMousedOver = true;
		_clonedCardView.IsHoverCopy = true;
		if (_examineStateToggle.CurrentState == ExamineState.PrintingWithMutations || _examineStateToggle.CurrentState == ExamineState.Specialize)
		{
			_staticHangerController.ClearHangers();
		}
		else
		{
			_clonedCardView.UpdateVisibility(shouldBeVisible: true);
			_staticHangerController.ShowHangersDuelscene(_clonedCardView, (_examineStateToggle.CurrentState == ExamineState.Instance) ? _sourceModel : examineModel, new HangerSituation
			{
				ShowFlavorText = true
			});
		}
		_animator.SetBool("Active", value: true);
		_clonedCardView.Collider.enabled = false;
		foreach (CDCPart_Textbox_SuperBase item in _clonedCardView.FindAllParts<CDCPart_Textbox_SuperBase>(AnchorPointType.Invalid))
		{
			item.EnableTouchScroll();
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_single_card_examine_big_in.EventName, _clonedCardView.Root.gameObject);
		_cardHolderLayout.minWidth = HACK_GetCardWidth(examineModel, CardWidth);
		SetupToggles();
	}

	private void SetupToggles()
	{
		_examineStateToggle.SetupToggle();
		_cardStyleStateToggle.SetupToggle();
		_examineStateToggle.LayoutToggleRect(_clonedCardView);
	}

	private static float HACK_GetCardWidth(ICardDataAdapter model, float defaultWidth)
	{
		if (model.CardTypes.Contains(CardType.Battle))
		{
			return defaultWidth * 0.2f + defaultWidth;
		}
		if (model.IsRoomParent())
		{
			return defaultWidth * 2f;
		}
		if (model.LinkedFaceGrpIds.Count > 1 && model.Printing.LinkedFaceType != LinkedFace.SpecializeChild)
		{
			return defaultWidth * 2f;
		}
		return defaultWidth;
	}

	private void SetButtonText()
	{
		_examineStateToggle.SetButtonText();
		_cardStyleStateToggle.SetButtonText();
	}

	public void Clear()
	{
		_sourceModel = null;
		_examineStateToggle.Clear();
		_cardStyleStateToggle.Clear();
		_clonedCardView?.UpdateVisibility(shouldBeVisible: false);
		_clonedCardView?.gameObject?.UpdateActive(active: false);
		_staticHangerController.ClearHangers();
		_contextualCDCLayoutParent.gameObject.UpdateActive(active: false);
		ClearContextualCDCs();
		ClearClickAndHoldProperties();
		_animator.SetBool("Active", value: false);
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_single_card_examine_big_out.EventName, _clonedCardView.Root.gameObject);
	}

	private void ClearContextualCDCs()
	{
		foreach (DuelScene_CDC contextualCdc in _contextualCdcs)
		{
			DestroyContextualCDC(contextualCdc);
		}
		_contextualCdcs.Clear();
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape && PlatformUtils.IsHandheld())
		{
			Clear();
			return true;
		}
		return false;
	}
}
