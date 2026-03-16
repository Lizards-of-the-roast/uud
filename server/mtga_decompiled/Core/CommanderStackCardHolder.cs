using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;

public class CommanderStackCardHolder : MonoBehaviour
{
	[SerializeField]
	public CommanderSlotCardHolder CommanderSlotCardHolder;

	[SerializeField]
	public CommanderSlotCardHolder PartnerSlotCardHolder;

	public void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, ICardRolloverZoom zoomHandler)
	{
		CommanderSlotCardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		CommanderSlotCardHolder.RolloverZoomView = zoomHandler;
		PartnerSlotCardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		PartnerSlotCardHolder.RolloverZoomView = zoomHandler;
	}

	public void SetActive(bool isActive)
	{
		CommanderSlotCardHolder.SetActive(isActive);
		PartnerSlotCardHolder.SetActive(isActive);
	}

	public void SetCard(ListMetaCardViewDisplayInformation di, bool ignoreErrorStates, DeckBuilderPile pile)
	{
		CommanderSlotCardHolder commanderSlotCardHolder = ((pile == DeckBuilderPile.Commander) ? CommanderSlotCardHolder : PartnerSlotCardHolder);
		if (di.Card != null)
		{
			commanderSlotCardHolder.SetCard(di, ignoreErrorStates);
		}
		else
		{
			commanderSlotCardHolder.ClearCards();
		}
	}

	public void ClearCard(DeckBuilderPile pile)
	{
		((pile == DeckBuilderPile.Commander) ? CommanderSlotCardHolder : PartnerSlotCardHolder).ClearCards();
	}
}
