using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Cosmetic;
using Core.Meta.MainNavigation.Cosmetics;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class PetRewardDisplay : MonoBehaviour
{
	[Serializable]
	public class PetDisplay
	{
		public string PetName;

		public List<GameObject> PetLevelPrefabs;
	}

	[SerializeField]
	private Transform _petAnchor;

	[SerializeField]
	private CustomButton _petHitbox;

	[SerializeField]
	private Localize _text;

	[SerializeField]
	private EventTrigger applyPet;

	private IClientLocProvider _localizationManager;

	public CustomButton ApplyPetButton;

	public Animator ApplyPetButtonAnimator;

	private PetLevel _pet;

	private static readonly int Wrapper_Hover = Animator.StringToHash("Wrapper_Hover");

	private static readonly int InWrapper = Animator.StringToHash("InWrapper");

	public event Action<string, string> OnObjectClicked;

	private void Awake()
	{
		_localizationManager = Languages.ActiveLocProvider;
	}

	public void SetPet(PetLevel pet, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.PetId = pet.PetName;
		assetLookupSystem.Blackboard.PetLevel = pet.Level;
		assetLookupSystem.Blackboard.PetVariantId = pet.VariantId;
		PetPayload payload = assetLookupSystem.TreeLoader.LoadTree<PetPayload>().GetPayload(assetLookupSystem.Blackboard);
		if (payload == null)
		{
			Debug.LogError("Missing asset definition for pet " + pet.PetName + "!");
			return;
		}
		_pet = pet;
		SetTextFromLevel();
		_petAnchor.DestroyChildren();
		Animator componentInChildren = AssetLoader.Instantiate(payload.WrapperPrefab, _petAnchor).GetComponentInChildren<Animator>();
		if (componentInChildren != null && componentInChildren.gameObject.activeSelf)
		{
			if (componentInChildren.ContainsParameter(InWrapper))
			{
				componentInChildren.SetBool(InWrapper, value: true);
			}
			componentInChildren.Play(Wrapper_Hover);
			SetUpHover(componentInChildren);
		}
	}

	private void SetUpHover(Animator petAnimator)
	{
		_petHitbox.OnMouseover.AddListener(delegate
		{
			petAnimator.SetBool(InWrapper, value: true);
			petAnimator.SetBool(Wrapper_Hover, value: true);
		});
		_petHitbox.OnMouseoff.AddListener(delegate
		{
			petAnimator.SetBool(InWrapper, value: true);
			petAnimator.SetBool(Wrapper_Hover, value: false);
		});
	}

	private AnimatorControllerParameterType GetParameterType(string parameterName, Animator animator)
	{
		for (int i = 0; i < animator.parameters.Length; i++)
		{
			AnimatorControllerParameter parameter = animator.GetParameter(i);
			if (parameter.name == parameterName)
			{
				return parameter.type;
			}
		}
		return (AnimatorControllerParameterType)(-1);
	}

	private void SetTextFromLevel()
	{
		if (_pet.PetName == "IKO_BattlePass")
		{
			_text.SetText("MainNav/PetNames/" + _pet.PetName + "_" + _pet.Level);
			return;
		}
		switch (_pet.Level)
		{
		case 1:
			if (_pet.PetName == "M20_BattlePass")
			{
				_text.SetText("MainNav/BattlePass/M20/Pet/Common_Elemental_Cat");
			}
			else if (_pet.PetName == "ELD_BattlePass")
			{
				_text.SetText("MainNav/BattlePass/ELD/Pet/Common_Fae_Fox");
			}
			else
			{
				SetNameFromLocalizedPetName();
			}
			break;
		case 2:
			if (_pet.PetName == "M20_BattlePass")
			{
				_text.SetText("MainNav/BattlePass/M20/Pet/Uncommon_Elemental_Cat");
			}
			else if (_pet.PetName == "ELD_BattlePass")
			{
				_text.SetText("MainNav/BattlePass/ELD/Pet/Uncommon_Fae_Fox");
			}
			else
			{
				SetNameFromLocalizedPetName();
			}
			break;
		case 3:
			if (_pet.PetName == "M20_BattlePass")
			{
				_text.SetText("MainNav/BattlePass/M20/Pet/Rare_Elemental_Cat");
			}
			else if (_pet.PetName == "ELD_BattlePass")
			{
				_text.SetText("MainNav/BattlePass/ELD/Pet/Rare_Fae_Fox");
			}
			else
			{
				SetNameFromLocalizedPetName();
			}
			break;
		default:
			SetNameFromLocalizedPetName();
			break;
		}
	}

	private void SetNameFromLocalizedPetName()
	{
		string key = PetUtils.KeyForPetDetails(_pet, _localizationManager);
		_text.SetText(key);
	}

	public void OnApplyButtonPressed()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		this.OnObjectClicked?.Invoke(_pet.PetName, _pet.VariantId);
	}
}
