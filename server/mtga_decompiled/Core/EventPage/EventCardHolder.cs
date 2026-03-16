using System;
using System.Collections.Generic;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace EventPage;

public class EventCardHolder : MetaCardHolder
{
	private List<BoosterMetaCardView> _allCardViews = new List<BoosterMetaCardView>();

	public bool ForceMouseOver = true;

	public override void ClearCards()
	{
		_allCardViews.Clear();
	}

	public override void EnsureInit(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		base.RolloverZoomView = SceneLoader.GetSceneLoader().GetCardZoomView();
		ICardRolloverZoom rolloverZoomView = base.RolloverZoomView;
		rolloverZoomView.OnRollover = (Action<Meta_CDC>)Delegate.Combine(rolloverZoomView.OnRollover, new Action<Meta_CDC>(OnRollover));
		base.EnsureInit(cardDatabase, cardViewBuilder);
	}

	private void OnRollover(Meta_CDC obj)
	{
		if (ForceMouseOver)
		{
			obj.ModelOverride = new ModelOverride(null, ZoneType.Library, null, null);
			obj.HolderTypeOverride = CardHolderType.Library;
			obj.IsMousedOver = true;
		}
	}
}
