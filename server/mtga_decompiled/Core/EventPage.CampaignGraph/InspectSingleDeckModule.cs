using UnityEngine;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Extensions;

namespace EventPage.CampaignGraph;

public class InspectSingleDeckModule : EventModule
{
	[SerializeField]
	private MetaDeckView _metaDeckView;

	protected override Animator Animator
	{
		get
		{
			if (_transitionAnimator == null)
			{
				_transitionAnimator = _metaDeckView.GetComponent<Animator>();
			}
			return _transitionAnimator;
		}
	}

	private void Awake()
	{
		_metaDeckView.Button.OnMouseover.AddListener(InspectDeckBoxesHovered);
		_metaDeckView.Button.OnClick.AddListener(InspectDecksButtonClicked);
	}

	public override void Show()
	{
		if (base.EventContext.PlayerEvent.CourseData.CourseDeck != null)
		{
			base.gameObject.UpdateActive(active: true);
			Client_Deck courseDeck = base.EventContext.PlayerEvent.CourseData.CourseDeck;
			_metaDeckView.Init(_cardDatabase, _cardViewBuilder, courseDeck);
			_metaDeckView.SetIsValid(DeckDisplayInfo.DeckDisplayState.Valid);
		}
	}

	public override void UpdateModule()
	{
	}

	public override void Hide()
	{
		base.gameObject.UpdateActive(active: false);
	}

	private void InspectDeckBoxesHovered()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_rollover_deck, AudioManager.Default);
	}

	private void InspectDecksButtonClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deckbuilding_box_open, AudioManager.Default);
		DeckBuilderContext deckBuilderContext = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(_metaDeckView.Model), base.EventContext, sideboarding: false, firstEdit: false, DeckBuilderMode.ReadOnly);
		deckBuilderContext.Format = WrapperController.Instance.FormatManager.GetSafeFormat(base.EventContext.PlayerEvent.EventUXInfo.DeckSelectFormat);
		SceneLoader.GetSceneLoader().GoToDeckBuilder(deckBuilderContext);
	}
}
