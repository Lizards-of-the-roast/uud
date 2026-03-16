using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class TraitFilter : CardPropertyFilter
{
	public enum TraitFilterType
	{
		TraditionalBasicLand,
		Commanders,
		Companions,
		Booster,
		Adventure,
		Omen,
		Transform,
		Split,
		Spell,
		Permanent,
		Styled,
		DualFaced,
		Party,
		Rebalanced
	}

	public TraitFilterType Trait;

	public TraitFilter()
	{
	}

	public TraitFilter(string str)
	{
		if (str == "BASICLAND")
		{
			Trait = TraitFilterType.TraditionalBasicLand;
		}
		if (str == "COMMANDER")
		{
			Trait = TraitFilterType.Commanders;
		}
		if (str == "COMPANION")
		{
			Trait = TraitFilterType.Companions;
		}
		if (str == "BOOSTER")
		{
			Trait = TraitFilterType.Booster;
		}
		if (str == "TRANSFORM")
		{
			Trait = TraitFilterType.Transform;
		}
		if (str == "SPLIT")
		{
			Trait = TraitFilterType.Split;
		}
		if (str == "DUALFACED")
		{
			Trait = TraitFilterType.DualFaced;
		}
		if (str == "PARTY")
		{
			Trait = TraitFilterType.Party;
		}
		if (str == "ADVENTURE")
		{
			Trait = TraitFilterType.Adventure;
		}
		if (str == "OMEN")
		{
			Trait = TraitFilterType.Omen;
		}
		if (str == "SPELL")
		{
			Trait = TraitFilterType.Spell;
		}
		if (str == "PERMANENT")
		{
			Trait = TraitFilterType.Permanent;
		}
		if (str == "STYLED")
		{
			Trait = TraitFilterType.Styled;
		}
		if (str == "REBALANCED")
		{
			Trait = TraitFilterType.Rebalanced;
		}
	}

	private bool IsInBooster(string cn_string, string cmax_string, bool? draftContent)
	{
		if (draftContent.HasValue)
		{
			return draftContent.Value;
		}
		if (!int.TryParse(cn_string, out var result))
		{
			return false;
		}
		if (!int.TryParse(cmax_string, out var result2))
		{
			return false;
		}
		if (result2 == 0)
		{
			return false;
		}
		return result <= result2;
	}

	public override CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = Trait switch
			{
				TraitFilterType.TraditionalBasicLand => cards.Cards[num].Card.IsBasicLand, 
				TraitFilterType.Commanders => DeckFormat.CardCanBeCommander(cards.Cards[num].Card), 
				TraitFilterType.Companions => CompanionUtil.CardCanBeCompanion(cards.Cards[num].Card), 
				TraitFilterType.Booster => IsInBooster(cards.Cards[num].Card.CollectorNumber, cards.Cards[num].Card.CollectorMax, cards.Cards[num].Card.DraftContent), 
				TraitFilterType.Transform => cards.Cards[num].Card.LinkedFaceType == LinkedFace.DfcBack, 
				TraitFilterType.Split => cards.Cards[num].Card.LinkedFaceType == LinkedFace.SplitHalf, 
				TraitFilterType.Adventure => cards.Cards[num].Card.LinkedFaceType == LinkedFace.AdventureChild, 
				TraitFilterType.Omen => cards.Cards[num].Card.LinkedFaceType == LinkedFace.OmenChild, 
				TraitFilterType.Spell => !cards.Cards[num].Card.Types.Contains(CardType.Land), 
				TraitFilterType.Permanent => !cards.Cards[num].Card.Types.Contains(CardType.Instant) && !cards.Cards[num].Card.Types.Contains(CardType.Sorcery), 
				TraitFilterType.Styled => cards.Cards[num].Card.KnownSupportedStyles.Count > 0, 
				TraitFilterType.DualFaced => CardUtilities.IsBackFaceOfDoubleSided(cards.Cards[num].Card.LinkedFaceType), 
				TraitFilterType.Party => cards.Cards[num].Card.Subtypes.Contains(SubType.Rogue) || cards.Cards[num].Card.Subtypes.Contains(SubType.Wizard) || cards.Cards[num].Card.Subtypes.Contains(SubType.Cleric) || cards.Cards[num].Card.Subtypes.Contains(SubType.Warrior) || cards.Cards[num].Card.Abilities.Exists((AbilityPrintingData x) => x.Id == 138540), 
				TraitFilterType.Rebalanced => cards.Cards[num].Card.IsRebalanced, 
				_ => false, 
			};
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.TraitFilter);
			}
		}
		return cards;
	}
}
