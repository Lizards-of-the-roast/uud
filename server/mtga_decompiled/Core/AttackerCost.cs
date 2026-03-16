using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Extensions;

public class AttackerCost : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public enum HoverState
	{
		Start,
		End
	}

	public Action<AttackerCost, HoverState> HoverStateChanged;

	[SerializeField]
	private TMP_Text AttackerCostText;

	private IReadOnlyDictionary<uint, List<uint>> _costSourcesByAbility;

	public bool ShouldHighlightCostSource { get; set; }

	private void Awake()
	{
		AttackerCostText.SetText(string.Empty);
		Disable();
	}

	private void OnDestroy()
	{
		HoverStateChanged = null;
	}

	public void SetManaCostSource(IReadOnlyDictionary<uint, List<uint>> costSourcesByAbility)
	{
		_costSourcesByAbility = costSourcesByAbility;
	}

	public void SetCostText(string costString)
	{
		Enable();
		AttackerCostText.SetText(costString);
	}

	public void Enable()
	{
		base.gameObject.UpdateActive(active: true);
	}

	public void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	public List<uint> GetAbilityCostSources(uint abilityId)
	{
		if (_costSourcesByAbility == null || !_costSourcesByAbility.ContainsKey(abilityId))
		{
			return new List<uint>();
		}
		return _costSourcesByAbility[abilityId];
	}

	public uint? GetFirstCostSource()
	{
		if (_costSourcesByAbility == null)
		{
			return null;
		}
		foreach (List<uint> value in _costSourcesByAbility.Values)
		{
			if (value.Count > 0)
			{
				return value[0];
			}
		}
		return null;
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		HoverStateChanged?.Invoke(this, HoverState.Start);
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		HoverStateChanged?.Invoke(this, HoverState.End);
	}
}
