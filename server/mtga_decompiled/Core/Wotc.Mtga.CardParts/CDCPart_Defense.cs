using GreClient.CardData;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.CardParts;

public class CDCPart_Defense : CDCPart
{
	[SerializeField]
	private TextMeshPro _defenseLabel;

	private bool _shouldBeVisible = true;

	public virtual void SetVisible(bool visible)
	{
		_shouldBeVisible = visible;
		bool active = _shouldBeVisible && !_cachedDestroyed;
		base.transform.gameObject.UpdateActive(active);
	}

	protected override void HandleDestructionInternal()
	{
		_defenseLabel.gameObject.SetActive(!_cachedDestroyed);
		base.HandleDestructionInternal();
	}

	protected override void HandleUpdateInternal()
	{
		_defenseLabel.SetText(GetDefenseText(_cachedModel));
	}

	private static string GetDefenseText(ICardDataAdapter model)
	{
		if (model == null)
		{
			return string.Empty;
		}
		if (model.Counters.TryGetValue(CounterType.Defense, out var value))
		{
			return value.ToString();
		}
		if (model.Zone != null && model.ZoneType == ZoneType.Battlefield)
		{
			return "0";
		}
		MtgCardInstance instance = model.Instance;
		if ((instance == null || !instance.Defense.HasValue) && model.Printing != null)
		{
			return model.Printing.Toughness.RawText;
		}
		return "0";
	}

	public override void HandleCleanup()
	{
		SetVisible(visible: true);
		base.HandleCleanup();
	}
}
