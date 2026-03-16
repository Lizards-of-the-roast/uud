using GreClient.CardData;
using UnityEngine;
using UnityEngine.Events;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class BattlefieldPhenomenonReactions : MonoBehaviour
{
	private UXEventQueue _evtQueue;

	[Header("Ability Reactions")]
	[SerializeField]
	protected AbilityType _abilityType;

	[SerializeField]
	protected AbilityWord _abilityWord;

	[SerializeField]
	protected UnityEvent _abilityTriggeredEvent;

	public void Init(UXEventQueue evtQueue)
	{
		_evtQueue = evtQueue ?? new UXEventQueue();
		_evtQueue.EventExecutionCompleted += OnEventExecutionCompleted;
	}

	private void OnEventExecutionCompleted(UXEvent uxEventComplete)
	{
		if (uxEventComplete is ResolutionEventEndedUXEvent resolutionEventEndedUXEvent)
		{
			bool num = resolutionEventEndedUXEvent.AbilityPrinting?.ReferencedAbilityTypes.Contains(_abilityType) ?? false;
			bool flag = resolutionEventEndedUXEvent.AbilityPrinting?.AbilityWord.Equals(_abilityWord) ?? false;
			bool flag2 = resolutionEventEndedUXEvent.CardPrinting?.Abilities.Exists(_abilityType, (AbilityPrintingData ability, AbilityType abilityType) => ability.ReferencedAbilityTypes.Contains(abilityType)) ?? false;
			if (num || flag2 || flag)
			{
				_abilityTriggeredEvent.Invoke();
			}
		}
	}

	public void OnDestroy()
	{
		_evtQueue.EventExecutionCompleted -= OnEventExecutionCompleted;
		_evtQueue = null;
	}
}
