using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Providers;

namespace Core.Rewards;

[Serializable]
public class PetReward : VanityItemReward<PetLevel, PetRewardDisplay>
{
	private static readonly int DISABLED_ANIMATOR_FLAG = Animator.StringToHash("Disabled");

	protected override RewardType _rewardType => RewardType.Pet;

	private CosmeticsProvider CosmeticsProvider => Pantry.Get<CosmeticsProvider>();

	public override string VanityItemPrefix => "pets";

	public void AddItemIfUniqueByName(string petName)
	{
		PetLevel petLevel = LevelForName(petName);
		if (petLevel != null)
		{
			AddItemIfUnique(petLevel);
		}
	}

	private PetLevel LevelForName(string petName)
	{
		string input = petName;
		int result;
		string variantType;
		if (petName.Contains("Skin"))
		{
			if (!int.TryParse(Regex.Replace(petName, "[\\w.]*Skin", "", RegexOptions.IgnoreCase), out result))
			{
				return null;
			}
			variantType = "Skin";
			petName = Regex.Replace(petName, ".Skin\\d", "", RegexOptions.IgnoreCase);
		}
		else
		{
			if (!int.TryParse(Regex.Replace(petName, "[\\w.]*Level", "", RegexOptions.IgnoreCase), out result))
			{
				return null;
			}
			variantType = "Level";
			petName = Regex.Replace(petName, ".Level\\d", "", RegexOptions.IgnoreCase);
		}
		input = Regex.Replace(input, petName + ".", "");
		petName = Regex.Replace(petName, "pets.", "", RegexOptions.IgnoreCase);
		return new PetLevel
		{
			PetName = petName,
			VariantType = variantType,
			Level = result,
			VariantId = input
		};
	}

	public override IEnumerable<Func<RewardDisplayContext, IEnumerator>> DisplayRewards(ContentControllerRewards ccr)
	{
		foreach (PetLevel petLevel in ToAdd)
		{
			yield return (RewardDisplayContext ctxt) => DisplayPetReward(ccr, petLevel, ctxt.ChildIndex);
		}
	}

	private IEnumerator DisplayPetReward(ContentControllerRewards ccr, PetLevel pet, int childIndex)
	{
		PetRewardDisplay petRewardDisplay = Instantiate(ccr, childIndex);
		petRewardDisplay.SetPet(pet, ccr.AssetLookupSystem);
		petRewardDisplay.OnObjectClicked += OnPetSetDefault(petRewardDisplay);
		yield return null;
	}

	public Action<string, string> OnPetSetDefault(PetRewardDisplay petDisplay)
	{
		return OnPetSetDefaultInner;
		void OnPetSetDefaultInner(string name, string variant)
		{
			CosmeticsProvider.SetPetSelection(name, variant);
			petDisplay.ApplyPetButtonAnimator.SetBool(DISABLED_ANIMATOR_FLAG, value: true);
			petDisplay.ApplyPetButton.Interactable = false;
			petDisplay.OnObjectClicked -= OnPetSetDefaultInner;
		}
	}

	public override void AddVanityItem(string name)
	{
		AddItemIfUniqueByName(name);
	}
}
