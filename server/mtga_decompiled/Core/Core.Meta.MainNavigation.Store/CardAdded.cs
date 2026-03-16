using Wizards.Mtga.FrontDoorModels;

namespace Core.Meta.MainNavigation.Store;

public class CardAdded
{
	public uint GrpID;

	public CardRarity ExpectedRarity;

	public AetherizedCardInformation AetherizedInfo;

	public uint count;

	public bool IsPartOfAlchemyPair => count < 1;

	public bool IsGemCard()
	{
		if (AetherizedInfo != null)
		{
			return AetherizedInfo.gemsAwarded > 0;
		}
		return false;
	}

	public bool IsDisplayableCard()
	{
		if (AetherizedInfo == null)
		{
			return true;
		}
		return !AetherizedInfo.isGrantedFromDeck;
	}

	public bool ShouldDisplay()
	{
		if (!IsGemCard())
		{
			return IsDisplayableCard();
		}
		return true;
	}
}
