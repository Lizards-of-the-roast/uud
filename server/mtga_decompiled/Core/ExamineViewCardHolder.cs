using System.Collections.Generic;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using MovementSystem;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Examine;
using Wotc.Mtga.DuelScene.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Hangers;
using Wotc.Mtga.Loc;

public class ExamineViewCardHolder : CardHolderBase
{
	private class ExamineLayout : ICardLayout
	{
		public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
		{
			foreach (DuelScene_CDC allCardView in allCardViews)
			{
				Vector3 vector = Vector3.right * allCardView.ActiveScaffold.GetColliderBounds.extents.x;
				CardLayoutData item = new CardLayoutData(allCardView, center + vector);
				allData.Add(item);
			}
		}
	}

	private class MutationsLayout : ICardLayout
	{
		public Vector2 Padding = new Vector2(0.2f, 0.2f);

		public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
		{
			(int, float) tuple = ((allCardViews.Count < 4) ? (1, 0.9f) : ((allCardViews.Count < 9) ? (2, 0.6f) : (3, 0.4f)));
			int num = Mathf.CeilToInt((float)allCardViews.Count / (float)tuple.Item1);
			float num2 = center.y;
			int i = 0;
			int num3 = 0;
			for (; i < tuple.Item1; i++)
			{
				if (num3 >= allCardViews.Count)
				{
					break;
				}
				float num4 = center.x;
				float num5 = 0f;
				int num6 = 0;
				while (num6 < num && num3 < allCardViews.Count)
				{
					DuelScene_CDC card = allCardViews[num3];
					Vector2 vector = CalculateCardSize(card, tuple.Item2);
					float x = num4 + vector.x * 0.5f + Padding.x;
					float y = num2 - vector.y * 0.5f;
					allData.Add(new CardLayoutData(card, new Vector3(x, y, center.z), rotation, Vector3.one * tuple.Item2));
					num5 = ((vector.y > num5) ? vector.y : num5);
					num4 += vector.x + Padding.x;
					num6++;
					num3++;
				}
				num2 -= num5 + Padding.y;
			}
		}
	}

	[Space(5f)]
	[Header("Examine Specific elements")]
	[SerializeField]
	private Transform _leftMostAnchorPoint;

	[SerializeField]
	private FaceHanger _limboFaceHanger;

	[SerializeField]
	private ViewPrintingButton _examineStateToggle;

	[SerializeField]
	private ViewSimplifiedButton _cardStyleStateToggle;

	[SerializeField]
	private Vector2 _mutationPadding = Vector2.one * 0.2f;

	[SerializeField]
	private float _abilityScale = 1f;

	private bool _isAnimatingCard;

	private Vector3 _finalCardScale = Vector3.one;

	private ICardDatabaseAdapter _cardDatabase;

	public readonly HangerController HangerController = new HangerController();

	private ICardDataAdapter _sourceModel;

	private IModelConverter _modelConverter = NullConverter.Default;

	private IContextualModelGenerator _contextualModelGenerator = NullGenerator.Default;

	private readonly MutationsLayout _mutationsLayout = new MutationsLayout();

	private readonly List<DuelScene_CDC> _contextualCdcs = new List<DuelScene_CDC>(3);

	public FaceHanger LimboFaceHanger => _limboFaceHanger;

	public DuelScene_CDC ClonedCardView { get; private set; }

	private void Awake()
	{
		base.Layout = new ExamineLayout();
		_examineStateToggle.Clicked += OnExamineButtonClicked;
		_examineStateToggle.SetObjActive(active: false);
		_cardStyleStateToggle.Clicked += OnCardStylesButtonClicked;
		_cardStyleStateToggle.SetObjActive(active: false);
		Languages.LanguageChangedSignal.Listeners += SetButtonText;
	}

	public override void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		base.Init(gameManager, cardViewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
		if (_cardViewBuilder != null)
		{
			_cardViewBuilder.onCardUpdated += OnCardUpdated;
		}
		_cardDatabase = gameManager.CardDatabase;
		HangerController.Init(base.transform, _gameManager.MainCamera, _splineMovementSystem, FaceHanger.Create(_assetLookupSystem, base.transform, base.gameObject.layer), AbilityHanger.Create(_assetLookupSystem, base.transform, base.gameObject.layer), 0.175f, _abilityScale);
		_limboFaceHanger.Init(FaceInfoGeneratorFactory.DuelScene.LeftBattlefieldGenerator(_cardDatabase, gameManager.GetCurrentGameState, new IsSameCardDataComparer(gameManager)), _cardViewBuilder);
		ClonedCardView = cardViewManager.CreateCardView(CardDataExtensions.CreateBlank());
		AddCard(ClonedCardView);
		ClonedCardView.Root.ZeroOut();
		ClonedCardView.gameObject.SetLayer(base.gameObject.layer);
		_modelConverter = new ModelConverter(_cardDatabase);
		_contextualModelGenerator = new ContextualModelGenerator(_cardDatabase);
		_examineStateToggle.Init(_gameManager.LocManager, _assetLookupSystem, ClonedCardView);
		_cardStyleStateToggle.Init(_gameManager.LocManager, _assetLookupSystem, ClonedCardView);
		Clear();
	}

	private void Update()
	{
		HangerController.OnUpdate(Time.deltaTime);
		if (!(ClonedCardView == null))
		{
			if (_isAnimatingCard && ClonedCardView.transform.localScale == _finalCardScale)
			{
				_isAnimatingCard = false;
				SetupHangers();
				SetupToggles();
			}
			if (Input.anyKey && !Input.GetMouseButtonDown(0) && !Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButton(1) && !Input.GetMouseButtonUp(1))
			{
				Dismiss();
			}
		}
	}

	protected override void OnDestroy()
	{
		if (_cardViewBuilder != null)
		{
			_cardViewBuilder.onCardUpdated -= OnCardUpdated;
			_cardViewBuilder.DestroyCDC(ClonedCardView);
		}
		if ((bool)_examineStateToggle)
		{
			_examineStateToggle.Clicked -= OnExamineButtonClicked;
		}
		if ((bool)_cardStyleStateToggle)
		{
			_cardStyleStateToggle.Clicked -= OnCardStylesButtonClicked;
		}
		Languages.LanguageChangedSignal.Listeners -= SetButtonText;
		base.OnDestroy();
	}

	public void OnExamineButtonClicked()
	{
		_cardStyleStateToggle.CurrentState = (_cardStyleStateToggle.IsStyledCard() ? ExamineState.Styled : ExamineState.None);
		SetExamineState(_examineStateToggle.FindNextExamineState(), _examineStateToggle);
		_examineStateToggle.ButtonCheckmarkOn(!_examineStateToggle.IsButtonCheckmarkOn());
		_cardStyleStateToggle.ButtonCheckmarkOn(active: false);
	}

	public void OnCardStylesButtonClicked()
	{
		SetExamineState(_cardStyleStateToggle.FindNextExamineState(), _cardStyleStateToggle);
		_cardStyleStateToggle.ButtonCheckmarkOn(!_cardStyleStateToggle.IsButtonCheckmarkOn());
		SetupHangers();
	}

	public void SetExamineState(ExamineState state, ViewPrintingButton button)
	{
		button.CurrentState = state;
		ICardDataAdapter sourceModel;
		if (_examineStateToggle.CurrentState == ExamineState.Printing && _cardStyleStateToggle.CurrentState != ExamineState.None)
		{
			sourceModel = ClonedCardView.Model;
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
			ClonedCardView?.gameObject?.UpdateActive(active: true);
			ExamineCardInternal(sourceModel);
			IReadOnlyList<ICardDataAdapter> readOnlyList = _contextualModelGenerator.GenerateContextualModels(_sourceModel, state);
			foreach (ICardDataAdapter item in readOnlyList)
			{
				_contextualCdcs.Add(GenerateContextualCDC(item));
			}
			while (_contextualCdcs.Count > readOnlyList.Count)
			{
				_cardViewBuilder.DestroyCDC(_contextualCdcs[0]);
				_contextualCdcs.RemoveAt(0);
			}
		}
		else
		{
			Dismiss();
		}
	}

	private void ExamineCardInternal(ICardDataAdapter examineModel)
	{
		ClonedCardView.IsExaminedCard = true;
		ClonedCardView.SetModel(examineModel);
		ClonedCardView.UpdateHighlight(HighlightType.None);
		ClonedCardView.SetOpponentHoverState(isMousedOver: false);
		ClonedCardView.SetDimmedState(isDimmed: false);
		ClonedCardView.ImmediateUpdate();
		Vector3 right = Vector3.right;
		if ((bool)ClonedCardView.ActiveScaffold)
		{
			right *= ClonedCardView.ActiveScaffold.GetColliderBounds.extents.x;
		}
		Vector3 localPosition = GetLayoutCenterPoint() + right;
		Transform transform = ClonedCardView.transform;
		if (ClonedCardView.TargetVisibility)
		{
			transform.localPosition = localPosition;
			transform.localScale = _finalCardScale;
			_isAnimatingCard = false;
		}
		else
		{
			transform.localPosition = localPosition;
			transform.position += base.transform.TransformVector(Vector3.forward);
			transform.localScale = _finalCardScale * 0.8f;
			_isAnimatingCard = true;
		}
		LayoutNow();
		if (!_isAnimatingCard)
		{
			SetupHangers();
			SetupToggles();
		}
		ClonedCardView.PlayPersistVFX<PersistVFXOnHoveredAndExaminedCards>();
		AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_single_card_examine_big_in.EventName, ClonedCardView.Root.gameObject);
	}

	private void SetupHangers()
	{
		ClonedCardView.UpdateVisibility(shouldBeVisible: true);
		if (_examineStateToggle.CurrentState == ExamineState.PrintingWithMutations || _examineStateToggle.CurrentState == ExamineState.Specialize)
		{
			HangerController.ClearHangers();
			return;
		}
		HangerController.ShowHangersForCard(ClonedCardView, (_examineStateToggle.CurrentState == ExamineState.Instance && _sourceModel != null) ? _sourceModel : ClonedCardView.Model, new HangerSituation
		{
			ShowFlavorText = true
		});
	}

	private void SetupToggles()
	{
		_examineStateToggle.SetupToggle();
		_cardStyleStateToggle.SetupToggle();
		_examineStateToggle.LayoutToggleRect(ClonedCardView);
	}

	public void ExamineCard(DuelScene_CDC cdc)
	{
		ExamineCard(cdc.VisualModel, cdc.CurrentCardHolder?.CardHolderType ?? CardHolderType.Invalid);
	}

	public void ExamineCard(ICardDataAdapter model)
	{
		ExamineCard(model, CardHolderType.Invalid);
	}

	private void ExamineCard(ICardDataAdapter model, CardHolderType cardHolder)
	{
		if (_sourceModel == model)
		{
			CycleFaceHangers();
			return;
		}
		_sourceModel = model;
		_examineStateToggle.UpdateSourceModel(model, cardHolder);
		_cardStyleStateToggle.UpdateSourceModel(model, cardHolder);
		SetExamineState(_examineStateToggle.DefaultState, _examineStateToggle);
		_cardStyleStateToggle.CurrentState = _cardStyleStateToggle.DefaultState;
	}

	public void CycleFaceHangers()
	{
		HangerController.ShowNextFaceHanger();
	}

	public void Dismiss()
	{
		if (_examineStateToggle.CurrentState != ExamineState.None || _cardStyleStateToggle.CurrentState != ExamineState.None)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_single_card_examine_big_out.EventName, ClonedCardView.Root.gameObject);
		}
		Clear();
	}

	private void Clear()
	{
		_sourceModel = null;
		_examineStateToggle.Clear();
		_cardStyleStateToggle.Clear();
		ClonedCardView?.UpdateVisibility(shouldBeVisible: false);
		ClonedCardView?.gameObject?.UpdateActive(active: false);
		HangerController.ClearHangers();
		ClearContextualCDCs();
	}

	private DuelScene_CDC GenerateContextualCDC(ICardDataAdapter model)
	{
		DuelScene_CDC duelScene_CDC = _cardViewBuilder.CreateDuelSceneCdc(model, _gameManager.GetCurrentGameState, _gameManager.GetCurrentInteraction, _gameManager.VfxProvider);
		duelScene_CDC.gameObject.SetLayer(base.Layer);
		duelScene_CDC.Collider.enabled = false;
		duelScene_CDC.CurrentCardHolder = this;
		duelScene_CDC.PreviousCardHolder = this;
		duelScene_CDC.Root.localScale = Vector3.one * 0.5f;
		duelScene_CDC.Root.position = base.transform.position + base.transform.forward * 0.5f;
		duelScene_CDC.ImmediateUpdate();
		return duelScene_CDC;
	}

	private void ClearContextualCDCs()
	{
		foreach (DuelScene_CDC contextualCdc in _contextualCdcs)
		{
			_cardViewBuilder.DestroyCDC(contextualCdc);
		}
		_contextualCdcs.Clear();
	}

	private void OnCardUpdated(BASE_CDC cdc)
	{
		if (ClonedCardView == cdc)
		{
			LayoutNow();
		}
		if (_contextualCdcs.Contains(cdc as DuelScene_CDC))
		{
			cdc.Collider.enabled = false;
			cdc.gameObject.SetLayer(base.Layer);
		}
	}

	private void SetButtonText()
	{
		_examineStateToggle.SetButtonText();
		_cardStyleStateToggle.SetButtonText();
	}

	protected override void LayoutNowInternal(List<DuelScene_CDC> cardsToLayout, bool layoutInstantly = false)
	{
		if (ClonedCardView != null)
		{
			List<DuelScene_CDC> list = _gameManager.GenericPool.PopObject<List<DuelScene_CDC>>();
			list.Add(ClonedCardView);
			base.LayoutNowInternal(list, layoutInstantly);
			_gameManager.GenericPool.PushObject(list);
		}
		if (_contextualCdcs.Count != 0)
		{
			_secondaryLayoutData.Clear();
			_mutationsLayout.Padding = _mutationPadding;
			Vector3 vector = CalculateCardSize(ClonedCardView);
			Vector3 center = ClonedCardView.transform.localPosition + new Vector3(vector.x * 0.5f, vector.y * 0.5f, 0f);
			_mutationsLayout.GenerateData(_contextualCdcs, ref _secondaryLayoutData, center, Quaternion.identity);
			for (int i = 0; i < _secondaryLayoutData.Count; i++)
			{
				CardLayoutData cardLayoutData = _secondaryLayoutData[i];
				cardLayoutData.Card.Root.parent = CardRoot;
				ApplyLayoutData(cardLayoutData, added: false, CalcCardVisibility(cardLayoutData, i), layoutInstantly);
			}
		}
	}

	protected override Vector3 GetLayoutCenterPoint()
	{
		return _leftMostAnchorPoint.localPosition;
	}

	public override void RemoveCard(DuelScene_CDC cardView)
	{
		cardView.Collider.enabled = true;
		cardView.Collider.gameObject.UpdateActive(active: true);
		cardView.ClearOverrides();
		base.RemoveCard(cardView);
	}

	private static Vector2 CalculateCardSize(DuelScene_CDC card, float scale = 1f)
	{
		if (card == null || card.Collider == null)
		{
			return Vector2.zero;
		}
		return card.Collider.size * scale;
	}
}
