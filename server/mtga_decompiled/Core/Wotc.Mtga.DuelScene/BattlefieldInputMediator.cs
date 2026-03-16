using System;
using System.Collections.Generic;
using InteractionSystem;
using Pooling;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class BattlefieldInputMediator : IDisposable
{
	private readonly IObjectPool _objectPool;

	private readonly IUnityObjectPool _unityPool;

	private readonly GameInteractionSystem _gameInteractionSystem;

	private readonly List<BattlefieldInput> _battlefieldInputs = new List<BattlefieldInput>();

	public BattlefieldInputMediator(IObjectPool objectPool, IUnityObjectPool unityPool, GameInteractionSystem gameInteractionSystem)
	{
		_objectPool = objectPool;
		_unityPool = unityPool;
		_gameInteractionSystem = gameInteractionSystem;
		_battlefieldInputs = _objectPool.PopObject<List<BattlefieldInput>>();
		BattlefieldInput[] array = UnityEngine.Object.FindObjectsOfType<BattlefieldInput>();
		foreach (BattlefieldInput battlefieldInput in array)
		{
			_battlefieldInputs.Add(battlefieldInput);
			battlefieldInput.SetInteractionSystem(_gameInteractionSystem);
			battlefieldInput.SetUnityPool(_unityPool);
		}
	}

	public void Dispose()
	{
		while (_battlefieldInputs.Count > 0)
		{
			_battlefieldInputs[0].SetInteractionSystem(null);
			_battlefieldInputs[0].SetUnityPool(NullUnityObjectPool.Default);
			_battlefieldInputs.RemoveAt(0);
		}
		_objectPool.PushObject(_battlefieldInputs);
	}
}
