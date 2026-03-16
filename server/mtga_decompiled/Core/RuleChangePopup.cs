using System;
using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

public class RuleChangePopup : PopupBase
{
	[SerializeField]
	private List<Transform> cardsAnchor;

	[SerializeField]
	private List<int> cardsToSpawn;

	[SerializeField]
	private CustomButton okayButton;

	private CardViewBuilder _viewBuilder;

	private CardDatabase _cardDb;

	private Action _onComplete;

	private string _userID = string.Empty;

	private bool initalized;

	public const string ruleViewed = "COMPANION";

	private void OnEnable()
	{
		okayButton.OnClick.AddListener(OnClick);
		okayButton.OnMouseover.AddListener(OnMouseOver);
	}

	private void OnDisable()
	{
		okayButton.OnClick.RemoveListener(OnClick);
		okayButton.OnMouseover.RemoveListener(OnMouseOver);
	}

	private void OnClick()
	{
		Hide();
	}

	private void OnMouseOver()
	{
	}

	public void Init(CardViewBuilder viewBuilder, CardDatabase cardDb, string userId, Action onComplete)
	{
		_viewBuilder = viewBuilder;
		_cardDb = cardDb;
		_userID = userId;
		_onComplete = (Action)Delegate.Combine(_onComplete, onComplete);
		initalized = true;
	}

	protected override void Show()
	{
		if (initalized)
		{
			base.Show();
			MDNPlayerPrefs.SetRuleChangeViewed(_userID, "COMPANION", value: true);
			for (int i = 0; i < cardsToSpawn.Count; i++)
			{
				int id = cardsToSpawn[i];
				CardPrintingData cardPrintingById = _cardDb.CardDataProvider.GetCardPrintingById((uint)id);
				if (cardPrintingById != null)
				{
					_viewBuilder.CreateCDCMetaCardView(new CardData(null, cardPrintingById), cardsAnchor[i]);
				}
			}
		}
		else
		{
			Hide();
		}
	}

	protected override void Hide()
	{
		Action onComplete = _onComplete;
		if (onComplete != null)
		{
			onComplete?.Invoke();
		}
		base.Hide();
	}

	public override void OnEscape()
	{
		Hide();
	}

	public override void OnEnter()
	{
		Hide();
	}
}
