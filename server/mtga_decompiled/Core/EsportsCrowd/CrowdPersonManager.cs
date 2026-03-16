using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace EsportsCrowd;

public class CrowdPersonManager : MonoBehaviour
{
	[SerializeField]
	private CrowdSettings _settings;

	private List<CrowdPersonBehaviour> _crowd;

	private void Start()
	{
		CombatAnimationPlayer.DamageDealtByCard += OnDamageDealtByCard;
		ZoneTransferUXEvent.ZoneTransferExecuted += OnZoneTransferExecuted;
		_crowd = new List<CrowdPersonBehaviour>(GetComponentsInChildren<CrowdPersonBehaviour>());
		_crowd.ForEach(delegate(CrowdPersonBehaviour x)
		{
			x.Init();
		});
		_crowd.Shuffle();
	}

	private void OnDestroy()
	{
		CombatAnimationPlayer.DamageDealtByCard -= OnDamageDealtByCard;
		ZoneTransferUXEvent.ZoneTransferExecuted -= OnZoneTransferExecuted;
	}

	private void OnZoneTransferExecuted(MtgCardInstance affector, ZoneTransferReason reason)
	{
		int b = affector?.Colors.Count ?? 1;
		float value = GetHypeFromZoneTransferReason(reason) / (float)Mathf.Max(1, b);
		foreach (CardColor? cardColor in GetCardColors(affector))
		{
			HandleHypeEvent(new HypeEvent(cardColor, value));
		}
	}

	private void OnDamageDealtByCard(MtgCardInstance affector, int amount)
	{
		int b = affector?.Colors.Count ?? 1;
		float value = GetHypeFromDamageAmount(amount) / (float)Mathf.Max(1, b);
		foreach (CardColor? cardColor in GetCardColors(affector))
		{
			HandleHypeEvent(new HypeEvent(cardColor, value));
		}
	}

	private void HandleHypeEvent(HypeEvent hypeEvent)
	{
		int num = Mathf.RoundToInt(_settings.CrowdParticipation * (float)_crowd.Count);
		for (int i = 0; i < num; i++)
		{
			HypeEvent hypeEvent2 = new HypeEvent(hypeEvent);
			_crowd[i].HandleHypeEvent(hypeEvent2);
		}
		_crowd.Shuffle();
	}

	private float GetHypeFromZoneTransferReason(ZoneTransferReason reason)
	{
		switch (reason)
		{
		case ZoneTransferReason.Resolve:
			return 0.5f;
		case ZoneTransferReason.Countered:
			return 0.75f;
		case ZoneTransferReason.Destroy:
		case ZoneTransferReason.Damage:
		case ZoneTransferReason.Deathtouch:
			return 0.25f;
		case ZoneTransferReason.Exile:
		case ZoneTransferReason.Bounce:
			return 0.5f;
		default:
			return 0.1f;
		}
	}

	private float GetHypeFromDamageAmount(int damage)
	{
		return (float)damage / 5f;
	}

	private IEnumerable<CardColor?> GetCardColors(MtgCardInstance card)
	{
		if (card == null)
		{
			yield return null;
			yield break;
		}
		if (card.Colors.Count == 0)
		{
			yield return CardColor.Colorless;
			yield break;
		}
		foreach (CardColor color in card.Colors)
		{
			yield return color;
		}
	}
}
