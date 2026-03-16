using System;
using System.Collections;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;

public class MOZTutorialPopup : PopupBase
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private Transform _cardAnchor;

	[SerializeField]
	private int _cardToSpawn;

	[SerializeField]
	private TMP_Text _delayedText;

	[SerializeField]
	private BoosterMetaCardHolder _cardHolder;

	[SerializeField]
	private CustomButton _mainButton;

	[SerializeField]
	private RuntimeAnimatorController _emphasisController;

	[SerializeField]
	private float _secondsBeforeEnabled;

	private ICardRolloverZoom _zoomHandler;

	private bool _backgroundTriggersClick;

	private CardViewBuilder _viewBuilder;

	private CardDatabase _cardDb;

	private Action _onComplete;

	private string _userID = string.Empty;

	private bool initalized;

	public static bool HasSeenMOZTutorial()
	{
		if (OverridesConfiguration.Local.GetFeatureToggleValue("mobile.mozTutorialAlwaysAppear"))
		{
			return false;
		}
		AccountInformation accountInformation = Pantry.Get<IAccountClient>()?.AccountInformation;
		if (accountInformation == null)
		{
			return true;
		}
		return MDNPlayerPrefs.GetHasSeenHandheldMOZTutorial(accountInformation.PersonaID);
	}

	private void OnDestroy()
	{
		ICardRolloverZoom zoomHandler = _zoomHandler;
		zoomHandler.OnRolloff = (Action<Meta_CDC>)Delegate.Remove(zoomHandler.OnRolloff, new Action<Meta_CDC>(OnRolloff));
	}

	private void OnRolloff(Meta_CDC obj)
	{
		if (obj.Model.GrpId == _cardToSpawn)
		{
			EnableButtons();
		}
	}

	private void EnableButtons()
	{
		_backgroundTriggersClick = true;
		_delayedText.gameObject.SetActive(value: true);
		_mainButton.Interactable = true;
	}

	public void Init(CardViewBuilder viewBuilder, CardDatabase cardDb, string userId, Action onComplete)
	{
		_viewBuilder = viewBuilder;
		_cardDb = cardDb;
		_userID = userId;
		_onComplete = (Action)Delegate.Combine(_onComplete, onComplete);
		initalized = true;
		_cardHolder.EnsureInit(cardDb, viewBuilder);
		_zoomHandler = SceneLoader.GetSceneLoader().GetCardZoomView();
		_cardHolder.RolloverZoomView = _zoomHandler;
		_cardHolder.ShowHighlight = (MetaCardView cardView) => true;
		_delayedText.gameObject.SetActive(value: false);
		ICardRolloverZoom zoomHandler = _zoomHandler;
		zoomHandler.OnRolloff = (Action<Meta_CDC>)Delegate.Combine(zoomHandler.OnRolloff, new Action<Meta_CDC>(OnRolloff));
		_mainButton.Interactable = false;
		_animator.gameObject.SetActive(value: true);
		_animator.enabled = true;
		StartCoroutine(DelayedEnable());
	}

	private IEnumerator DelayedEnable()
	{
		yield return new WaitForSeconds(_secondsBeforeEnabled);
		EnableButtons();
	}

	protected override void Show()
	{
		if (initalized)
		{
			base.Show();
			MDNPlayerPrefs.SetHasSeenHandheldMOZTutorial(_userID, value: true);
			CardPrintingData cardPrintingById = _cardDb.CardDataProvider.GetCardPrintingById((uint)_cardToSpawn);
			if (cardPrintingById != null)
			{
				CDCMetaCardView cDCMetaCardView = _viewBuilder.CreateCDCMetaCardView(new CardData(null, cardPrintingById), _cardAnchor);
				cDCMetaCardView.Holder = _cardHolder;
				cDCMetaCardView.CardView.gameObject.AddComponent<Animator>();
				cDCMetaCardView.CardView.GetComponent<Animator>().runtimeAnimatorController = _emphasisController;
			}
		}
		else
		{
			Hide();
		}
	}

	public void OnBackgroundClicked_Unity()
	{
		if (_backgroundTriggersClick)
		{
			Hide();
		}
	}

	protected override void Hide()
	{
		_onComplete?.Invoke();
		base.Hide();
	}

	public override void OnEscape()
	{
		if (_backgroundTriggersClick)
		{
			Hide();
		}
		else
		{
			Emphasize();
		}
	}

	private void Emphasize()
	{
		_animator.SetTrigger("Emphasize");
	}

	public override void OnEnter()
	{
	}
}
