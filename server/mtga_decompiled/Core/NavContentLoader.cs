using System;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Achievements;
using UnityEngine;
using Wizards.Mtga.Platforms;

public class NavContentLoader
{
	private class NavContentData
	{
		public NavContentController _contentControllerRef;

		private NavContentType _contentType;

		private bool _canLoadDynamically;

		private Func<NavContentController> _initAction;

		private Func<NavContentType, NavContentType, bool> _destroyAction;

		public NavContentData(NavContentType type, bool allowDynamicLoad = false, Func<NavContentController> initFunction = null, Func<NavContentType, NavContentType, bool> destroyFunction = null)
		{
			_contentType = type;
			_canLoadDynamically = allowDynamicLoad;
			_initAction = initFunction;
			_destroyAction = destroyFunction;
		}

		public NavContentController InitContent()
		{
			return _initAction?.Invoke();
		}

		public bool DestroyContent(NavContentType currentType, NavContentType nextType, bool dynamicLoadingEnabled)
		{
			if (!_canLoadDynamically || !dynamicLoadingEnabled)
			{
				return false;
			}
			return _destroyAction?.Invoke(currentType, nextType) ?? false;
		}
	}

	private Dictionary<NavContentType, NavContentData> _navContentDatas = new Dictionary<NavContentType, NavContentData>();

	private bool UseDynamicLoading => PlatformUtils.IsHandheld();

	public void SetController(NavContentType type, NavContentController controller)
	{
		_navContentDatas[type]._contentControllerRef = controller;
	}

	public NavContentController GetController(NavContentType type)
	{
		NavContentData navContentData = _navContentDatas[type];
		if (navContentData._contentControllerRef != null)
		{
			return navContentData._contentControllerRef;
		}
		switch (type)
		{
		case NavContentType.LearnToPlay:
			navContentData._contentControllerRef = UnityEngine.Object.FindObjectOfType<LearnToPlayControllerV2>();
			if (navContentData._contentControllerRef == null)
			{
				navContentData._contentControllerRef = navContentData.InitContent();
			}
			break;
		case NavContentType.Achievements:
			navContentData._contentControllerRef = UnityEngine.Object.FindObjectOfType<AchievementsContentController>();
			if (navContentData._contentControllerRef == null)
			{
				navContentData._contentControllerRef = navContentData.InitContent();
			}
			break;
		case NavContentType.Profile:
			navContentData._contentControllerRef = UnityEngine.Object.FindObjectOfType<ProfileContentController>();
			if (navContentData._contentControllerRef == null)
			{
				navContentData._contentControllerRef = navContentData.InitContent();
			}
			break;
		case NavContentType.DeckListViewer:
			navContentData._contentControllerRef = UnityEngine.Object.FindObjectOfType<DeckManagerController>();
			if (navContentData._contentControllerRef == null)
			{
				navContentData._contentControllerRef = navContentData.InitContent();
			}
			break;
		case NavContentType.BoosterChamber:
			navContentData._contentControllerRef = UnityEngine.Object.FindObjectOfType<BoosterChamberController>();
			if (navContentData._contentControllerRef == null)
			{
				navContentData._contentControllerRef = navContentData.InitContent();
			}
			break;
		case NavContentType.RewardTrack:
			navContentData._contentControllerRef = UnityEngine.Object.FindObjectOfType<ProgressionTracksContentController>();
			if (navContentData._contentControllerRef == null)
			{
				navContentData._contentControllerRef = navContentData.InitContent();
			}
			break;
		case NavContentType.Store:
			navContentData._contentControllerRef = UnityEngine.Object.FindObjectOfType<ContentController_StoreCarousel>();
			if (navContentData._contentControllerRef == null)
			{
				navContentData._contentControllerRef = navContentData.InitContent();
			}
			break;
		case NavContentType.Home:
			navContentData._contentControllerRef = UnityEngine.Object.FindObjectOfType<HomePageContentController>();
			if (navContentData._contentControllerRef == null)
			{
				navContentData._contentControllerRef = navContentData.InitContent();
			}
			break;
		case NavContentType.DeckBuilder:
			navContentData._contentControllerRef = UnityEngine.Object.FindObjectOfType<WrapperDeckBuilder>();
			if (navContentData._contentControllerRef == null)
			{
				navContentData._contentControllerRef = navContentData.InitContent();
			}
			break;
		default:
			navContentData._contentControllerRef = navContentData.InitContent();
			break;
		}
		return navContentData._contentControllerRef;
	}

	public T GetController<T>(NavContentType type) where T : NavContentController
	{
		return (T)GetController(type);
	}

	public void UnloadNavContent(NavContentType nextContentType, NavContentType currentContentType)
	{
		if (UseDynamicLoading)
		{
			bool flag = false;
			NavContentData navContentData = null;
			if (_navContentDatas.TryGetValue(currentContentType, out var value))
			{
				navContentData = value;
				flag = navContentData.DestroyContent(currentContentType, nextContentType, UseDynamicLoading);
			}
			if (flag)
			{
				UnityEngine.Object.Destroy(navContentData._contentControllerRef.gameObject);
				navContentData._contentControllerRef = null;
				Resources.UnloadUnusedAssets();
				GC.Collect();
			}
		}
	}

	public void AddNavContentData(NavContentType type, bool canDynamicLoad = false, Func<NavContentController> initFunc = null, Func<NavContentType, NavContentType, bool> destroyFunc = null)
	{
		if (!_navContentDatas.ContainsKey(type))
		{
			_navContentDatas.Add(type, new NavContentData(type, canDynamicLoad, initFunc, destroyFunc));
		}
	}

	public bool HasNavContentController(NavContentType type)
	{
		if (!_navContentDatas.ContainsKey(type))
		{
			Debug.Log("Data for this type does not have an entry in the dictionary\nCall AddNavContentData to initialize one");
			return false;
		}
		return _navContentDatas[type]._contentControllerRef != null;
	}

	public void CleanupContent()
	{
		foreach (KeyValuePair<NavContentType, NavContentData> navContentData in _navContentDatas)
		{
			if (navContentData.Value != null && navContentData.Value._contentControllerRef != null)
			{
				UnityEngine.Object.Destroy(navContentData.Value._contentControllerRef);
				navContentData.Value._contentControllerRef = null;
			}
		}
		_navContentDatas.Clear();
	}
}
