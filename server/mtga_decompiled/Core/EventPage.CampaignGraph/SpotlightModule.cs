using UnityEngine;
using Wotc.Mtga.Extensions;

namespace EventPage.CampaignGraph;

public class SpotlightModule : EventModule
{
	[SerializeField]
	private EventEmblem _eventEmblemPrefab;

	private EventEmblem _eventEmblem;

	private string _eventName;

	public override void Show()
	{
		if (!base.EventContext.PlayerEvent.InPlayingMatchesModule)
		{
			Hide();
		}
		else
		{
			_instantiateEmblems();
		}
	}

	private void _instantiateEmblems()
	{
		if (base.EventContext.PlayerEvent.EventInfo.InternalEventName != _eventName)
		{
			_eventName = base.EventContext.PlayerEvent.EventInfo.InternalEventName;
			if (_eventEmblem != null)
			{
				Object.Destroy(_eventEmblem.gameObject);
				_eventEmblem = null;
			}
			string colorChallengeMatchPath = ClientEventDefinitionList.GetColorChallengeMatchPath(_assetLookupSystem, base.EventContext);
			if (!string.IsNullOrEmpty(colorChallengeMatchPath))
			{
				ColorChallengeMatch objectData = AssetLoader.GetObjectData<ColorChallengeMatch>(colorChallengeMatchPath);
				_eventEmblem = Object.Instantiate(_eventEmblemPrefab, base.transform, worldPositionStays: false);
				_eventEmblem.transform.localScale = Vector3.one;
				_eventEmblem.CardType = EventEmblem.eCardType.SkinCard;
				_eventEmblem.Show(objectData.FeaturedGRPID, WrapperController.Instance.CardDatabase, WrapperController.Instance.CardViewBuilder, SceneLoader.GetSceneLoader().GetCardZoomView());
			}
		}
		if (_eventEmblem != null)
		{
			base.gameObject.UpdateActive(active: true);
		}
	}

	public override void UpdateModule()
	{
		if (base.EventContext.PlayerEvent.InPlayingMatchesModule)
		{
			_instantiateEmblems();
		}
	}

	public override void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
