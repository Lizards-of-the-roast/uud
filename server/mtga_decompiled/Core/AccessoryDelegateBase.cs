using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga.Inventory;

public abstract class AccessoryDelegateBase : AccessoryController
{
	public GameObject[] Variants;

	protected AccessoryController[] _accessoryControllers;

	protected AccessoryController _activeAccessory;

	protected Collider[] _variantColliders;

	protected ClientPetSelection _petSelection;

	protected int _petVariantIdx;

	protected int VariantIdx;

	protected EventTrigger[] _evtTriggerClicks;

	protected float _variantChangeCooldown;

	protected float _variantChangeCooldownTime = 1f;

	public string animStateToPlayOnVariantChange = "idle";

	public AccessoryController ActiveAccessory => _activeAccessory;

	public override void Update()
	{
		_variantChangeCooldown -= Time.deltaTime;
	}

	public virtual void HandleOpponentVariantChange(int idx)
	{
		SetAccessoryVariant(idx);
	}

	protected virtual void HandleVariantChange(GREPlayerNum inputSource)
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

	protected void SendVariantChange(AccessoryInteraction item)
	{
		if ((GREPlayerNum.LocalPlayer == _ownerPlayerNum || !DebugMode) && _gameManager != null)
		{
			_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", item.ToString());
		}
	}

	public virtual void SetNextVariant()
	{
		if ((_petSelection == null || _cosmetics == null) && !debugScene)
		{
			return;
		}
		for (int i = _petVariantIdx + 1; i < _petVariantIdx + _accessoryControllers.Length; i++)
		{
			int num = i % _accessoryControllers.Length;
			if (!debugScene && !_cosmetics.IsPetAvailable(_petSelection.name, $"Skin{num + 1}"))
			{
				continue;
			}
			_petVariantIdx = num;
			SetAccessoryVariant(_petVariantIdx);
			_variantChangeCooldown = _variantChangeCooldownTime;
			Component[] componentsInChildren = GetComponentsInChildren<Animator>();
			componentsInChildren = componentsInChildren;
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				Animator animator = (Animator)componentsInChildren[j];
				if (hasState(animator, animStateToPlayOnVariantChange))
				{
					animator.Play(animStateToPlayOnVariantChange);
				}
			}
			break;
		}
	}

	public abstract void SetAccessoryVariant(int variantIdx);

	public override void OnGlobalMuteChanged(bool globalMuted)
	{
		if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
		{
			return;
		}
		bool muted = base._muted;
		_globalMuted = globalMuted;
		bool muted2 = base._muted;
		if (muted != muted2)
		{
			if (base._muted)
			{
				HandleGlobalMuteOn();
			}
			else
			{
				HandleGlobalMuteOff();
			}
		}
	}

	public override void OnPlayerMuteChanged(bool playerMuted)
	{
		if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
		{
			return;
		}
		bool muted = base._muted;
		_playerMuted = playerMuted;
		bool muted2 = base._muted;
		if (muted != muted2)
		{
			if (base._muted)
			{
				HandlePlayerMuteOn();
			}
			else
			{
				HandlePlayerMuteOff();
			}
		}
	}

	public override void HandleLocalEmote()
	{
		_activeAccessory.HandleLocalEmote();
	}

	public override void HandleOpponentEmote()
	{
		if ((bool)_activeAccessory)
		{
			_activeAccessory.HandleOpponentEmote();
		}
	}

	public void HandlePlayerMuteOn()
	{
		if ((bool)_activeAccessory)
		{
			_activeAccessory.SetPlayerMuted(playerMuted: true);
			_activeAccessory.HandleMuteOn();
		}
	}

	public void HandlePlayerMuteOff()
	{
		if ((bool)_activeAccessory)
		{
			_activeAccessory.SetPlayerMuted(playerMuted: false);
			_activeAccessory.HandleMuteOff();
		}
	}

	public void HandleGlobalMuteOn()
	{
		if ((bool)_activeAccessory)
		{
			_activeAccessory.SetGlobalMuted(globalMuted: true);
			_activeAccessory.HandleMuteOn();
		}
	}

	public void HandleGlobalMuteOff()
	{
		if ((bool)_activeAccessory)
		{
			_activeAccessory.SetGlobalMuted(globalMuted: false);
			_activeAccessory.HandleMuteOff();
		}
	}

	public bool hasState(Animator anim, string stateName, int layer = 0)
	{
		int stateID = Animator.StringToHash(stateName);
		return anim.HasState(layer, stateID);
	}
}
