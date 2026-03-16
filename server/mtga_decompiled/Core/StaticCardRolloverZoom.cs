using System;
using System.Collections;
using Core.Code.Decks;
using Core.Meta.Cards.Views;
using GreClient.CardData;
using MTGA.KeyboardManager;
using Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Cards.Parts.Textbox;
using Wotc.Mtga.DuelScene.Examine;
using Wotc.Mtga.DuelScene.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class StaticCardRolloverZoom : CardRolloverZoomBase
{
	[Header("Static Hover Card Elements")]
	[SerializeField]
	protected StaticHangerController _staticHangerController;

	[SerializeField]
	protected LayoutElement _metaCardLayoutContainer;

	[SerializeField]
	private CardZoomModalFade _modalFade;

	[SerializeField]
	private GameObject _viewPrintedCardButton;

	[SerializeField]
	private ViewSimplifiedButton _cardStyleStateToggle;

	[SerializeField]
	private float _clickAndHoldDuration = 0.1f;

	[Header("Layout Sizes")]
	[SerializeField]
	protected float CardWidth = 600f;

	private IModelConverter _modelConverter = NullConverter.Default;

	private Animator _animator;

	private Coroutine _clickAndHoldTimer;

	private MetaCardView _sourceCard;

	private MetaCardHolder _currentCardHolder;

	private static readonly int Active = Animator.StringToHash("Active");

	public StaticHangerController StaticHangerController => _staticHangerController;

	private MetaCardViewDragState MetaCardViewDragState => Pantry.Get<MetaCardViewDragState>();

	private bool UsingCardStyleStateToggle => _cardStyleStateToggle;

	private void Awake()
	{
		if ((bool)_cardStyleStateToggle)
		{
			if (UsingCardStyleStateToggle)
			{
				_cardStyleStateToggle.Clicked += OnCardStylesButtonClicked;
			}
			_cardStyleStateToggle.gameObject.UpdateActive(active: false);
		}
		Languages.LanguageChangedSignal.Listeners += SetButtonText;
	}

	public override void Initialize(CardViewBuilder cardViewBuilder, CardDatabase cardDatabase, IClientLocProvider locManager, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, KeyboardManager keyboardManager, DeckFormat currentEventFormat)
	{
		base.Initialize(cardViewBuilder, cardDatabase, locManager, unityObjectPool, genericObjectPool, keyboardManager, currentEventFormat);
		_modelConverter = new ModelConverter(cardDatabase);
		_staticHangerController.InitWrapper(cardDatabase, cardViewBuilder, cardViewBuilder.AssetLookupSystem, locManager, FaceInfoGeneratorFactory.HoverGenerator(cardDatabase, cardViewBuilder.AssetLookupSystem, genericObjectPool), unityObjectPool, genericObjectPool, currentEventFormat, base.gameObject.layer);
		_zoomCard.UpdateVisuals();
		_zoomCard.Root.SetParent(_cardParent, worldPositionStays: true);
		_zoomCard.Root.ZeroOut();
		CardZoomModalFade modalFade = _modalFade;
		modalFade.OnClicked = (System.Action)Delegate.Combine(modalFade.OnClicked, new System.Action(Close));
		_animator = GetComponent<Animator>();
		_animator.SetBool(Active, value: false);
		if (UsingCardStyleStateToggle)
		{
			_cardStyleStateToggle.Init(locManager, cardViewBuilder.AssetLookupSystem, _zoomCard);
		}
	}

	public override void CardPointerDown(PointerEventData.InputButton inputButton, ICardDataAdapter model, MetaCardView metaCardView = null, HangerSituation hangerSituation = default(HangerSituation))
	{
		if (_rolloverCoroutine != null)
		{
			StopCoroutine(_rolloverCoroutine);
		}
		if (PlatformUtils.IsHandheld())
		{
			_clickAndHoldTimer = StartCoroutine(CheckClickAndHold(model, metaCardView, hangerSituation));
		}
		else if (inputButton == PointerEventData.InputButton.Right && (SceneLoader.GetSceneLoader().CurrentContentType != NavContentType.DeckBuilder || !Pantry.Get<DeckBuilderContextProvider>().Context.CanCraft))
		{
			ShowCardDisplayWindow(model, metaCardView, hangerSituation);
		}
	}

	private IEnumerator CheckClickAndHold(ICardDataAdapter model, MetaCardView metaCardView, HangerSituation hangerSituation)
	{
		yield return new WaitForSecondsRealtime(_clickAndHoldDuration);
		ShowCardDisplayWindow(model, metaCardView, hangerSituation);
	}

	private void ShowCardDisplayWindow(ICardDataAdapter model, MetaCardView metaCardView, HangerSituation hangerSituation)
	{
		if (model != null && !(MetaCardViewDragState.DraggingCard != null) && (!(metaCardView != null) || !metaCardView.IsDragDetected))
		{
			_sourceCard = metaCardView;
			if (_viewPrintedCardButton != null)
			{
				_viewPrintedCardButton.UpdateActive(active: false);
			}
			base.OnRolloverStart?.Invoke(model);
			_currentCardHolder = ((_sourceCard == null) ? null : _sourceCard.Holder);
			_lastRolloverModel = model;
			_lastHangerSituation = hangerSituation;
			_rolloverCoroutine = StartCoroutine(Coroutine_RolloverWait());
		}
	}

	private IEnumerator Coroutine_RolloverWait()
	{
		while (!base.IsActive)
		{
			yield return new WaitForEndOfFrame();
		}
		_zoomCard.gameObject.SetActive(value: true);
		_zoomCard.IsMousedOver = true;
		_animator.SetBool(Active, value: true);
		if (_currentCardHolder != null)
		{
			_currentCardHolder.SetScrollEnabled(enabled: false);
		}
		if (_sourceCard != null)
		{
			_sourceCard.StartZoom();
		}
		base.OnRollover?.Invoke(_zoomCard);
		SetZoomCardModel(_lastRolloverModel);
		if (UsingCardStyleStateToggle)
		{
			_cardStyleStateToggle.CurrentState = _cardStyleStateToggle.DefaultState;
			_cardStyleStateToggle.SetButtonText();
			_cardStyleStateToggle.ButtonCheckmarkOn(active: false);
			_cardStyleStateToggle.UpdateSourceModel(_lastRolloverModel, CardHolderType.RolloverZoom);
			_cardStyleStateToggle.SetupToggle();
		}
		_metaCardLayoutContainer.minWidth = GetCardWidth(_zoomCard.Model, CardWidth);
	}

	private void OnCardStylesButtonClicked()
	{
		SetExamineState(_cardStyleStateToggle.FindNextExamineState(), _cardStyleStateToggle);
		_staticHangerController.ShowHangersForCard(_zoomCard, _cardStyleStateToggle.GetSourceModel(), _lastHangerSituation);
	}

	private void SetExamineState(ExamineState state, ViewPrintingButton button)
	{
		button.CurrentState = state;
		ICardDataAdapter cardDataAdapter = _modelConverter.ConvertModel(_lastRolloverModel, state);
		if (cardDataAdapter != null)
		{
			SetZoomCardModel(cardDataAdapter);
		}
		else
		{
			Close();
		}
	}

	private void SetZoomCardModel(ICardDataAdapter model)
	{
		if (_zoomCard.Model != model)
		{
			_zoomCard.SetModel(model, updateVisuals: true, CardHolderType.RolloverZoom);
		}
		_zoomCard.SetDimmed(null);
		_zoomCard.UpdateVisibility(shouldBeVisible: true);
		_zoomCard.ImmediateUpdate();
		_zoomCard.Collider.enabled = false;
		DecorateLastHangerSituation();
		_staticHangerController.ShowHangersForCard(_zoomCard, _zoomCard.Model, _lastHangerSituation);
		foreach (CDCPart_Textbox_SuperBase item in _zoomCard.FindAllParts<CDCPart_Textbox_SuperBase>(AnchorPointType.Invalid))
		{
			item.EnableTouchScroll();
		}
		_zoomCard.GetSleeveFXPayload(model, CardHolderType.None, out var sleeveFXPayload, out var prefabFilePath);
		if (sleeveFXPayload != null && prefabFilePath != null)
		{
			GameObject gameObject = _unityObjectPool.PopObject(prefabFilePath);
			gameObject.transform.SetParent(_zoomCard.EffectsRoot);
			gameObject.transform.localPosition = sleeveFXPayload.OffsetData.PositionOffset;
			gameObject.transform.localEulerAngles = sleeveFXPayload.OffsetData.RotationOffset;
			gameObject.transform.localScale = sleeveFXPayload.OffsetData.ScaleMultiplier;
			gameObject.AddOrGetComponent<SelfCleanup>().SetLifetime(sleeveFXPayload.CleanUpAfterSeconds);
			AudioManager.PlayAudio(sleeveFXPayload.AudioEvent, gameObject);
		}
	}

	private void SetButtonText()
	{
		if (UsingCardStyleStateToggle)
		{
			_cardStyleStateToggle.SetButtonText();
		}
	}

	private static float GetCardWidth(ICardDataAdapter model, float defaultWidth)
	{
		if (model.CardTypes.Contains(CardType.Battle))
		{
			return defaultWidth * 1.333f;
		}
		if (model.LinkedFaceGrpIds.Count == 2)
		{
			return defaultWidth * 2f;
		}
		return defaultWidth;
	}

	public override void CardPointerUp(PointerEventData.InputButton inputButton, ICardDataAdapter model, MetaCardView metaCardView = null)
	{
		if (_clickAndHoldTimer != null)
		{
			StopCoroutine(_clickAndHoldTimer);
		}
	}

	public override bool CardRolledOver(ICardDataAdapter model, Bounds cardColliderBounds, HangerSituation hangerSituation = default(HangerSituation), Vector2 offset = default(Vector2))
	{
		return false;
	}

	public override void CardRolledOff(ICardDataAdapter model, bool alwaysRollOff = false)
	{
		if (_clickAndHoldTimer != null)
		{
			StopCoroutine(_clickAndHoldTimer);
		}
	}

	public override bool CardScrolled(Vector2 scrollDelta)
	{
		return false;
	}

	public override void Close()
	{
		_lastRolloverModel = null;
		base.OnRolloff?.Invoke(_zoomCard);
		if (UsingCardStyleStateToggle)
		{
			_cardStyleStateToggle.Clear();
		}
		if (_zoomCard != null)
		{
			_zoomCard.UpdateVisibility(shouldBeVisible: false);
			_zoomCard.gameObject.UpdateActive(active: false);
		}
		_staticHangerController.ClearHangers();
		if (_rolloverCoroutine != null)
		{
			StopCoroutine(_rolloverCoroutine);
		}
		_animator.SetBool(Active, value: false);
		if (_currentCardHolder != null)
		{
			_currentCardHolder.SetScrollEnabled(enabled: true);
			_currentCardHolder = null;
		}
		if (_sourceCard != null)
		{
			_sourceCard.CancelZoom();
			_sourceCard = null;
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		CardZoomModalFade modalFade = _modalFade;
		modalFade.OnClicked = (System.Action)Delegate.Remove(modalFade.OnClicked, new System.Action(Close));
		if (UsingCardStyleStateToggle)
		{
			_cardStyleStateToggle.Clicked -= OnCardStylesButtonClicked;
		}
		Languages.LanguageChangedSignal.Listeners -= SetButtonText;
	}
}
