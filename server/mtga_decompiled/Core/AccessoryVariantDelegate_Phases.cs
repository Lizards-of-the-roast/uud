using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Providers;

public class AccessoryVariantDelegate_Phases : AccessoryDelegateBase
{
	private enum VariantChange
	{
		PhaseTransition
	}

	[Serializable]
	public class AccessoryPhase
	{
		public string Name = string.Empty;

		[Space]
		public GameObject thisVariant;

		public Animator thisAnimator;

		public GameObject[] hideOnTransition;

		public GameObject nextVariant;

		public Animator nextAnimator;

		private SkinnedMeshRenderer[] next_skinnedMeshRenderers;

		public void Init()
		{
			next_skinnedMeshRenderers = nextVariant.GetComponentsInChildren<SkinnedMeshRenderer>();
		}

		public void TransitionFromOpponent()
		{
			thisAnimator.SetTrigger("TransitionTo");
		}

		public void TransitionStart()
		{
			thisAnimator.SetTrigger("TransitionTo");
		}

		public void TransitionWarmUp()
		{
			nextVariant.SetActive(value: true);
			EnableNextObjects(val: false);
			nextAnimator.SetTrigger("TransitionFrom");
		}

		public void TransitionExecute()
		{
			thisVariant.SetActive(value: false);
			EnableNextObjects(val: true);
		}

		private void EnableNextObjects(bool val)
		{
			SkinnedMeshRenderer[] array = next_skinnedMeshRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = val;
			}
			GameObject[] array2 = hideOnTransition;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].UpdateActive(val);
			}
		}
	}

	public AccessoryPhase[] accessoryPhases;

	public override void Init(GameManager gameManager, GREPlayerNum playerNum, CosmeticsProvider cosmetics = null, ClientPetSelection petSelection = null)
	{
		AccessoryPhase[] array = accessoryPhases;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Init();
		}
		_gameManager = gameManager;
		_ownerPlayerNum = playerNum;
		_cosmetics = cosmetics;
		_petSelection = petSelection;
		_accessoryControllers = new AccessoryController[Variants.Length];
		_variantColliders = new Collider[Variants.Length];
		_evtTriggerClicks = new EventTrigger[Variants.Length];
		for (int j = 0; j < Variants.Length; j++)
		{
			_accessoryControllers[j] = Variants[j].GetComponent<AccessoryController>();
		}
		for (int k = 0; k < _accessoryControllers.Length; k++)
		{
			_accessoryControllers[k].Init(gameManager, playerNum, _cosmetics, petSelection);
		}
		SetAccessoryVariant(_petVariantIdx);
		if (_ownerPlayerNum == GREPlayerNum.Opponent)
		{
			UpdateUIMessageHandler();
			if (overrideOpponentTransforms)
			{
				SetTransforms(opponentPosition, opponentRotation);
			}
			if (overrideTransformsOnHandheld && PlatformUtils.IsHandheld())
			{
				if (handheldTransformsGO != null)
				{
					handheldTransformsGO.transform.localPosition = opponentPositionHandheld;
				}
				else
				{
					Debug.Log("No child with the name  <color=red>'HandheldTransforms' </color>attached");
				}
			}
			SetOpponentAnimController();
			if (MirrorOnOpponentSide)
			{
				MirrorTransform();
			}
		}
		else
		{
			if (overrideLocalTransforms)
			{
				SetTransforms(localPosition, localRotation);
			}
			if (overrideTransformsOnHandheld && PlatformUtils.IsHandheld())
			{
				GameObject gameObject = base.transform.Find("HandheldTransforms").gameObject;
				if (gameObject != null)
				{
					gameObject.transform.localPosition = localPositionHandheld;
				}
				else
				{
					Debug.Log("No child with the name  <color=red>'HandheldTransforms' </color>attached");
				}
			}
		}
		OnGlobalMuteChanged(MDNPlayerPrefs.DisableEmotes);
		SubscribeToMuteEvents();
	}

	protected override void OnGenericEventReceived(string category, string payload)
	{
		if (!MDNPlayerPrefs.DisableEmotes && !base._muted && !(_gameManager == null) && _gameManager.Context.TryGet<IEntityDialogControllerProvider>(out var result) && (!result.TryGetDialogControllerByPlayerType(_ownerPlayerNum, out var dialogController) || !dialogController.IsMuted()) && string.CompareOrdinal(category, "AccessoryInteraction") == 0 && Enum.TryParse<VariantChange>(payload, ignoreCase: true, out var result2) && result2 == VariantChange.PhaseTransition)
		{
			HandleOpponentPhaseTransition();
		}
	}

	private void HandleOpponentPhaseTransition()
	{
		accessoryPhases[_petVariantIdx].TransitionFromOpponent();
	}

	protected override void HandleVariantChange(GREPlayerNum inputSource)
	{
		if (_ownerPlayerNum == GREPlayerNum.LocalPlayer && !(_variantChangeCooldown > 0f))
		{
			SetNextVariant();
			if (_gameManager != null)
			{
				_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", $"PhaseTransition");
			}
		}
	}

	public override void SetNextVariant()
	{
		_petVariantIdx++;
		_petVariantIdx %= Variants.Length;
		SetAccessoryVariant(_petVariantIdx);
	}

	public override void SetAccessoryVariant(int variantIdx)
	{
		_activeAccessory = _accessoryControllers[variantIdx];
	}

	public void TransitionWarmUp()
	{
		accessoryPhases[_petVariantIdx].TransitionWarmUp();
		if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
		{
			_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", "PhaseTransition");
		}
	}

	public void TransitionExecute()
	{
		accessoryPhases[_petVariantIdx].TransitionExecute();
		SetNextVariant();
	}

	public void TransitionEnd()
	{
		SetNextVariant();
	}
}
