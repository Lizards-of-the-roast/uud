using System;
using UnityEngine;
using Wotc.Mtga.DuelScene;

namespace Wotc.Mtga.Battlefield;

public class IncrementPlayerLifeEvent : MonoBehaviour
{
	private ISignalListen<IncrementPlayerLifeSignalArgs> _signalListener;

	public event Action<uint, int> LifeTotalUpdated;

	public void SetListener(ISignalListen<IncrementPlayerLifeSignalArgs> signalListener)
	{
		ClearListener();
		_signalListener = signalListener;
		if (signalListener != null)
		{
			_signalListener.Listeners += OnLifeTotalUpdated;
		}
	}

	private void OnLifeTotalUpdated(IncrementPlayerLifeSignalArgs args)
	{
		OnLifeTotalUpdated(args.PlayerId, args.Amount);
	}

	private void OnLifeTotalUpdated(uint playerId, int amount)
	{
		this.LifeTotalUpdated?.Invoke(playerId, amount);
	}

	private void ClearListener()
	{
		if (_signalListener != null)
		{
			_signalListener.Listeners -= OnLifeTotalUpdated;
			_signalListener = null;
		}
	}

	private void OnDestroy()
	{
		ClearListener();
		this.LifeTotalUpdated = null;
	}
}
