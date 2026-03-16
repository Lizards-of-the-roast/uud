using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class RelativeAbilityComparer : IModalActionComparer, IComparer<Action>
{
	private ICardDataAdapter _cardModel;

	public void SetCompareParams(ICardDataAdapter cardModel)
	{
		_cardModel = cardModel;
	}

	public void ClearCompareParams()
	{
		_cardModel = null;
	}

	public int Compare(Action x, Action y)
	{
		if (_cardModel == null)
		{
			return 0;
		}
		uint data = RelativeAbilityGrpId(x.AbilityGrpId, _cardModel.PrintingAbilities);
		uint data2 = RelativeAbilityGrpId(y.AbilityGrpId, _cardModel.PrintingAbilities);
		int num = _cardModel.PrintingAbilities.FindIndex(data, (AbilityPrintingData ability, uint t) => ability.Id == t);
		int num2 = _cardModel.PrintingAbilities.FindIndex(data2, (AbilityPrintingData ability, uint t) => ability.Id == t);
		if (num != -1 && num2 != -1 && num == num2)
		{
			if (x.AbilityGrpId != y.AbilityGrpId)
			{
				return x.AbilityGrpId.CompareTo(y.AbilityGrpId);
			}
			return 0;
		}
		if (num == -1 && num2 != -1)
		{
			return 1;
		}
		if (num2 == -1 && num != -1)
		{
			return -1;
		}
		if (num == -1 && num2 == -1)
		{
			data = RelativeAbilityGrpId(x.AbilityGrpId, _cardModel.Abilities);
			data2 = RelativeAbilityGrpId(y.AbilityGrpId, _cardModel.Abilities);
			num = _cardModel.Abilities.FindIndex(data, (AbilityPrintingData ability, uint xId) => ability.Id == xId);
			num2 = _cardModel.Abilities.FindIndex(data2, (AbilityPrintingData ability, uint yId) => ability.Id == yId);
			if (num != -1 && num2 != -1 && num == num2 && x.AbilityGrpId != y.AbilityGrpId)
			{
				return x.AbilityGrpId.CompareTo(y.AbilityGrpId);
			}
		}
		return num.CompareTo(num2);
	}

	private uint RelativeAbilityGrpId(uint abilityGrpId, IEnumerable<AbilityPrintingData> abilities)
	{
		foreach (AbilityPrintingData ability in abilities)
		{
			if (abilityGrpId - 243 <= 1 && ability.BaseId == 237)
			{
				return ability.Id;
			}
		}
		return abilityGrpId;
	}
}
