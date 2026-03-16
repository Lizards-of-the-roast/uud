using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Achievements;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using Wizards.DynamicTimelineBinding;
using Wizards.Mtga;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementsToastManager : UIBehaviour, IDisposable
{
	[SerializeField]
	private PlayableDirector timelineDirector;

	[SerializeField]
	private BindingAssignment bindingAssignment;

	[SerializeField]
	private GameObject toastParent;

	private AchievementToast _achievementToastInstance;

	private IAchievementManager _achievementsProvider;

	private IAchievementsToastProvider _achievementsToastProvider;

	private AssetLookupManager _assetLookupManager;

	private Coroutine _currentToastProcessingCoroutine;

	private WaitUntil _waitUntilToastAnimationComplete;

	protected override void Awake()
	{
		_achievementsProvider = Pantry.Get<IAchievementManager>();
		_achievementsToastProvider = Pantry.Get<IAchievementsToastProvider>();
		_assetLookupManager = Pantry.Get<AssetLookupManager>();
		_waitUntilToastAnimationComplete = new WaitUntil(() => timelineDirector.state != PlayState.Playing);
	}

	private new IEnumerator Start()
	{
		yield return new WaitUntil(() => _achievementsProvider.CachePopulated);
		_achievementsToastProvider.ToastReceived += ProcessRemainingToasts;
		ProcessRemainingToasts();
	}

	private void ProcessRemainingToasts()
	{
		if (_currentToastProcessingCoroutine == null)
		{
			_currentToastProcessingCoroutine = StartCoroutine(ProcessRemainingToasts_Coroutine());
		}
	}

	private IEnumerator ProcessRemainingToasts_Coroutine()
	{
		int iterations = 0;
		while (_achievementsToastProvider.HasAchievementNotificationInQueue() && iterations < 10)
		{
			iterations++;
			IReadOnlyList<AchievementNotification> multipleNextAchievementNotificationsInQueue = _achievementsToastProvider.GetMultipleNextAchievementNotificationsInQueue();
			foreach (AchievementNotification item in multipleNextAchievementNotificationsInQueue)
			{
				yield return ProcessToast(item);
			}
		}
		if (_achievementToastInstance != null)
		{
			UnityEngine.Object.Destroy(_achievementToastInstance.gameObject);
			_achievementToastInstance = null;
		}
		_currentToastProcessingCoroutine = null;
	}

	private IEnumerator ProcessToast(AchievementNotification toastAchievement)
	{
		if (_achievementToastInstance == null)
		{
			AssetLookupSystem assetLookupSystem = _assetLookupManager.AssetLookupSystem;
			string prefabPath = assetLookupSystem.TreeLoader.LoadTree<AchievementToastPrefab>().GetPayload(assetLookupSystem.Blackboard).PrefabPath;
			_achievementToastInstance = AssetLoader.Instantiate<AchievementToast>(prefabPath, toastParent.transform);
			bindingAssignment.SetTimelineAndBindings();
		}
		_achievementToastInstance.SetToastData(toastAchievement);
		timelineDirector.Play();
		yield return _waitUntilToastAnimationComplete;
	}

	private void ReleaseUnmanagedResources()
	{
		if (_achievementsProvider != null)
		{
			_achievementsToastProvider.ToastReceived -= ProcessRemainingToasts;
		}
		_achievementsProvider = null;
		_achievementsToastProvider = null;
	}

	protected override void OnDestroy()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~AchievementsToastManager()
	{
		ReleaseUnmanagedResources();
	}
}
