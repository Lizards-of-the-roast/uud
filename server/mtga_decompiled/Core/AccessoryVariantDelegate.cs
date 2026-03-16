using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Providers;

public class AccessoryVariantDelegate : AccessoryDelegateBase
{
	private enum VariantChange
	{
		VariantChange_0,
		VariantChange_1,
		VariantChange_2,
		VariantChange_3,
		VariantChange_4
	}

	public override void Init(GameManager gameManager, GREPlayerNum playerNum, CosmeticsProvider cosmetics = null, ClientPetSelection petSelection = null)
	{
		_gameManager = gameManager;
		_ownerPlayerNum = playerNum;
		_cosmetics = cosmetics;
		_petSelection = petSelection;
		_accessoryControllers = new AccessoryController[Variants.Length];
		_variantColliders = new Collider[Variants.Length];
		_evtTriggerClicks = new EventTrigger[Variants.Length];
		for (int i = 0; i < Variants.Length; i++)
		{
			_accessoryControllers[i] = Variants[i].GetComponent<AccessoryController>();
			Variants[i].SetActive(value: false);
		}
		for (int j = 0; j < _accessoryControllers.Length; j++)
		{
			_accessoryControllers[j].Init(gameManager, playerNum, cosmetics, petSelection);
			_variantColliders[j] = _accessoryControllers[j].VariantChangeCollider;
			_variantColliders[j].gameObject.AddComponent<EventTrigger>();
			_evtTriggerClicks[j] = _variantColliders[j].gameObject.GetComponent<EventTrigger>();
			EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
			triggerEvent.AddListener(delegate
			{
				HandleVariantChange(GREPlayerNum.LocalPlayer);
			});
			EventTrigger.Entry item = new EventTrigger.Entry
			{
				callback = triggerEvent,
				eventID = EventTriggerType.PointerClick
			};
			_evtTriggerClicks[j].triggers.Add(item);
		}
		if (_cosmetics?.PlayerPetSelection != null)
		{
			if (int.TryParse(Regex.Match(petSelection.variant, "\\d+").Value, out var result))
			{
				_petVariantIdx = result - 1;
			}
			else
			{
				_petVariantIdx = 0;
			}
			SetAccessoryVariant(_petVariantIdx);
		}
		if (_gameManager != null && _ownerPlayerNum == GREPlayerNum.LocalPlayer)
		{
			_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", string.Format("{0}_{1}", "VariantChange", _petVariantIdx.ToString()));
		}
		SetAccessoryVariant(_petVariantIdx);
		if (_ownerPlayerNum == GREPlayerNum.Opponent)
		{
			UpdateUIMessageHandler();
			if (overrideOpponentTransforms)
			{
				SetTransforms(opponentPosition, opponentRotation);
			}
			SetOpponentAnimController();
			if (MirrorOnOpponentSide)
			{
				MirrorTransform();
			}
		}
		else if (overrideLocalTransforms)
		{
			SetTransforms(localPosition, localRotation);
		}
		OnGlobalMuteChanged(MDNPlayerPrefs.DisableEmotes);
		SubscribeToMuteEvents();
	}

	public override void Update()
	{
		_variantChangeCooldown -= Time.deltaTime;
	}

	protected override void OnGenericEventReceived(string category, string payload)
	{
		if (!MDNPlayerPrefs.DisableEmotes && !base._muted && !(_gameManager == null) && string.CompareOrdinal(category, "AccessoryInteraction") == 0 && Enum.TryParse<VariantChange>(payload, ignoreCase: true, out var result))
		{
			switch (result)
			{
			case VariantChange.VariantChange_0:
				HandleOpponentVariantChange(0);
				break;
			case VariantChange.VariantChange_1:
				HandleOpponentVariantChange(1);
				break;
			case VariantChange.VariantChange_2:
				HandleOpponentVariantChange(2);
				break;
			case VariantChange.VariantChange_3:
				HandleOpponentVariantChange(3);
				break;
			case VariantChange.VariantChange_4:
				HandleOpponentVariantChange(4);
				break;
			}
		}
	}

	public override void HandleOpponentVariantChange(int idx)
	{
		if (!base._muted || _ownerPlayerNum != GREPlayerNum.Opponent)
		{
			SetAccessoryVariant(idx);
		}
	}

	protected override void HandleVariantChange(GREPlayerNum inputSource)
	{
		if (_ownerPlayerNum == GREPlayerNum.LocalPlayer && !(_variantChangeCooldown > 0f))
		{
			SetNextVariant();
			if (_gameManager != null)
			{
				_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", string.Format("{0}_{1}", "VariantChange", _petVariantIdx.ToString()));
			}
		}
	}

	public override void SetAccessoryVariant(int variantIdx)
	{
		variantIdx = Mathf.Clamp(variantIdx, 0, Variants.Length);
		if (Variants.Length != 0)
		{
			GameObject[] variants = Variants;
			for (int i = 0; i < variants.Length; i++)
			{
				variants[i].UpdateActive(active: false);
			}
			Variants[variantIdx].UpdateActive(active: true);
			_activeAccessory = _accessoryControllers[variantIdx];
		}
	}
}
