using System;
using System.Collections.Generic;
using Core.Code.Decks;
using Core.Meta.Cards.Views;
using TMPro;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class DeckSideboardTitlePanel : MonoBehaviour
{
	[SerializeField]
	private Localize _cardCountLabel;

	[SerializeField]
	private Animator _expandAnimator;

	[SerializeField]
	private Animator _contractAnimator;

	[SerializeField]
	private Animator _expandOnHoverAnimator;

	[SerializeField]
	private Animator _arrowAnimator;

	[SerializeField]
	private Animator _highlightAnimator;

	[SerializeField]
	private CustomButton _button;

	[SerializeField]
	private bool _expandOnHover = true;

	[SerializeField]
	private Color _cardCountWarningColor = new Color(0.87f, 0.62f, 0.078f);

	[SerializeField]
	private Color _cardCountErrorColor = new Color(0.72f, 0.07f, 0.07f);

	private Color _cardCountDefaultColor;

	private TMP_Text _labelText;

	private bool _dropStateOn;

	public Action OnExpandChange;

	private static int ArrowExpandedHash = Animator.StringToHash("ArrowExpanded");

	private static int ExpandOnHover = Animator.StringToHash("ExpandOnHover");

	private static readonly int Expand = Animator.StringToHash("Expand");

	private static readonly int Highlight = Animator.StringToHash("Highlight");

	public bool Expanded { get; private set; }

	private DeckBuilderModel Model => Pantry.Get<DeckBuilderModelProvider>().Model;

	private DeckBuilderContext Context => Pantry.Get<DeckBuilderContextProvider>().Context;

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	private void Awake()
	{
		_button.OnClick.AddListener(ToggleExpand);
		_button.OnMouseover.AddListener(OnMouseover);
		_button.OnMouseoff.AddListener(OnExit);
	}

	private void GetLabel()
	{
		if (!_labelText)
		{
			_labelText = _cardCountLabel.GetComponent<TMP_Text>();
			_cardCountDefaultColor = _labelText.color;
		}
	}

	private void OnDestroy()
	{
		_button.OnClick.RemoveAllListeners();
		_button.OnMouseover.RemoveAllListeners();
		_button.OnMouseoff.RemoveAllListeners();
	}

	public void SetActive(bool active)
	{
		_button.gameObject.SetActive(active);
	}

	public void SetCardCount(int count, int max = 15)
	{
		string key = ((count == 1) ? "MainNav/Draft/Sideboard_CardQuantity_Label_Singular" : "MainNav/Draft/Sideboard_CardQuantity_Label_Plural");
		Dictionary<string, string> parameters = new Dictionary<string, string> { 
		{
			"currentQuantity",
			count.ToString()
		} };
		_cardCountLabel.SetText(key, parameters);
		GetLabel();
		_labelText.color = ((count > max) ? _cardCountErrorColor : _cardCountDefaultColor);
	}

	private void OnMouseover()
	{
		SetExpandState(state: true);
	}

	private void OnExit()
	{
		SetExpandState(Expanded);
	}

	public void SetExpand(bool expand)
	{
		if (Expanded != expand)
		{
			ToggleExpand();
		}
	}

	private void OnEnable()
	{
		Expanded = false;
		SetExpandState(Expanded);
		_arrowAnimator.SetTrigger(ArrowExpandedHash, value: false);
		_expandOnHoverAnimator.SetTrigger(ExpandOnHover, _expandOnHover);
		SetDropState(on: false);
		Pantry.Get<MetaCardViewDragState>().OnDragStateChange += OnDragStateChange;
		VisualsUpdater.SideboardCountVisualsUpdated += SetCardCount;
		SetCardCount((int)Model.GetTotalSideboardSize(), Context.Format.MaxSideboardCards);
	}

	public void OnDisable()
	{
		Pantry.Get<MetaCardViewDragState>().OnDragStateChange -= OnDragStateChange;
		VisualsUpdater.SideboardCountVisualsUpdated -= SetCardCount;
	}

	private void OnDragStateChange(MetaCardView draggingCard)
	{
		bool flag = draggingCard != null;
		if (_dropStateOn != flag)
		{
			SetDropState(flag);
		}
	}

	private void ToggleExpand()
	{
		Expanded = !Expanded;
		OnExpandChange?.Invoke();
		SetExpandState(Expanded);
		_arrowAnimator.SetTrigger(ArrowExpandedHash, Expanded);
	}

	private void SetDropState(bool on)
	{
		_dropStateOn = on;
		_highlightAnimator.SetTrigger(Highlight, _dropStateOn);
	}

	private void SetExpandState(bool state)
	{
		_expandAnimator.SetBool(Expand, state);
		if ((bool)_contractAnimator)
		{
			_contractAnimator.SetTrigger(Expand, state);
		}
	}
}
