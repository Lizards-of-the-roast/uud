using System.Collections.Generic;
using System.Linq;
using Pooling;
using UnityEngine;
using Wizards.Mtga;

namespace Wotc.Mtga.DuelScene.VFX;

public static class LoopingAnimationManager
{
	private class LoopingEffectEntry
	{
		public Transform Space;

		public string Key;

		public bool SurvivesZoneTransfer;

		public GameObject Instance;
	}

	private static IUnityObjectPool _unityObjectPool;

	private static readonly List<LoopingEffectEntry> _activeLoopingEffects = new List<LoopingEffectEntry>();

	private static IUnityObjectPool UnityObjectPool
	{
		get
		{
			if (_unityObjectPool == null)
			{
				_unityObjectPool = Pantry.Get<IUnityObjectPool>();
			}
			return _unityObjectPool;
		}
	}

	public static IEnumerable<string> ActiveLoopingKeys()
	{
		return _activeLoopingEffects.Select((LoopingEffectEntry x) => x.Key);
	}

	public static bool IsLoopRunning(Transform space, string loopKey)
	{
		CleanupEffectList();
		return _activeLoopingEffects.Find((LoopingEffectEntry x) => x.Space == space && x.Key == loopKey) != null;
	}

	public static bool AddLoopingEffect(Transform space, string loopKey, bool survivesZoneTransfer, GameObject instance)
	{
		if (space == null)
		{
			Debug.LogError("Cannot add a looping animation without providing the parenting space.");
			return false;
		}
		CleanupEffectList();
		LoopingEffectEntry loopingEffectEntry = _activeLoopingEffects.Find((LoopingEffectEntry x) => x.Space == space && x.Key == loopKey);
		if (loopingEffectEntry == null && instance == null)
		{
			Debug.LogError("Cannot add a new looping animation without providing the VFX instance.");
			return false;
		}
		if (loopingEffectEntry == null)
		{
			List<LoopingEffectEntry> activeLoopingEffects = _activeLoopingEffects;
			LoopingEffectEntry obj = new LoopingEffectEntry
			{
				Space = space,
				Key = loopKey,
				SurvivesZoneTransfer = survivesZoneTransfer,
				Instance = instance
			};
			loopingEffectEntry = obj;
			activeLoopingEffects.Add(obj);
		}
		return true;
	}

	public static void RemoveLoopingEffect(Transform space, string loopKey)
	{
		if (space == null)
		{
			Debug.LogError("Cannot remove a looping animation without providing the parenting space.");
			return;
		}
		CleanupEffectList();
		RemoveEffectsFromActive(_activeLoopingEffects.FindAll((LoopingEffectEntry x) => x.Space == space && x.Key == loopKey));
	}

	public static void RemoveLoopingEffect(GameObject instance)
	{
		CleanupEffectList();
		_activeLoopingEffects.RemoveAll((LoopingEffectEntry x) => x.Instance == instance);
	}

	public static void RemoveAllLoopingEffects(Transform space)
	{
		if (space == null)
		{
			Debug.LogError("Cannot remove a looping animation without providing the parenting space.");
			return;
		}
		CleanupEffectList();
		RemoveEffectsFromActive(_activeLoopingEffects.FindAll((LoopingEffectEntry x) => x.Space == space));
	}

	public static void RemoveAllLoopingEffectsDuringZoneTransfer(Transform space)
	{
		if (space == null)
		{
			Debug.LogError("Cannot remove a looping animation without providing the parenting space.");
			return;
		}
		CleanupEffectList();
		List<LoopingEffectEntry> list = _activeLoopingEffects.FindAll((LoopingEffectEntry x) => x.Space == space && !x.SurvivesZoneTransfer);
		if (list.Count > 0)
		{
			AudioManager.StopSFX(space.gameObject);
		}
		RemoveEffectsFromActive(list);
	}

	public static bool IsLoopingInstance(GameObject instance)
	{
		CleanupEffectList();
		return _activeLoopingEffects.Exists((LoopingEffectEntry x) => x.Instance == instance);
	}

	private static void RemoveEffectsFromActive(List<LoopingEffectEntry> effects)
	{
		CleanupEffectList();
		foreach (LoopingEffectEntry effect in effects)
		{
			_activeLoopingEffects.Remove(effect);
			if (effect.Instance != null)
			{
				AudioManager.StopSFX(effect.Instance);
				UnityObjectPool.PushObject(effect.Instance);
			}
		}
	}

	private static void CleanupEffectList()
	{
		_activeLoopingEffects.RemoveAll((LoopingEffectEntry x) => x == null || x.Instance == null || x.Space == null);
	}
}
