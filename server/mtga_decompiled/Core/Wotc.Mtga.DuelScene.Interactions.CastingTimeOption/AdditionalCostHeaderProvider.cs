using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.CastingTimeOption;

public class AdditionalCostHeaderProvider : ICastTimeOptionHeaderProvider
{
	private class BlightHeaderProvider : ISubHeaderProvider
	{
		private readonly IClientLocProvider _locProvider;

		public BlightHeaderProvider(IClientLocProvider locProvider)
		{
			_locProvider = locProvider;
		}

		public BrowserCardHeader.BrowserCardHeaderData GetCastTimeOptionHeader(MtgCastTimeOption castTimeOption, AbilityPrintingData abilityPrintingData)
		{
			if (!abilityPrintingData.ReferencedAbilityTypes.Contains(AbilityType.Blight) || !TryGetBodyKey(abilityPrintingData, out var result))
			{
				return null;
			}
			return new BrowserCardHeader.BrowserCardHeaderData(_locProvider.GetLocalizedText("DuelScene/Browsers/BrowserCardInfo_CastWith"), _locProvider.GetLocalizedText(result));
		}

		private bool TryGetBodyKey(AbilityPrintingData abilityPrintingData, out string result)
		{
			if (IsBlight1(abilityPrintingData))
			{
				result = "AbilityHanger/Keyword/Blight1_Title";
				return true;
			}
			if (IsBlight2(abilityPrintingData))
			{
				result = "AbilityHanger/Keyword/Blight2_Title";
				return true;
			}
			if (IsBlightX(abilityPrintingData))
			{
				result = "AbilityHanger/Keyword/BlightX_Title";
				return true;
			}
			result = null;
			return false;
		}

		private bool IsBlight1(AbilityPrintingData abilityPrintingData)
		{
			return abilityPrintingData.BaseIdNumeral == 1;
		}

		private bool IsBlight2(AbilityPrintingData abilityPrintingData)
		{
			return abilityPrintingData.BaseIdNumeral == 2;
		}

		private bool IsBlightX(AbilityPrintingData abilityPrintingData)
		{
			return abilityPrintingData.FakeBaseIdNumeral == 391;
		}
	}

	private class ByAbilityGrpIdHeaderProvider : ISubHeaderProvider
	{
		private readonly IClientLocProvider _locProvider;

		private readonly uint[] ABILITY_GRPIDS = new uint[1] { 194000u };

		public ByAbilityGrpIdHeaderProvider(IClientLocProvider locProvider)
		{
			_locProvider = locProvider;
		}

		public BrowserCardHeader.BrowserCardHeaderData GetCastTimeOptionHeader(MtgCastTimeOption castTimeOption, AbilityPrintingData abilityPrintingData)
		{
			if (abilityPrintingData.Category != AbilityCategory.AdditionalCost)
			{
				return null;
			}
			if (Array.IndexOf(ABILITY_GRPIDS, castTimeOption.GrpId) < 0)
			{
				return null;
			}
			return new BrowserCardHeader.BrowserCardHeaderData(_locProvider.GetLocalizedText("DuelScene/Browsers/BrowserCardInfo_CastWith"), _locProvider.GetLocalizedText("DuelScene/FaceHanger/AdditionalCost"));
		}
	}

	private class GiftHeaderProvider : ISubHeaderProvider
	{
		private const uint GIFT_CARD_TRIGGERED_TEXTID = 820499u;

		private const uint GIFT_CARD_REGULAR_TEXTID = 810963u;

		private const uint GIFT_FISH_TRIGGERED_TEXTID = 820507u;

		private const uint GIFT_FISH_REGULARD_TEXTID = 811023u;

		private readonly Dictionary<uint, uint> _giftTextIdReplacements = new Dictionary<uint, uint>
		{
			[820499u] = 810963u,
			[820507u] = 811023u
		};

		private readonly ICardDatabaseAdapter _cardDatabase;

		public GiftHeaderProvider(ICardDatabaseAdapter cardDatabase)
		{
			_cardDatabase = cardDatabase;
		}

		public BrowserCardHeader.BrowserCardHeaderData GetCastTimeOptionHeader(MtgCastTimeOption castTimeOption, AbilityPrintingData abilityPrintingData)
		{
			if (abilityPrintingData.BaseId != 342)
			{
				return null;
			}
			uint value;
			return new BrowserCardHeader.BrowserCardHeaderData(_giftTextIdReplacements.TryGetValue(abilityPrintingData.TextId, out value) ? _cardDatabase.GreLocProvider.GetLocalizedText(value) : _cardDatabase.GreLocProvider.GetLocalizedText(abilityPrintingData.TextId), null);
		}
	}

	private interface ISubHeaderProvider
	{
		BrowserCardHeader.BrowserCardHeaderData GetCastTimeOptionHeader(MtgCastTimeOption castTimeOption, AbilityPrintingData abilityPrintingData);

		bool TryGetCastTimeOptionHeader(MtgCastTimeOption castTimeOption, AbilityPrintingData abilityPrintingData, out BrowserCardHeader.BrowserCardHeaderData cardHeaderData)
		{
			cardHeaderData = GetCastTimeOptionHeader(castTimeOption, abilityPrintingData);
			return cardHeaderData != null;
		}
	}

	private class OffspringHeaderProvider : ISubHeaderProvider
	{
		private readonly ICardDatabaseAdapter _cardDatabase;

		private readonly IGameStateProvider _gameStateProvider;

		public OffspringHeaderProvider(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider)
		{
			_cardDatabase = cardDatabase;
			_gameStateProvider = gameStateProvider;
		}

		public BrowserCardHeader.BrowserCardHeaderData GetCastTimeOptionHeader(MtgCastTimeOption castTimeOption, AbilityPrintingData abilityPrintingData)
		{
			if (abilityPrintingData.BaseId != 341)
			{
				return null;
			}
			if (!((MtgGameState)_gameStateProvider.LatestGameState).TryGetCard(castTimeOption.AffectedId, out var card))
			{
				return null;
			}
			return new BrowserCardHeader.BrowserCardHeaderData(_cardDatabase.ClientLocProvider.GetLocalizedText("DuelScene/Browsers/BrowserCardInfo_CastWith"), _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(abilityPrintingData.Id, abilityPrintingData.BaseId, card.Abilities.Select((AbilityPrintingData x) => x.Id), card.TitleId) + ManaUtilities.ConvertManaSymbols(abilityPrintingData.OldSchoolManaText));
		}
	}

	private class WaterbendHeaderProvider : ISubHeaderProvider
	{
		private readonly IClientLocProvider _locProvider;

		public WaterbendHeaderProvider(IClientLocProvider locProvider)
		{
			_locProvider = locProvider;
		}

		public BrowserCardHeader.BrowserCardHeaderData GetCastTimeOptionHeader(MtgCastTimeOption castTimeOption, AbilityPrintingData abilityPrintingData)
		{
			if (!abilityPrintingData.ReferencedAbilityTypes.Contains(AbilityType.Waterbend))
			{
				return null;
			}
			if (abilityPrintingData.Category != AbilityCategory.AdditionalCost)
			{
				return null;
			}
			return new BrowserCardHeader.BrowserCardHeaderData(_locProvider.GetLocalizedText("DuelScene/Browsers/BrowserCardInfo_CastWith"), _locProvider.GetLocalizedText("AbilityHanger/Keyword/Waterbend_Title"));
		}
	}

	private readonly IAbilityDataProvider _abilityDataProvider;

	private readonly IReadOnlyList<ISubHeaderProvider> _subHeaderProviders;

	public AdditionalCostHeaderProvider(ICardDatabaseAdapter cardDatabase, IGameStateProvider gameStateProvider)
	{
		_abilityDataProvider = cardDatabase.AbilityDataProvider;
		_subHeaderProviders = new List<ISubHeaderProvider>
		{
			new BlightHeaderProvider(cardDatabase.ClientLocProvider),
			new GiftHeaderProvider(cardDatabase),
			new OffspringHeaderProvider(cardDatabase, gameStateProvider),
			new WaterbendHeaderProvider(cardDatabase.ClientLocProvider),
			new ByAbilityGrpIdHeaderProvider(cardDatabase.ClientLocProvider)
		};
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCastTimeOptionHeader(MtgCastTimeOption castTimeOption)
	{
		if (!_abilityDataProvider.TryGetAbilityPrintingById(castTimeOption.GrpId, out var ability))
		{
			return null;
		}
		foreach (ISubHeaderProvider subHeaderProvider in _subHeaderProviders)
		{
			if (subHeaderProvider.TryGetCastTimeOptionHeader(castTimeOption, ability, out var cardHeaderData))
			{
				return cardHeaderData;
			}
		}
		return null;
	}
}
