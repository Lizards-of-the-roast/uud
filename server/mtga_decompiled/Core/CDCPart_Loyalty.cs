using GreClient.CardData;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

public class CDCPart_Loyalty : CDCPart
{
	[SerializeField]
	private TextMeshPro _loyaltyLabel;

	private bool _shouldBeVisible = true;

	public virtual void SetVisible(bool visible)
	{
		_shouldBeVisible = visible;
		bool flag = _shouldBeVisible && !_cachedDestroyed;
		if (base.transform.gameObject.activeSelf != flag)
		{
			base.transform.gameObject.SetActive(visible);
		}
	}

	protected override void HandleDestructionInternal()
	{
		_loyaltyLabel.gameObject.SetActive(!_cachedDestroyed);
		base.HandleDestructionInternal();
	}

	protected override void HandleUpdateInternal()
	{
		if (_cachedModel.Counters.ContainsKey(CounterType.Loyalty))
		{
			_loyaltyLabel.SetText(_cachedModel.Counters[CounterType.Loyalty].ToString());
			return;
		}
		if (_cachedModel.Zone != null && _cachedModel.Zone.Type == ZoneType.Battlefield)
		{
			_loyaltyLabel.SetText("0");
			return;
		}
		MtgCardInstance instance = _cachedModel.Instance;
		if ((instance == null || !instance.Loyalty.HasValue) && _cachedModel.Printing != null)
		{
			_loyaltyLabel.SetText(_cachedModel.Printing.Toughness.RawText);
		}
		else
		{
			_loyaltyLabel.SetText(LoyaltyString(_cachedModel));
		}
	}

	private static string LoyaltyString(ICardDataAdapter model)
	{
		if (model == null)
		{
			return string.Empty;
		}
		if (IsCompleated(model))
		{
			return (model.Loyalty - (uint)(2 * model.Instance.ActiveAbilityWords.Find((AbilityWordData x) => x.AbilityGrpId == 238).Values[0])).ToString();
		}
		return model.Loyalty.ToString();
	}

	private static bool IsCompleated(ICardDataAdapter model)
	{
		if (model != null && model.Instance?.ActiveAbilityWords != null && model.Printing != null)
		{
			return model.Instance.ActiveAbilityWords.Exists((AbilityWordData x) => x.AbilityGrpId == 238);
		}
		return false;
	}

	public override void HandleCleanup()
	{
		SetVisible(visible: true);
		base.HandleCleanup();
	}
}
