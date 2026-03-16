using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Ability;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Decks.DeckValidation;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Arena.DeckValidation.Core.Models;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Providers;

public class CompanionUtil
{
	public static readonly Color WarningYellow = new Color(0.87f, 0.62f, 0.078f);

	private const uint YORION_TITLE_ID = 428610u;

	private readonly Dictionary<uint, CompanionValidity> _cachedCompanionValidity = new Dictionary<uint, CompanionValidity>();

	private static string _companionIconPath;

	public bool IsValid { get; private set; }

	public static CompanionUtil Create()
	{
		return new CompanionUtil();
	}

	private static bool TryGetCompanionAbility(CardPrintingData card, out AbilityPrintingData companionAbility)
	{
		if (card == null)
		{
			companionAbility = null;
			return false;
		}
		foreach (AbilityPrintingData item in card.Abilities ?? Array.Empty<AbilityPrintingData>())
		{
			if (item.BaseId == 202)
			{
				companionAbility = item;
				return true;
			}
		}
		companionAbility = null;
		return false;
	}

	public static bool CardCanBeCompanion(CardPrintingData card)
	{
		AbilityPrintingData companionAbility;
		return TryGetCompanionAbility(card, out companionAbility);
	}

	public static bool InvalidInFormat(ICardDatabaseAdapter cardDb, CardPrintingData companion, DeckFormat format, out MTGALocalizedString errorText)
	{
		if (!CardCanBeCompanion(companion))
		{
			errorText = new MTGALocalizedString
			{
				Key = "MainNav/DeckBuilder/CompanionError_NotCompanion",
				Parameters = new Dictionary<string, string> { 
				{
					"cardName",
					cardDb.GreLocProvider.GetLocalizedText(companion.TitleId)
				} }
			};
			return true;
		}
		if (format.IsCardBanned(companion.TitleId))
		{
			errorText = new MTGALocalizedString
			{
				Key = "MainNav/DeckBuilder/CompanionError_Illegal",
				Parameters = new Dictionary<string, string>
				{
					{
						"companionName",
						cardDb.GreLocProvider.GetLocalizedText(companion.TitleId)
					},
					{
						"formatName",
						format.GetLocalizedName()
					}
				}
			};
			return true;
		}
		if (companion.TitleId == 428610 && format.MaxMainDeckCards < format.MinMainDeckCards + 20)
		{
			errorText = new MTGALocalizedString
			{
				Key = "MainNav/DeckBuilder/CompanionError_FixedCards",
				Parameters = new Dictionary<string, string>
				{
					{
						"formatName",
						format.GetLocalizedName()
					},
					{
						"companionName",
						cardDb.GreLocProvider.GetLocalizedText(companion.TitleId)
					}
				}
			};
			return true;
		}
		errorText = null;
		return false;
	}

	public static string GetAbilityText(CardPrintingData companion, IGreLocProvider greLocProvider)
	{
		if (TryGetCompanionAbility(companion, out var companionAbility))
		{
			string text = greLocProvider.GetLocalizedText(companionAbility.TextId);
			string[] array = text.Split('—');
			if (array.Length > 1)
			{
				text = array[1].TrimStart();
			}
			return text;
		}
		return null;
	}

	public bool ShouldShowCompanionFilter()
	{
		return _cachedCompanionValidity.Keys.Any((uint tid) => tid != 428610);
	}

	public Func<CardPrintingData, bool> GetFilterForDeckBuilder(DeckBuilderContext context, DeckBuilderModel model)
	{
		Func<CardPrintingData, bool> result = null;
		CardPrintingData companion;
		if (context.Mode != DeckBuilderMode.ReadOnlyCollection)
		{
			companion = model.GetCompanion();
			result = ((companion != null) ? new Func<CardPrintingData, bool>(CompanionFilter) : null);
		}
		return result;
		bool CompanionFilter(CardPrintingData card)
		{
			return _cachedCompanionValidity.GetValueOrDefault(companion.TitleId)?.CardPoolFilter(new DeckValidationCardInfo(card, null)) ?? true;
		}
	}

	public bool IsDeckCardValid(CardPrintingData card, CardPrintingData companion, AssetLookupSystem altSystem, IGreLocProvider greLocProvider, out IEnumerable<AbilityHangerData> cardInvalidHanger)
	{
		bool flag = true;
		if (companion != null && _cachedCompanionValidity.TryGetValue(companion.TitleId, out var value))
		{
			flag = value.CardInDeckFilter(new DeckValidationCardInfo(card, null));
		}
		IEnumerable<AbilityHangerData> enumerable2;
		if (!flag)
		{
			IEnumerable<AbilityHangerData> enumerable = new AbilityHangerData[1]
			{
				new AbilityHangerData
				{
					Header = "AbilityHanger/DeckBuilder/CompanionViolation_Title",
					Body = new MTGALocalizedString
					{
						Key = "MainNav/DeckBuilder/CompanionSelection_Body",
						Parameters = new Dictionary<string, string> { 
						{
							"conditionText",
							GetAbilityText(companion, greLocProvider)
						} }
					},
					BadgePath = GetCompanionIconPath(companion, altSystem),
					Color = WarningYellow
				}
			};
			enumerable2 = enumerable;
		}
		else
		{
			enumerable2 = Enumerable.Empty<AbilityHangerData>();
		}
		cardInvalidHanger = enumerable2;
		return flag;
	}

	public static string GetCompanionIconPath(CardPrintingData companion, AssetLookupSystem altSystem)
	{
		if (_companionIconPath != null)
		{
			return _companionIconPath;
		}
		if (TryGetCompanionAbility(companion, out var companionAbility))
		{
			altSystem.Blackboard.Clear();
			altSystem.Blackboard.Ability = companionAbility;
			if (altSystem.TreeLoader.TryLoadTree(out AssetLookupTree<BadgeEntry> loadedTree))
			{
				BadgeEntry payload = loadedTree.GetPayload(altSystem.Blackboard);
				if (payload != null)
				{
					_companionIconPath = payload.Data.SpriteRef.RelativePath;
				}
			}
			altSystem.Blackboard.Clear();
		}
		return _companionIconPath;
	}

	public int GetMinMainDeckCards(DeckFormat deckFormat)
	{
		if (deckFormat == null)
		{
			return 0;
		}
		if (_cachedCompanionValidity.ContainsKey(428610u))
		{
			return deckFormat.MinMainDeckCards + 20;
		}
		return deckFormat.MinMainDeckCards;
	}

	public bool UpdateValidation(DeckBuilderModel model, DeckFormat deckFormat)
	{
		if (model == null)
		{
			return false;
		}
		if (model.GetCompanion() == null)
		{
			_cachedCompanionValidity.Clear();
			bool companionValid = (IsValid = false);
			model.SetCompanionValid(companionValid);
			return false;
		}
		IReadOnlyDictionary<uint, CompanionValidity> readOnlyDictionary = DeckValidationHelper.CalculateDeckCompanionValidity(deckFormat, model.GetServerModel(), Pantry.Get<ICardDatabaseAdapter>(), Pantry.Get<IEmergencyCardBansProvider>(), Pantry.Get<ISetMetadataProvider>(), Pantry.Get<CosmeticsProvider>(), Pantry.Get<DesignerMetadataProvider>());
		IsValid = readOnlyDictionary.All((KeyValuePair<uint, CompanionValidity> kvp) => kvp.Value.IsValid);
		bool flag2 = _cachedCompanionValidity.Count == readOnlyDictionary.Count;
		uint key;
		CompanionValidity value;
		if (flag2)
		{
			foreach (KeyValuePair<uint, CompanionValidity> item in readOnlyDictionary)
			{
				item.Deconstruct(out key, out value);
				uint key2 = key;
				CompanionValidity other = value;
				flag2 &= _cachedCompanionValidity.TryGetValue(key2, out var value2) && value2.Equals(other);
				if (!flag2)
				{
					break;
				}
			}
		}
		_cachedCompanionValidity.Clear();
		foreach (KeyValuePair<uint, CompanionValidity> item2 in readOnlyDictionary)
		{
			item2.Deconstruct(out key, out value);
			uint key3 = key;
			CompanionValidity value3 = value;
			_cachedCompanionValidity.Add(key3, value3);
		}
		model.SetCompanionValid(IsValid);
		return !flag2;
	}
}
