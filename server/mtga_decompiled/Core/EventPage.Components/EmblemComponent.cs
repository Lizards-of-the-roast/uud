using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;

namespace EventPage.Components;

public class EmblemComponent : EventComponent
{
	[SerializeField]
	private EventEmblem _eventEmblemPrefab;

	private List<EventEmblem> _eventEmblems = new List<EventEmblem>(2);

	public void SetEmblems(List<uint> emblemIDs, EventEmblem.eCardType cardType, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		foreach (EventEmblem eventEmblem2 in _eventEmblems)
		{
			Object.Destroy(eventEmblem2.gameObject);
		}
		_eventEmblems.Clear();
		ICardRolloverZoom cardZoomView = SceneLoader.GetSceneLoader().GetCardZoomView();
		foreach (uint emblemID in emblemIDs)
		{
			EventEmblem eventEmblem = Object.Instantiate(_eventEmblemPrefab, base.transform, worldPositionStays: false);
			eventEmblem.CardType = cardType;
			eventEmblem.Show(emblemID, cardDatabase, cardViewBuilder, cardZoomView);
			_eventEmblems.Add(eventEmblem);
		}
		if (_eventEmblems.Count > 0)
		{
			base.gameObject.SetActive(value: true);
		}
	}
}
