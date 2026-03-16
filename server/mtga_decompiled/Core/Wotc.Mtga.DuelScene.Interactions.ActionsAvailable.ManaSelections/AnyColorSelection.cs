using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public class AnyColorSelection : ManaSelection
{
	private readonly (Action, ManaPaymentOption)[] _paymentOptions = new(Action, ManaPaymentOption)[1];

	public override bool ShouldPrune => false;

	public override IEnumerable<ManaColor> SelectableColors => ManaUtilities.GetUncombinedColors(ManaColor.AnyColor);

	public override bool HasConstantCount => true;

	public override uint MaxColorCount => 1u;

	public override void Init(List<ManaGroup> manaGroups)
	{
		base.Init(manaGroups);
		foreach (ManaGroup manaGroup in manaGroups)
		{
			if (manaGroup.Contains(ManaColor.AnyColor))
			{
				_paymentOptions[0] = manaGroup.ManaAction;
				break;
			}
		}
	}

	public override uint CountForColor(ManaColor color)
	{
		return 1u;
	}

	public override (Action, ManaPaymentOption) GetManaAction()
	{
		(Action, ManaPaymentOption) manaAction = base.GetManaAction();
		ManaPaymentOption manaPaymentOption = new ManaPaymentOption(manaAction.Item2);
		manaPaymentOption.Mana.Clear();
		ManaInfo manaInfo = manaAction.Item2.Mana[0];
		for (int i = 0; i < manaInfo.Count; i++)
		{
			ManaInfo manaInfo2 = new ManaInfo(manaInfo);
			manaInfo2.Color = _selectionTotal[i];
			manaInfo2.Count = 1u;
			manaPaymentOption.Mana.Add(manaInfo2);
		}
		return (manaAction.Item1, manaPaymentOption);
	}
}
