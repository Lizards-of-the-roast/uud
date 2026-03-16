using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public static class ActionsAvailableWorkflowUtils_ColorPicker
{
	private static Dictionary<ManaColor, uint> _restrictedManaColors = new Dictionary<ManaColor, uint>();

	private static HashSet<ManaColor> _unrestrictedManaColors = new HashSet<ManaColor>();

	public static bool CanUseColorPicker(IAbilityDataProvider cdb, params GreInteraction[] greInteractions)
	{
		Wotc.Mtgo.Gre.External.Messaging.Action[] array = new Wotc.Mtgo.Gre.External.Messaging.Action[greInteractions.Length];
		for (int i = 0; i < greInteractions.Length; i++)
		{
			array[i] = greInteractions[i].GreAction;
		}
		return CanUseColorPicker(cdb, array);
	}

	public static bool HasMultipleTriggeredAbilities(IAbilityDataProvider abilityDataProvider, ICardDataProvider cardDataProvider, params GreInteraction[] greInteractions)
	{
		Wotc.Mtgo.Gre.External.Messaging.Action[] array = new Wotc.Mtgo.Gre.External.Messaging.Action[greInteractions.Length];
		for (int i = 0; i < greInteractions.Length; i++)
		{
			array[i] = greInteractions[i].GreAction;
		}
		return HasMultipleTriggeredAbilities(abilityDataProvider, cardDataProvider, array);
	}

	public static bool CanUseColorPicker(IAbilityDataProvider cdb, params Wotc.Mtgo.Gre.External.Messaging.Action[] actions)
	{
		if (actions.Exists((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.ActionType != ActionType.ActivateMana || x.ManaPaymentOptions.Count == 0))
		{
			return false;
		}
		if (containsAbilityPropertyMismatch(cdb, actions))
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		_restrictedManaColors.Clear();
		_unrestrictedManaColors.Clear();
		int num = 0;
		int num2 = 0;
		uint num3 = 0u;
		int num4 = 0;
		foreach (Wotc.Mtgo.Gre.External.Messaging.Action action in actions)
		{
			if (ContainsMultiManaOption(action))
			{
				return false;
			}
			if (action.ManaCost.Count > 0)
			{
				flag2 = true;
			}
			else
			{
				flag = true;
			}
			if (flag2 && flag)
			{
				return false;
			}
			AbilityPrintingData abilityPrintingById = cdb.GetAbilityPrintingById(action.AbilityGrpId);
			if (abilityPrintingById == null)
			{
				return false;
			}
			if (abilityPrintingById.Tags.Contains(MetaDataTag.IgnoreClientManaWheel))
			{
				return false;
			}
			if (abilityPrintingById.RequiresConfirmation != RequiresConfirmation.None && actions.Length > 1)
			{
				return false;
			}
			if (abilityPrintingById.ReferencedAbilityTypes.Contains(AbilityType.Exert))
			{
				return false;
			}
			AutoTapSolution autoTapSolution = action.AutoTapSolution;
			if (autoTapSolution != null)
			{
				RepeatedField<ManaPaymentCondition> manaPaymentConditions = autoTapSolution.ManaPaymentConditions;
				if (manaPaymentConditions != null && ((IReadOnlyCollection<ManaPaymentCondition>)manaPaymentConditions).Count > 0)
				{
					return false;
				}
			}
			foreach (ManaPaymentOption manaPaymentOption in action.ManaPaymentOptions)
			{
				uint srcInstanceId = manaPaymentOption.Mana[0].SrcInstanceId;
				num4++;
				foreach (ManaInfo item in manaPaymentOption.Mana)
				{
					if (item.Specs.All((ManaInfo.Types.Spec x) => x.Type != ManaSpecType.Predictive))
					{
						return false;
					}
					if (item.Specs.Any((ManaInfo.Types.Spec x) => x.Type == ManaSpecType.Restricted) || action.IsManaAbilityWithSideEffect)
					{
						foreach (ManaColor uncombinedColor in ManaUtilities.GetUncombinedColors(item.Color))
						{
							if (_restrictedManaColors.TryGetValue(uncombinedColor, out var value) && value != item.AbilityGrpId)
							{
								return false;
							}
							_restrictedManaColors[uncombinedColor] = item.AbilityGrpId;
						}
					}
					else
					{
						_unrestrictedManaColors.UnionWith(ManaUtilities.GetUncombinedColors(item.Color));
					}
					if (item.Specs.Any((ManaInfo.Types.Spec x) => x.Type == ManaSpecType.Trigger))
					{
						num2++;
					}
					else
					{
						num++;
					}
					if (item.Color == ManaColor.AnyColor)
					{
						num3 += item.Count;
						if (num3 > 5)
						{
							return false;
						}
						if (item.SrcInstanceId != srcInstanceId)
						{
							return false;
						}
					}
				}
			}
		}
		if (_unrestrictedManaColors.Overlaps(_restrictedManaColors.Keys))
		{
			return false;
		}
		if (num != 0 && num2 != 0)
		{
			return false;
		}
		if (num4 <= 1 && num3 == 0)
		{
			return false;
		}
		return true;
		static bool containsAbilityPropertyMismatch(IAbilityDataProvider abilityProvider, IReadOnlyList<Wotc.Mtgo.Gre.External.Messaging.Action> readOnlyList)
		{
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				AbilityPrintingData abilityPrintingById2 = abilityProvider.GetAbilityPrintingById(readOnlyList[i].AbilityGrpId);
				if (abilityPrintingById2 != null)
				{
					for (int j = i + 1; j < readOnlyList.Count; j++)
					{
						AbilityPrintingData abilityPrintingById3 = abilityProvider.GetAbilityPrintingById(readOnlyList[j].AbilityGrpId);
						if (abilityPrintingById3 != null && (mismatchedCostTypes(abilityPrintingById2, abilityPrintingById3) || mismatchedPaymentType(abilityPrintingById2, abilityPrintingById3)))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		static bool mismatchedCostTypes(AbilityPrintingData ability1, AbilityPrintingData ability2)
		{
			IReadOnlyList<CostType> costTypes = ability1.CostTypes;
			IReadOnlyList<CostType> costTypes2 = ability2.CostTypes;
			if (costTypes.Count != costTypes2.Count)
			{
				return true;
			}
			for (int i = 0; i < costTypes.Count; i++)
			{
				CostType costType = costTypes[i];
				CostType costType2 = costTypes2[i];
				if (costType != costType2)
				{
					return true;
				}
				if (costType == CostType.Effect && costType2 == CostType.Effect && ability1.Id != ability2.Id)
				{
					return true;
				}
			}
			return false;
		}
		static bool mismatchedPaymentType(AbilityPrintingData ability1, AbilityPrintingData ability2)
		{
			return ability1.PaymentType != ability2.PaymentType;
		}
	}

	public static bool HasMultipleTriggeredAbilities(IAbilityDataProvider abilityDataProvider, ICardDataProvider cardDataProvider, params Wotc.Mtgo.Gre.External.Messaging.Action[] actions)
	{
		uint instanceId = actions[0].InstanceId;
		uint grpId = actions[0].GrpId;
		if (!cardDataProvider.TryGetCardPrintingById(grpId, out var _))
		{
			return false;
		}
		foreach (Wotc.Mtgo.Gre.External.Messaging.Action action in actions)
		{
			if (action.InstanceId != instanceId)
			{
				return false;
			}
			foreach (ManaPaymentOption manaPaymentOption in action.ManaPaymentOptions)
			{
				uint num = 0u;
				foreach (ManaInfo item in manaPaymentOption.Mana)
				{
					if (abilityDataProvider.TryGetAbilityPrintingById(item.AbilityGrpId, out var ability) && ability.Category == AbilityCategory.Triggered)
					{
						num++;
					}
				}
				if (num <= 1)
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool ContainsMultiManaOption(Wotc.Mtgo.Gre.External.Messaging.Action action)
	{
		uint instanceId = action.InstanceId;
		foreach (ManaPaymentOption manaPaymentOption in action.ManaPaymentOptions)
		{
			if (manaPaymentOption.Mana.Count < 2)
			{
				continue;
			}
			int num = 0;
			foreach (ManaInfo item in manaPaymentOption.Mana)
			{
				if (item.SrcInstanceId == instanceId)
				{
					num++;
					if (num > 1)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static void ShowManaColorSelection(DuelScene_CDC cdc, IReadOnlyList<GreInteraction> interactions, Action<GreInteraction> submitInteraction, ManaColorSelector colorSelector, IAbilityDataProvider abilityDataProvider, ICardHolderProvider cardHolderProvider)
	{
		Wotc.Mtgo.Gre.External.Messaging.Action[] array = new Wotc.Mtgo.Gre.External.Messaging.Action[interactions.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = interactions[i].GreAction;
		}
		ShowManaColorSelection(cdc, array, submitInteraction, colorSelector, abilityDataProvider, cardHolderProvider);
	}

	public static void ShowManaColorSelection(DuelScene_CDC cdc, IEnumerable<Wotc.Mtgo.Gre.External.Messaging.Action> actions, Action<GreInteraction> submitInteraction, ManaColorSelector colorSelector, IAbilityDataProvider abilityDataProvider, ICardHolderProvider cardHolderProvider)
	{
		uint sourceId = ((cdc != null && cdc.Model != null) ? cdc.Model.InstanceId : 0u);
		ManaSelectionFlow flow = new ManaSelectionFlow(sourceId, abilityDataProvider);
		flow.CreateTrees(actions);
		if (cardHolderProvider == null)
		{
			cardHolderProvider = NullCardHolderProvider.Default;
		}
		StackCardHolder cardHolder = cardHolderProvider.GetCardHolder<StackCardHolder>(GREPlayerNum.Invalid, CardHolderType.Stack);
		colorSelector.OpenSelector(flow, cdc.Root, new ManaColorSelector.ManaColorSelectorConfig(cdc.Model, canCancel: true, string.Empty, cardHolder), delegate
		{
			flow.Submit(submitInteraction);
		});
	}

	public static void ShowSourceInstanceManaColorSelection(DuelScene_CDC cdc, IReadOnlyList<GreInteraction> interactions, Action<GreInteraction> submitInteraction, ManaColorSelector colorSelector, IAbilityDataProvider abilityDataProvider, ICardHolderProvider cardHolderProvider)
	{
		Wotc.Mtgo.Gre.External.Messaging.Action[] array = new Wotc.Mtgo.Gre.External.Messaging.Action[interactions.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = interactions[i].GreAction;
		}
		ShowSourceInstanceManaColorSelection(cdc, array, submitInteraction, colorSelector, abilityDataProvider, cardHolderProvider);
	}

	public static void ShowSourceInstanceManaColorSelection(DuelScene_CDC cdc, IEnumerable<Wotc.Mtgo.Gre.External.Messaging.Action> actions, Action<GreInteraction> submitInteraction, ManaColorSelector colorSelector, IAbilityDataProvider abilityDataProvider, ICardHolderProvider cardHolderProvider)
	{
		uint sourceId = ((cdc != null && cdc.Model != null) ? cdc.Model.InstanceId : 0u);
		ManaSelectionFlow flow = new ManaSelectionFlow(sourceId, abilityDataProvider);
		flow.CreateSourceTree(actions, sourceId);
		if (cardHolderProvider == null)
		{
			cardHolderProvider = NullCardHolderProvider.Default;
		}
		StackCardHolder cardHolder = cardHolderProvider.GetCardHolder<StackCardHolder>(GREPlayerNum.Invalid, CardHolderType.Stack);
		colorSelector.OpenSelector(flow, cdc.Root, new ManaColorSelector.ManaColorSelectorConfig(cdc.Model, canCancel: true, string.Empty, cardHolder), delegate
		{
			flow.SubmitSourceOnly(submitInteraction);
		});
	}
}
