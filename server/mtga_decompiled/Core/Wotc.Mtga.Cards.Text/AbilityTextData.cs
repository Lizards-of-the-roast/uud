using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Card.RulesText;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Text;

public class AbilityTextData
{
	private const string LOC_PARAM_ABILITY_TEXT = "abilityText";

	private const string LOC_PARAM_COUNT = "count";

	private static readonly AbilityTextComparer _abilityTextComparer = new AbilityTextComparer();

	private static readonly LandAbilityReplacer _landAbilityReplacer = new LandAbilityReplacer();

	private static readonly StringBuilder _sharedCachedStringBuilder = new StringBuilder(100);

	public AbilityPrintingData Printing;

	public AbilityState State;

	public string RawLocalizedText;

	public string FormattedLocalizedText;

	public bool IsKeyword;

	public bool IsGroupable;

	public bool OmitDuplicates;

	public bool IsPerpetual;

	public bool IsMutation;

	public int Count = 1;

	public List<uint> Chapters;

	public bool HideText;

	public StationAbilityData StationData;

	public LevelUpAbilityData LevelUpData;

	private static readonly IObjectPool _sharedObjPool = new ObjectPool();

	public bool IsLoyaltyAbility
	{
		get
		{
			if (Printing != null)
			{
				return !string.IsNullOrEmpty(Printing.LoyaltyCost.RawText);
			}
			return false;
		}
	}

	public bool IsChapterAbility
	{
		get
		{
			if (Printing != null)
			{
				return Printing.BaseId == 166;
			}
			return false;
		}
	}

	public bool IsFuseAbility
	{
		get
		{
			AbilityPrintingData printing = Printing;
			if (printing != null)
			{
				return printing.Id == 103;
			}
			return false;
		}
	}

	public bool IsClassLevelAbility
	{
		get
		{
			if (Printing != null)
			{
				return Printing.SubCategory == AbilitySubCategory.ClassLevel;
			}
			return false;
		}
	}

	public bool IsD20Ability => (Printing?.ReferencedAbilityTypes?.Contains(AbilityType.RollD20)).Value;

	public bool IsIntensityAbility
	{
		get
		{
			AbilityPrintingData printing = Printing;
			if (printing == null)
			{
				return false;
			}
			return printing.BaseId == 249;
		}
	}

	public bool IsToSolveAbility
	{
		get
		{
			AbilityPrintingData printing = Printing;
			if (printing == null)
			{
				return false;
			}
			return printing.SubCategory == AbilitySubCategory.ToSolveCase;
		}
	}

	public bool IsSolvedAbility
	{
		get
		{
			AbilityPrintingData printing = Printing;
			if (printing == null)
			{
				return false;
			}
			return printing.SubCategory == AbilitySubCategory.SolvedCase;
		}
	}

	public bool IsStationAbility => StationData.ParentAbilityId != 0;

	public bool IsLevelUpAbility => !LevelUpData.Equals(default(LevelUpAbilityData));

	public AbilityTextData Clone()
	{
		return (AbilityTextData)MemberwiseClone();
	}

	public static void ProcessAbilityTextDatas(List<AbilityTextData> abilityTextDatas, ICardDataAdapter cardData, CardTextColorSettings colorSettings, Func<uint, string> GetAbilityText, Func<ICardDataAdapter, AbilityPrintingData, bool, string> GetFormatForModalChildAbility, ICardDatabaseAdapter cardDB, AssetLookupSystem assetLookupSystem, string groupedAbilitySeparator = ", ")
	{
		FormatRoomChildAbilities(cardDB, cardData, abilityTextDatas, colorSettings, GetAbilityText);
		SortAbilities(abilityTextDatas, cardData);
		CheckAbilityDisplayOverride(abilityTextDatas, cardData, assetLookupSystem);
		_landAbilityReplacer.Replace(abilityTextDatas, cardDB.AbilityDataProvider, GetAbilityText, colorSettings);
		ColorizeModalAbilityText(abilityTextDatas, cardData, GetAbilityText, GetFormatForModalChildAbility);
		RearrangeForBackupMechanic(abilityTextDatas);
		RemoveDuplicateAbilities(abilityTextDatas);
		CollapsePerpetualAbilities(abilityTextDatas, cardDB.ClientLocProvider, colorSettings);
		CollapseAddedAbilities(abilityTextDatas, cardDB.ClientLocProvider, colorSettings);
		FormatStationGrantedAbilities(abilityTextDatas, cardData, colorSettings, groupedAbilitySeparator, GetAbilityText);
		FormatLevelUpGrantedAbilities(abilityTextDatas, cardData, colorSettings, groupedAbilitySeparator, GetAbilityText);
		GroupAbilityText(abilityTextDatas, groupedAbilitySeparator, colorSettings);
		CondenseChapterAbilities(abilityTextDatas, cardData);
		FormatChapterAbilities(abilityTextDatas, cardData, colorSettings);
		FormatClassGrantedAbilities(abilityTextDatas, cardData, colorSettings, GetAbilityText);
		UnspoolPerpetualCTOAbilitiesOnTheStack(cardData, abilityTextDatas, cardDB.ClientLocProvider, colorSettings);
		FormatAlternateCostsOnTheStack(cardData, abilityTextDatas, colorSettings, cardDB.AbilityDataProvider, cardDB.ClientLocProvider);
		FormatIntensityAbilities(cardData, abilityTextDatas, colorSettings, cardDB.ClientLocProvider);
		FormatCaseAbilities(cardData, abilityTextDatas, colorSettings);
		FormatFuseAbilities(cardData, abilityTextDatas, string.Format(colorSettings.DefaultFormat, groupedAbilitySeparator));
	}

	private static void SortAbilities(List<AbilityTextData> abilityTextDatas, ICardDataAdapter cardData)
	{
		int startIndex = Math.Max(0, abilityTextDatas.FindIndex((AbilityTextData x) => x.Printing.BaseId == 287));
		int num = abilityTextDatas.FindIndex(startIndex, (AbilityTextData x) => x.IsKeyword && x.IsGroupable && x.State == AbilityState.Normal);
		AbilityTextData anchorKeyword = ((num > -1) ? abilityTextDatas[num] : null);
		_abilityTextComparer.SetParams(anchorKeyword, cardData?.AllAbilities, cardData?.PrintingAbilities, cardData?.Instance?.MutationChildren, abilityTextDatas);
		abilityTextDatas.Sort(_abilityTextComparer);
		_abilityTextComparer.ClearParams();
	}

	private static void CheckAbilityDisplayOverride(List<AbilityTextData> abilityTextDatas, ICardDataAdapter cardData, AssetLookupSystem assetLookupSystem)
	{
		if (!assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AbilityTextDisplayOverride> loadedTree))
		{
			return;
		}
		IBlackboard blackboard = assetLookupSystem.Blackboard;
		blackboard.Clear();
		blackboard.SetCardDataExtensive(cardData);
		foreach (AbilityTextData abilityTextData in abilityTextDatas)
		{
			blackboard.Ability = abilityTextData.Printing;
			AbilityTextDisplayOverride payload = loadedTree.GetPayload(blackboard);
			if (payload != null)
			{
				abilityTextData.HideText = payload.HideAbility;
			}
		}
	}

	public static void RearrangeForBackupMechanic(List<AbilityTextData> abilityTextDatas)
	{
		int num = abilityTextDatas.FindIndex((AbilityTextData x) => x.Printing.BaseId == 287);
		if (num <= -1)
		{
			return;
		}
		int num2 = 0;
		int num3 = 0;
		for (int num4 = num - 1; num4 >= 0; num4--)
		{
			if (abilityTextDatas[num4].IsMutation)
			{
				num3 = num4 + 1;
				break;
			}
		}
		for (int num5 = num + 1; num5 < abilityTextDatas.Count; num5++)
		{
			AbilityTextData abilityTextData = abilityTextDatas[num5];
			if ((abilityTextData.State == AbilityState.Added || abilityTextData.State == AbilityState.Removed || abilityTextData.IsMutation) && (!abilityTextData.IsKeyword || !abilityTextData.OmitDuplicates || !abilityTextDatas.Exists(abilityTextData.Printing.Id, (AbilityTextData x, uint printingId) => !x.IsMutation && x.State != AbilityState.Added && x.Printing.Id == printingId)))
			{
				if (abilityTextData.IsKeyword && abilityTextData.IsGroupable)
				{
					abilityTextDatas.RemoveAt(num5);
					abilityTextDatas.Insert(num2, abilityTextData);
					num2++;
					num3++;
				}
				else
				{
					abilityTextDatas.RemoveAt(num5);
					abilityTextDatas.Insert(num3, abilityTextData);
					num3++;
				}
			}
		}
	}

	public static void RemoveDuplicateAbilities(List<AbilityTextData> abilityTextDatas)
	{
		for (int num = abilityTextDatas.Count - 1; num >= 0; num--)
		{
			AbilityTextData abilityTextData = abilityTextDatas[num];
			if (abilityTextData.OmitDuplicates)
			{
				for (int num2 = num - 1; num2 >= 0; num2--)
				{
					AbilityTextData abilityTextData2 = abilityTextDatas[num2];
					if (abilityTextData2.OmitDuplicates && abilityTextData2.Printing.Id == abilityTextData.Printing.Id)
					{
						abilityTextDatas.RemoveAt(num2);
						break;
					}
				}
			}
		}
	}

	public static void CollapsePerpetualAbilities(List<AbilityTextData> abilityTextDatas, IClientLocProvider locMan, CardTextColorSettings colorSettings)
	{
		for (int i = 0; i < abilityTextDatas.Count; i++)
		{
			AbilityTextData abilityTextData = abilityTextDatas[i];
			if (!abilityTextData.IsPerpetual || abilityTextData.OmitDuplicates)
			{
				continue;
			}
			for (int j = i + 1; j < abilityTextDatas.Count; j++)
			{
				AbilityTextData abilityTextData2 = abilityTextDatas[j];
				if (abilityTextData2.IsPerpetual && !abilityTextData2.OmitDuplicates && abilityTextData2.Printing.Id == abilityTextData.Printing.Id)
				{
					abilityTextData.Count++;
					abilityTextDatas.RemoveAt(j);
					j--;
				}
			}
			if (abilityTextData.Count > 1)
			{
				SetAndApplyFormattingToCount(abilityTextData, locMan, colorSettings.DefaultFormat);
			}
		}
	}

	public static void CollapseAddedAbilities(List<AbilityTextData> abilityTextDatas, IClientLocProvider locMan, CardTextColorSettings colorSettings)
	{
		for (int i = 0; i < abilityTextDatas.Count; i++)
		{
			AbilityTextData abilityTextData = abilityTextDatas[i];
			if (abilityTextData.IsPerpetual || abilityTextData.OmitDuplicates || !abilityTextData.State.HasFlag(AbilityState.Added))
			{
				continue;
			}
			for (int j = i + 1; j < abilityTextDatas.Count; j++)
			{
				AbilityTextData abilityTextData2 = abilityTextDatas[j];
				if (!abilityTextData2.IsPerpetual && !abilityTextData2.OmitDuplicates && abilityTextData2.State == abilityTextData.State && abilityTextData2.Printing.Id == abilityTextData.Printing.Id)
				{
					abilityTextData.Count++;
					abilityTextDatas.RemoveAt(j);
					j--;
				}
			}
			if (abilityTextData.Count > 1)
			{
				SetAndApplyFormattingToCount(abilityTextData, locMan, colorSettings.DefaultFormat);
			}
		}
	}

	private static void ColorizeModalAbilityText(List<AbilityTextData> abilityTextDatas, ICardDataAdapter cardData, Func<uint, string> getAbilityText, Func<ICardDataAdapter, AbilityPrintingData, bool, string> getFormatForModalChildAbility)
	{
		for (int i = 0; i < abilityTextDatas.Count; i++)
		{
			AbilityTextData abilityTextData = abilityTextDatas[i];
			if (!HasModalAbilitiesToFormat(abilityTextData?.Printing, cardData, abilityTextDatas))
			{
				continue;
			}
			AbilityPrintingData printing = abilityTextData.Printing;
			if (printing.BaseId == 330)
			{
				foreach (AbilityPrintingData childPrinting in printing.ModalAbilityChildren)
				{
					string text = getAbilityText(childPrinting.Id);
					int num = abilityTextDatas.RemoveAll((AbilityTextData x) => x.Printing.Id == childPrinting.Id);
					string format = getFormatForModalChildAbility(cardData, childPrinting, num > 0);
					abilityTextData.FormattedLocalizedText = abilityTextData.FormattedLocalizedText.Replace(text, string.Format(format, text));
				}
				continue;
			}
			MatchCollection matchCollection = Regex.Matches(abilityTextData.FormattedLocalizedText, "[•]\\s*[^•]+");
			for (int num2 = 0; num2 < printing.ModalAbilityChildren.Count; num2++)
			{
				AbilityPrintingData childPrinting2 = printing.ModalAbilityChildren[num2];
				Match match = ((matchCollection.Count > num2) ? matchCollection[num2] : Match.Empty);
				if (match.Success)
				{
					int num3 = abilityTextDatas.RemoveAll((AbilityTextData x) => x.Printing.Id == childPrinting2.Id);
					string text2 = string.Format(getFormatForModalChildAbility(cardData, childPrinting2, num3 > 0), match.Value);
					abilityTextData.FormattedLocalizedText = abilityTextData.FormattedLocalizedText.Replace(match.Value, text2);
					for (int num4 = 0; num4 < num3 - 1; num4++)
					{
						int startIndex = abilityTextData.FormattedLocalizedText.IndexOf(text2);
						abilityTextData.FormattedLocalizedText = abilityTextData.FormattedLocalizedText.Insert(startIndex, text2 + Environment.NewLine);
					}
				}
			}
		}
	}

	private static bool HasModalAbilitiesToFormat(AbilityPrintingData abilityPrinting, ICardDataAdapter cardData, IEnumerable<AbilityTextData> otherAbilityText)
	{
		if (abilityPrinting == null || cardData == null || otherAbilityText == null)
		{
			return false;
		}
		if (!abilityPrinting.IsModalAbility())
		{
			return false;
		}
		MtgCardInstance instance = cardData.Instance;
		foreach (AbilityPrintingData modalAbilityChild in abilityPrinting.ModalAbilityChildren)
		{
			if (instance != null)
			{
				foreach (MtgAbilityInstance abilityInstance in instance.AbilityInstances)
				{
					if (abilityInstance.ModalAbilityIsExhausted(modalAbilityChild.Id))
					{
						return true;
					}
				}
			}
			foreach (AbilityTextData item in otherAbilityText)
			{
				if (item.Printing.Id == modalAbilityChild.Id)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void GroupAbilityText(List<AbilityTextData> abilityTextDatas, string separator, CardTextColorSettings colorSettings)
	{
		for (int i = 0; i < abilityTextDatas.Count - 1; i++)
		{
			if (abilityTextDatas[i].IsGroupable && abilityTextDatas[i + 1].IsGroupable)
			{
				_sharedCachedStringBuilder.Clear();
				_sharedCachedStringBuilder.Append(abilityTextDatas[i].FormattedLocalizedText);
				while (i < abilityTextDatas.Count - 1 && abilityTextDatas[i + 1].IsGroupable)
				{
					_sharedCachedStringBuilder.Append(string.Format(colorSettings.DefaultFormat, separator));
					_sharedCachedStringBuilder.Append(abilityTextDatas[i + 1].FormattedLocalizedText);
					abilityTextDatas.RemoveAt(i + 1);
				}
				abilityTextDatas[i].FormattedLocalizedText = _sharedCachedStringBuilder.ToString();
				_sharedCachedStringBuilder.Clear();
			}
		}
	}

	private static void CondenseChapterAbilities(List<AbilityTextData> abilityTextDatas, ICardDataAdapter cardData)
	{
		for (int i = 0; i < abilityTextDatas.Count; i++)
		{
			AbilityTextData abilityTextData = abilityTextDatas[i];
			AbilityPrintingData printing = abilityTextData.Printing;
			if (printing.Id == 260 && cardData.Subtypes.Contains(SubType.Saga))
			{
				abilityTextDatas.RemoveAt(i);
				i--;
			}
			else
			{
				if (!abilityTextData.IsChapterAbility)
				{
					continue;
				}
				List<uint> list = new List<uint>();
				list.Add(printing.BaseIdNumeral.GetValueOrDefault());
				string formattedLocalizedText = abilityTextData.FormattedLocalizedText;
				while (i + 1 < abilityTextDatas.Count)
				{
					AbilityTextData abilityTextData2 = abilityTextDatas[i + 1];
					AbilityPrintingData printing2 = abilityTextDatas[i + 1].Printing;
					if (!abilityTextData2.IsChapterAbility || !abilityTextData2.FormattedLocalizedText.Equals(formattedLocalizedText))
					{
						break;
					}
					list.Add(printing2.BaseIdNumeral.GetValueOrDefault());
					abilityTextDatas.RemoveAt(i + 1);
				}
				abilityTextData.Chapters = list;
				abilityTextData.FormattedLocalizedText = formattedLocalizedText;
			}
		}
	}

	private static void FormatChapterAbilities(IEnumerable<AbilityTextData> abilityTextDatas, ICardDataAdapter cardModel, CardTextColorSettings colorSettings)
	{
		foreach (AbilityTextData abilityTextData in abilityTextDatas)
		{
			if (abilityTextData.IsChapterAbility)
			{
				AbilityState abilityState = chapterAbilityState(abilityTextData, cardModel);
				string format = colorSettings.FormatForAbilityState(abilityState);
				abilityTextData.FormattedLocalizedText = string.Format(format, abilityTextData.RawLocalizedText);
			}
		}
		static AbilityState chapterAbilityState(AbilityTextData abilityTextData, ICardDataAdapter cardDataAdapter)
		{
			if (cardDataAdapter.Counters.TryGetValue(CounterType.Lore, out var value) && !abilityTextData.Chapters.Contains((uint)value))
			{
				return AbilityState.Removed;
			}
			return abilityTextData.State;
		}
	}

	private static void FormatClassGrantedAbilities(List<AbilityTextData> abilityTexts, ICardDataAdapter cardData, CardTextColorSettings colorSettings, Func<uint, string> getAbilityText)
	{
		if (cardData == null)
		{
			return;
		}
		bool flag = cardData.ZoneType == ZoneType.Battlefield;
		for (int num = abilityTexts.Count - 1; num >= 0; num--)
		{
			AbilityTextData abilityTextData = abilityTexts[num];
			if (abilityTextData.State != AbilityState.Removed && abilityTextData.IsClassLevelAbility)
			{
				int num2 = 0;
				foreach (AbilityPrintingData classGrantedAbility in abilityTextData.Printing.GetClassGrantedAbilities(cardData.Printing))
				{
					uint id = classGrantedAbility.Id;
					if (!needsToAddGrantedAbility(abilityTexts, num, id))
					{
						continue;
					}
					if (tryGetAddedIdx(abilityTexts, id, out var addedIdx))
					{
						AbilityTextData abilityTextData2 = abilityTexts[addedIdx];
						string format = colorSettings.FormatForAbilityState(AbilityState.Normal);
						abilityTextData2.FormattedLocalizedText = string.Format(format, abilityTextData2.RawLocalizedText);
						abilityTexts.RemoveAt(addedIdx);
						if (addedIdx < num)
						{
							num--;
						}
						abilityTexts.Insert(num + 1 + num2, abilityTextData2);
					}
					else
					{
						AbilityState abilityState = ((!flag) ? AbilityState.Normal : AbilityState.Removed);
						string format2 = colorSettings.FormatForAbilityState(abilityState);
						AbilityTextData item = new AbilityTextData
						{
							Printing = classGrantedAbility,
							RawLocalizedText = getAbilityText(classGrantedAbility.Id),
							FormattedLocalizedText = string.Format(format2, getAbilityText(classGrantedAbility.Id)),
							State = abilityState
						};
						abilityTexts.Insert(num + 1 + num2, item);
					}
					num2++;
				}
			}
		}
		static bool needsToAddGrantedAbility(IReadOnlyList<AbilityTextData> texts, int idx, uint grantedAbilityId)
		{
			for (int i = idx; i < texts.Count; i++)
			{
				AbilityTextData abilityTextData3 = texts[i];
				if (abilityTextData3.Printing.Id == grantedAbilityId && abilityTextData3.State == AbilityState.Normal)
				{
					return false;
				}
			}
			return true;
		}
		static bool tryGetAddedIdx(IReadOnlyList<AbilityTextData> texts, uint grantedAbilityId, out int reference)
		{
			for (int i = 0; i < texts.Count; i++)
			{
				AbilityTextData abilityTextData3 = texts[i];
				if (abilityTextData3.Printing.Id == grantedAbilityId && abilityTextData3.State == AbilityState.Added)
				{
					reference = i;
					return true;
				}
			}
			reference = -1;
			return false;
		}
	}

	public static void FormatStationGrantedAbilities(List<AbilityTextData> abilityTexts, ICardDataAdapter cardData, CardTextColorSettings colorSettings, string commaSeparator, Func<uint, string> getAbilityText)
	{
		if (cardData == null)
		{
			return;
		}
		bool flag = cardData.ZoneType == ZoneType.Battlefield;
		int num = abilityTexts.FindIndex(0, (AbilityTextData x) => x.Printing.Id == 373);
		if (num == -1)
		{
			return;
		}
		AbilityTextData abilityTextData = abilityTexts[num];
		List<AbilityPrintingData> list = _sharedObjPool.PopObject<List<AbilityPrintingData>>();
		list.AddRange(getStationGrantingAbilities(num, abilityTexts, cardData));
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			AbilityPrintingData abilityPrintingData = list[num2];
			List<AbilityTextData> list2 = _sharedObjPool.PopObject<List<AbilityTextData>>();
			foreach (AbilityPrintingData hiddenAbility in abilityPrintingData.HiddenAbilities)
			{
				uint id = hiddenAbility.Id;
				if (tryGetExistingIdx(abilityTexts, id, out var existingIdx))
				{
					if (existingIdx < num)
					{
						num--;
					}
					AbilityTextData abilityTextData2 = abilityTexts[existingIdx];
					abilityTexts.RemoveAt(existingIdx);
					string format = colorSettings.FormatForAbilityState(abilityTextData.State);
					abilityTextData2.FormattedLocalizedText = string.Format(format, abilityTextData2.RawLocalizedText);
					abilityTextData2.StationData = new StationAbilityData(abilityPrintingData.Id, abilityPrintingData.LevelRequirement, abilityPrintingData.Power, abilityPrintingData.Toughness, num2 == 0, cardData.PresentationColor, isActive: true);
					list2.Add(abilityTextData2);
				}
				else
				{
					AbilityState abilityState = (flag ? AbilityState.Removed : abilityTextData.State);
					string format2 = colorSettings.FormatForAbilityState(abilityTextData.State);
					list2.Add(new AbilityTextData
					{
						Printing = hiddenAbility,
						RawLocalizedText = getAbilityText(hiddenAbility.Id),
						FormattedLocalizedText = string.Format(format2, getAbilityText(hiddenAbility.Id)),
						State = abilityState,
						IsKeyword = hiddenAbility.IsKeyword(),
						IsGroupable = hiddenAbility.IsGroupable(),
						StationData = new StationAbilityData(abilityPrintingData.Id, abilityPrintingData.LevelRequirement, abilityPrintingData.Power, abilityPrintingData.Toughness, num2 == 0, cardData.PresentationColor, abilityState == AbilityState.Normal)
					});
				}
			}
			if (list2.Count > 0)
			{
				GroupAbilityText(list2, commaSeparator, colorSettings);
				AbilityTextData abilityTextData3 = list2[0];
				_sharedCachedStringBuilder.Clear();
				while (list2.Count > 0)
				{
					_sharedCachedStringBuilder.AppendLine(list2[0].FormattedLocalizedText);
					list2.RemoveAt(0);
				}
				_sharedCachedStringBuilder.Length -= Environment.NewLine.Length;
				abilityTextData3.FormattedLocalizedText = _sharedCachedStringBuilder.ToString();
				abilityTextData3.IsGroupable = false;
				abilityTextData3.IsKeyword = false;
				_sharedCachedStringBuilder.Clear();
				insertOrAdd(abilityTexts, num + 1 + num2, abilityTextData3);
			}
			list2.Clear();
			_sharedObjPool.PushObject(list2, tryClear: false);
		}
		list.Clear();
		_sharedObjPool.PushObject(list, tryClear: false);
		static IEnumerable<AbilityPrintingData> getStationGrantingAbilities(int stationIdx, List<AbilityTextData> list3, ICardDataAdapter cardDataAdapter)
		{
			if (list3[stationIdx].State == AbilityState.Removed)
			{
				List<AbilityPrintingData> removedIntrinsicAbilities = _sharedObjPool.PopObject<List<AbilityPrintingData>>();
				int num3 = stationIdx + 1;
				while (num3 < list3.Count)
				{
					AbilityPrintingData printing = list3[num3].Printing;
					if (printing.SubCategory == AbilitySubCategory.StationIntrinsicLevel)
					{
						removedIntrinsicAbilities.Add(printing);
						list3.RemoveAt(num3);
					}
					else
					{
						num3++;
					}
				}
				foreach (AbilityPrintingData item in removedIntrinsicAbilities)
				{
					yield return item;
				}
				removedIntrinsicAbilities.Clear();
				_sharedObjPool.PushObject(removedIntrinsicAbilities, tryClear: false);
			}
			else
			{
				foreach (AbilityPrintingData intrinsicAbility in cardDataAdapter.IntrinsicAbilities)
				{
					if (intrinsicAbility.SubCategory == AbilitySubCategory.StationIntrinsicLevel)
					{
						yield return intrinsicAbility;
					}
				}
			}
		}
		static void insertOrAdd(List<AbilityTextData> texts, int insertIdx, AbilityTextData toInsert)
		{
			if (insertIdx < texts.Count)
			{
				texts.Insert(insertIdx, toInsert);
			}
			else
			{
				texts.Add(toInsert);
			}
		}
		static bool tryGetExistingIdx(IReadOnlyList<AbilityTextData> texts, uint grantedAbilityId, out int reference)
		{
			for (int i = 0; i < texts.Count; i++)
			{
				if (texts[i].Printing.Id == grantedAbilityId)
				{
					reference = i;
					return true;
				}
			}
			reference = -1;
			return false;
		}
	}

	private static void FormatLevelUpGrantedAbilities(List<AbilityTextData> abilityTexts, ICardDataAdapter cardData, CardTextColorSettings colorSettings, string commaSeparator, Func<uint, string> getAbilityText)
	{
		if (cardData == null)
		{
			return;
		}
		bool flag = cardData.ZoneType == ZoneType.Battlefield;
		int num = abilityTexts.FindIndex(0, (AbilityTextData x) => x.Printing.BaseId == 88);
		if (num == -1)
		{
			return;
		}
		AbilityTextData abilityTextData = abilityTexts[num];
		List<AbilityPrintingData> list = _sharedObjPool.PopObject<List<AbilityPrintingData>>();
		list.AddRange(getLevelUpGrantingAbilities(num, abilityTexts, cardData));
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			AbilityPrintingData abilityPrintingData = list[num2];
			List<AbilityTextData> list2 = _sharedObjPool.PopObject<List<AbilityTextData>>();
			foreach (AbilityPrintingData hiddenAbility in abilityPrintingData.HiddenAbilities)
			{
				uint id = hiddenAbility.Id;
				if (tryGetExistingIdx(abilityTexts, id, out var existingIdx))
				{
					if (existingIdx < num)
					{
						num--;
					}
					AbilityTextData abilityTextData2 = abilityTexts[existingIdx];
					abilityTexts.RemoveAt(existingIdx);
					string format = colorSettings.FormatForAbilityState(abilityTextData.State);
					abilityTextData2.FormattedLocalizedText = string.Format(format, abilityTextData2.RawLocalizedText);
					abilityTextData2.LevelUpData = new LevelUpAbilityData(abilityPrintingData.LevelRequirement, abilityPrintingData.Power, abilityPrintingData.Toughness, num2 == 0, cardData.PresentationColor, isActive: true);
					list2.Add(abilityTextData2);
				}
				else
				{
					AbilityState abilityState = (flag ? AbilityState.Removed : abilityTextData.State);
					string format2 = colorSettings.FormatForAbilityState(abilityTextData.State);
					list2.Add(new AbilityTextData
					{
						Printing = hiddenAbility,
						RawLocalizedText = getAbilityText(hiddenAbility.Id),
						FormattedLocalizedText = string.Format(format2, getAbilityText(hiddenAbility.Id)),
						State = abilityState,
						IsKeyword = hiddenAbility.IsKeyword(),
						IsGroupable = hiddenAbility.IsGroupable(),
						LevelUpData = new LevelUpAbilityData(abilityPrintingData.LevelRequirement, abilityPrintingData.Power, abilityPrintingData.Toughness, num2 == 0, cardData.PresentationColor, abilityState == AbilityState.Normal)
					});
				}
			}
			if (list2.Count > 0)
			{
				GroupAbilityText(list2, commaSeparator, colorSettings);
				AbilityTextData abilityTextData3 = list2[0];
				_sharedCachedStringBuilder.Clear();
				while (list2.Count > 0)
				{
					_sharedCachedStringBuilder.AppendLine(list2[0].FormattedLocalizedText);
					list2.RemoveAt(0);
				}
				_sharedCachedStringBuilder.Length -= Environment.NewLine.Length;
				abilityTextData3.FormattedLocalizedText = _sharedCachedStringBuilder.ToString();
				abilityTextData3.IsGroupable = false;
				abilityTextData3.IsKeyword = false;
				_sharedCachedStringBuilder.Clear();
				insertOrAdd(abilityTexts, num + 1 + num2, abilityTextData3);
			}
			list2.Clear();
			_sharedObjPool.PushObject(list2, tryClear: false);
		}
		list.Clear();
		_sharedObjPool.PushObject(list, tryClear: false);
		static IEnumerable<AbilityPrintingData> getLevelUpGrantingAbilities(int levelUpIdx, List<AbilityTextData> list3, ICardDataAdapter cardDataAdapter)
		{
			if (list3[levelUpIdx].State == AbilityState.Removed)
			{
				List<AbilityPrintingData> removedIntrinsicAbilities = _sharedObjPool.PopObject<List<AbilityPrintingData>>();
				int num3 = levelUpIdx + 1;
				while (num3 < list3.Count)
				{
					AbilityPrintingData printing = list3[num3].Printing;
					if (printing.SubCategory == AbilitySubCategory.LevelUpAbilityGranting)
					{
						removedIntrinsicAbilities.Add(printing);
						list3.RemoveAt(num3);
					}
					else
					{
						num3++;
					}
				}
				foreach (AbilityPrintingData item in removedIntrinsicAbilities)
				{
					yield return item;
				}
				removedIntrinsicAbilities.Clear();
				_sharedObjPool.PushObject(removedIntrinsicAbilities, tryClear: false);
			}
			else
			{
				foreach (AbilityPrintingData intrinsicAbility in cardDataAdapter.IntrinsicAbilities)
				{
					if (intrinsicAbility.SubCategory == AbilitySubCategory.LevelUpAbilityGranting)
					{
						yield return intrinsicAbility;
					}
				}
			}
		}
		static void insertOrAdd(List<AbilityTextData> texts, int insertIdx, AbilityTextData toInsert)
		{
			if (insertIdx < texts.Count)
			{
				texts.Insert(insertIdx, toInsert);
			}
			else
			{
				texts.Add(toInsert);
			}
		}
		static bool tryGetExistingIdx(IReadOnlyList<AbilityTextData> texts, uint grantedAbilityId, out int reference)
		{
			for (int i = 0; i < texts.Count; i++)
			{
				if (texts[i].Printing.Id == grantedAbilityId)
				{
					reference = i;
					return true;
				}
			}
			reference = -1;
			return false;
		}
	}

	private static void UnspoolPerpetualCTOAbilitiesOnTheStack(ICardDataAdapter cardModel, List<AbilityTextData> abilityTexts, IClientLocProvider locMan, CardTextColorSettings colorSettings)
	{
		if (cardModel.ZoneType != ZoneType.Stack || cardModel.ObjectType == GameObjectType.Ability)
		{
			return;
		}
		for (int i = 0; i < abilityTexts.Count; i++)
		{
			AbilityTextData abilityTextData = abilityTexts[i];
			if (!abilityTextData.IsPerpetual || abilityTextData.Count <= 1 || abilityTextData.Printing.BaseId != 241)
			{
				continue;
			}
			int num = 0;
			foreach (CastingTimeOption castingTimeOption in cardModel.CastingTimeOptions)
			{
				if (castingTimeOption.IsCasualty && castingTimeOption.AbilityId.HasValue && castingTimeOption.AbilityId == abilityTextData.Printing.Id)
				{
					num++;
				}
			}
			if (num == 0)
			{
				continue;
			}
			int num2 = 0;
			foreach (AbilityTextData abilityText in abilityTexts)
			{
				if (!abilityText.IsPerpetual && abilityText.Printing.Id == abilityTextData.Printing.Id)
				{
					num2 += abilityText.Count;
				}
			}
			int num3 = num - num2;
			int num4 = abilityTextData.Count - num3;
			if (num3 > 0 && num4 > 0)
			{
				abilityTextData.Count = num4;
				ApplyAbilityCountFormatting(abilityTextData, locMan, colorSettings.PerpetualFormat);
				AbilityTextData abilityTextData2 = (AbilityTextData)abilityTextData.MemberwiseClone();
				abilityTextData2.Count = num3;
				ApplyAbilityCountFormatting(abilityTextData2, locMan, colorSettings.PerpetualFormat);
				abilityTexts.Insert(i + 1, abilityTextData2);
				i++;
			}
		}
	}

	private static void ApplyAbilityCountFormatting(AbilityTextData abilityTextData, IClientLocProvider locManager, string colorFormat)
	{
		string text = string.Format(colorFormat, abilityTextData.RawLocalizedText);
		string formattedLocalizedText = ((abilityTextData.Count == 1) ? text : locManager.GetLocalizedText("DuelScene/RuleText/AbilityStacking", ("abilityText", text), ("count", abilityTextData.Count.ToString())));
		abilityTextData.FormattedLocalizedText = formattedLocalizedText;
	}

	private static void SetAndApplyFormattingToCount(AbilityTextData abilityTextData, IClientLocProvider locManager, string colorFormat)
	{
		abilityTextData.FormattedLocalizedText = string.Format(colorFormat, locManager.GetLocalizedText("DuelScene/RuleText/AbilityStacking", ("abilityText", abilityTextData.FormattedLocalizedText), ("count", abilityTextData.Count.ToString())));
	}

	private static void FormatAlternateCostsOnTheStack(ICardDataAdapter cardModel, List<AbilityTextData> abilityTexts, CardTextColorSettings colorSettings, IAbilityDataProvider abilityDataProvider, IClientLocProvider locProvider)
	{
		if (cardModel.ZoneType != ZoneType.Stack || cardModel.ObjectType == GameObjectType.Ability)
		{
			return;
		}
		IReadOnlyList<CastingTimeOption> castingTimeOptions = cardModel.CastingTimeOptions;
		if (castingTimeOptions == null)
		{
			return;
		}
		foreach (AbilityTextData abilityText in abilityTexts)
		{
			AbilityPrintingData printing = abilityText.Printing;
			if (abilityIsTypeAndUnused(castingTimeOptions, printing, 34u, CastingTimeOptionType.Kicker) || abilityIsTypeAndUnused(castingTimeOptions, printing, 303u, CastingTimeOptionType.Bargain) || abilityIsTypeAndUnused(castingTimeOptions, printing, 342u, CastingTimeOptionType.AdditionalCost) || abilityIsTypeAndUnused(castingTimeOptions, printing, 341u, CastingTimeOptionType.AdditionalCost) || abilityIsTypeAndUnused(castingTimeOptions, printing, 371u, CastingTimeOptionType.CastThroughAbility) || abilityIsTypeAndUnused(castingTimeOptions, printing, 382u, CastingTimeOptionType.CastThroughAbility) || abilityIsTypeAndUnused(castingTimeOptions, printing, 79u, CastingTimeOptionType.Conspire) || abilityIsInactiveCasualtyInstance(castingTimeOptions, abilityText, abilityTexts) || abilityIsCollectEvidenceAndUnused(castingTimeOptions, printing) || abilityIsAlternativeCostAndUnused(castingTimeOptions, printing, abilityDataProvider))
			{
				ApplyAbilityCountFormatting(abilityText, locProvider, colorSettings.RemovedFormat);
			}
		}
		static bool abilityIsAlternativeCostAndUnused(IReadOnlyList<CastingTimeOption> readOnlyList, AbilityPrintingData abPrinting, IAbilityDataProvider abilityDataProvider2)
		{
			if (abPrinting.Id == 7022)
			{
				return false;
			}
			if (!((abPrinting.Category == AbilityCategory.AlternativeCost) | (abilityDataProvider2.GetAbilityRecordById(abPrinting.BaseId).Category == AbilityCategory.AlternativeCost)))
			{
				return false;
			}
			foreach (CastingTimeOption readOnly in readOnlyList)
			{
				if (readOnly.IsCastThroughAbility && readOnly.AbilityId.HasValue && readOnly.AbilityId.Value == abPrinting.Id)
				{
					return false;
				}
			}
			return true;
		}
		static bool abilityIsCollectEvidenceAndUnused(IReadOnlyList<CastingTimeOption> readOnlyList, AbilityPrintingData abPrinting)
		{
			if (abPrinting.Category != AbilityCategory.AdditionalCost || abPrinting.SubCategory != AbilitySubCategory.CastingTimeOption || !abPrinting.ReferencedAbilityTypes.Contains(AbilityType.CollectEvidence))
			{
				return false;
			}
			foreach (CastingTimeOption readOnly2 in readOnlyList)
			{
				if (readOnly2.AbilityId.HasValue && readOnly2.AbilityId.Value == abPrinting.Id && readOnly2.Type == CastingTimeOptionType.AdditionalCost)
				{
					return false;
				}
			}
			return true;
		}
		static bool abilityIsInactiveCasualtyInstance(IReadOnlyList<CastingTimeOption> readOnlyList, AbilityTextData abilityText, List<AbilityTextData> list)
		{
			AbilityPrintingData printing2 = abilityText.Printing;
			if (printing2.BaseId != 241)
			{
				return false;
			}
			if (!readOnlyList.Exists((CastingTimeOption x) => x.IsCasualty))
			{
				return true;
			}
			int num = 0;
			foreach (CastingTimeOption readOnly3 in readOnlyList)
			{
				if (readOnly3.IsCasualty && readOnly3.AbilityId.HasValue && readOnly3.AbilityId == printing2.Id)
				{
					num++;
				}
			}
			int num2 = 0;
			int num3 = 0;
			foreach (AbilityTextData item in list)
			{
				if (printing2.Id == item.Printing.Id)
				{
					if (item == abilityText)
					{
						num3 = num2;
					}
					num2 += item.Count;
				}
			}
			return num3 < num2 - num;
		}
		static bool abilityIsTypeAndUnused(IReadOnlyList<CastingTimeOption> readOnlyList, AbilityPrintingData ability, uint abilityId, CastingTimeOptionType castingTimeOptionType)
		{
			if (ability.BaseId != abilityId && ability.Id != abilityId)
			{
				return false;
			}
			foreach (CastingTimeOption readOnly4 in readOnlyList)
			{
				if (readOnly4.Type == castingTimeOptionType && readOnly4.AbilityId.HasValue && readOnly4.AbilityId.Value == ability.Id)
				{
					return false;
				}
			}
			return true;
		}
	}

	private static void FormatIntensityAbilities(ICardDataAdapter cardData, List<AbilityTextData> abilityTexts, CardTextColorSettings colorSettings, IClientLocProvider locProvider)
	{
		if (cardData.Instance == null)
		{
			return;
		}
		foreach (DesignationData designation in cardData.Instance.Designations)
		{
			if (!DesignationTranslator.TryTranslateIntensityDesignation(designation, out var intensityDesignation))
			{
				continue;
			}
			foreach (AbilityTextData abilityText in abilityTexts)
			{
				if (abilityText.IsIntensityAbility)
				{
					string localizedText = locProvider.GetLocalizedText("Card/Textbox/CurrentIntensity", ("currentIntensity", intensityDesignation.IntensityLevel.ToString("N0")));
					localizedText = Utilities.GetBoldedAbilityText(localizedText);
					localizedText = string.Format(colorSettings.PerpetualFormat, localizedText);
					abilityText.FormattedLocalizedText = abilityText.FormattedLocalizedText + " " + localizedText;
					break;
				}
			}
		}
	}

	private static void FormatCaseAbilities(ICardDataAdapter cardData, List<AbilityTextData> abilityTextDatas, CardTextColorSettings colorSettings)
	{
		foreach (AbilityTextData abilityTextData in abilityTextDatas)
		{
			if (!abilityTextData.IsSolvedAbility)
			{
				continue;
			}
			MtgCardInstance instance = cardData.Instance;
			if (instance != null && instance.Zone.Type == ZoneType.Battlefield)
			{
				MtgCardInstance instance2 = cardData.Instance;
				if (instance2 != null && !instance2.Designations.Exists((DesignationData x) => x.Type == Designation.Solved))
				{
					abilityTextData.FormattedLocalizedText = string.Format(colorSettings.RemovedFormat, abilityTextData.RawLocalizedText);
				}
			}
		}
	}

	public static void FormatRoomChildAbilities(ICardDatabaseAdapter cdb, ICardDataAdapter cardData, List<AbilityTextData> abilityTextDatas, CardTextColorSettings colorSettings, Func<uint, string> getAbilityText)
	{
		MtgCardInstance parent = cardData.Parent;
		if (!parent.IsRoomParent())
		{
			return;
		}
		IReadOnlyList<KeyValuePair<AbilityPrintingData, AbilityState>> readOnlyList = CardData.GenerateAllAbilitiesList(parent, cdb.GetPrintingFromInstance(parent));
		bool flag = IsUnlockedRoom(parent, cardData);
		if (flag)
		{
			for (int i = 0; i < abilityTextDatas.Count; i++)
			{
				AbilityTextData abilityTextData = abilityTextDatas[i];
				AbilityPrintingData printing = abilityTextData.Printing;
				if (RemoveAbilityFromRoomChild(printing, readOnlyList))
				{
					abilityTextDatas[i] = new AbilityTextData
					{
						Printing = printing,
						RawLocalizedText = abilityTextData.RawLocalizedText,
						FormattedLocalizedText = string.Format(colorSettings.FormatForAbilityState(AbilityState.Removed), abilityTextData.RawLocalizedText),
						State = AbilityState.Removed
					};
				}
			}
		}
		if (!flag && !BothDoorsLocked(parent))
		{
			return;
		}
		MtgCardInstance mtgCardInstance = parent.Children.Find(cardData, (MtgCardInstance child, ICardDataAdapter thisCardData) => child.ObjectType != thisCardData.ObjectType);
		foreach (AbilityPrintingData nonPerpetualAddedAbility in GetNonPerpetualAddedAbilities(parent, readOnlyList))
		{
			if (!cardData.AddedAbilities.Exists(nonPerpetualAddedAbility, (AbilityPrintingData addedAbility, AbilityPrintingData thisAbility) => addedAbility.Id == thisAbility.Id) && (mtgCardInstance == null || !mtgCardInstance.AbilityAdders.Exists(nonPerpetualAddedAbility, (AddedAbilityData abilAdder, AbilityPrintingData thisAbility) => abilAdder.AbilityId == thisAbility.Id)))
			{
				string text = getAbilityText(nonPerpetualAddedAbility.Id);
				abilityTextDatas.Add(new AbilityTextData
				{
					Printing = parent.Abilities.GetById(nonPerpetualAddedAbility.Id),
					RawLocalizedText = text,
					FormattedLocalizedText = string.Format(colorSettings.FormatForAbilityState(AbilityState.Added), text),
					State = AbilityState.Added
				});
			}
		}
	}

	private static bool IsUnlockedRoom(MtgCardInstance parentInstance, ICardDataAdapter roomChild)
	{
		if (roomChild.ObjectType != GameObjectType.RoomLeft || !parentInstance.Designations.Exists((DesignationData d) => d.Type == Designation.LeftUnlocked))
		{
			if (roomChild.ObjectType == GameObjectType.RoomRight)
			{
				return parentInstance.Designations.Exists((DesignationData d) => d.Type == Designation.RightUnlocked);
			}
			return false;
		}
		return true;
	}

	private static bool BothDoorsLocked(MtgCardInstance cardInstance)
	{
		if (!cardInstance.Designations.Exists((DesignationData x) => x.Type == Designation.LeftUnlocked))
		{
			return !cardInstance.Designations.Exists((DesignationData x) => x.Type == Designation.RightUnlocked);
		}
		return false;
	}

	private static bool RemoveAbilityFromRoomChild(AbilityPrintingData ability, IReadOnlyList<KeyValuePair<AbilityPrintingData, AbilityState>> parentAbilities)
	{
		foreach (KeyValuePair<AbilityPrintingData, AbilityState> parentAbility in parentAbilities)
		{
			if (parentAbility.Key.Id == ability.Id && parentAbility.Value == AbilityState.Removed)
			{
				return true;
			}
		}
		return false;
	}

	private static IEnumerable<AbilityPrintingData> GetNonPerpetualAddedAbilities(MtgCardInstance instance, IReadOnlyList<KeyValuePair<AbilityPrintingData, AbilityState>> abilities)
	{
		foreach (KeyValuePair<AbilityPrintingData, AbilityState> ability in abilities)
		{
			if (ability.Value == AbilityState.Added)
			{
				AbilityPrintingData key = ability.Key;
				if (!instance.HasPerpetualAddedAbility(key))
				{
					yield return key;
				}
			}
		}
	}

	private static void FormatFuseAbilities(ICardDataAdapter cardData, List<AbilityTextData> abilityTexts, string separator)
	{
		if (!cardData.IsFuseCard())
		{
			return;
		}
		abilityTexts.RemoveAll((AbilityTextData x) => !x.IsFuseAbility && x.State == AbilityState.Normal);
		if (abilityTexts.Count > 1)
		{
			_sharedCachedStringBuilder.Clear();
			_sharedCachedStringBuilder.Append(abilityTexts[0].FormattedLocalizedText);
			while (abilityTexts.Count > 1)
			{
				_sharedCachedStringBuilder.Append(separator);
				_sharedCachedStringBuilder.Append(abilityTexts[1].FormattedLocalizedText);
				abilityTexts.RemoveAt(1);
			}
			abilityTexts[0].FormattedLocalizedText = _sharedCachedStringBuilder.ToString();
			_sharedCachedStringBuilder.Clear();
		}
	}
}
