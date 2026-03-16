using System;
using System.Collections;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Client.Logging;
using Wotc.Mtga.Extensions;

namespace Core.Shared.Code.Connection;

public class ConnectionIndicator : MonoBehaviour, IConnectionIndicator, IDisposable
{
	private GameObject _reconnectIndicator;

	private Coroutine _hideReconnectIndicatorCoroutine;

	private DateTime _hideReconnectIndicatorTime;

	private const float _hideReconnectIndicatorDelay = 0.5f;

	private AssetLookupSystem _assetLookupSystem;

	private Wizards.Arena.Client.Logging.ILogger _logger;

	private bool _isEnabled;

	public static ConnectionIndicator Create()
	{
		GameObject obj = new GameObject("ConnectionIndicator");
		UnityEngine.Object.DontDestroyOnLoad(obj);
		Canvas canvas = obj.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 4;
		CanvasScaler canvasScaler = obj.AddComponent<CanvasScaler>();
		canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
		canvasScaler.matchWidthOrHeight = 1f;
		obj.AddComponent<GraphicRaycaster>();
		return obj.AddComponent<ConnectionIndicator>();
	}

	public void Initialize(AssetLookupSystem assetLookupSystem, Wizards.Arena.Client.Logging.ILogger logger)
	{
		_assetLookupSystem = assetLookupSystem;
		_logger = logger;
	}

	public void Dispose()
	{
		if (base.gameObject != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void ShowReconnectIndicator(bool shouldEnable)
	{
		if (shouldEnable == _isEnabled)
		{
			return;
		}
		_isEnabled = shouldEnable;
		if (_reconnectIndicator == null)
		{
			if (AssetBundleManager.AssetBundlesActive && AssetBundleManager.Instance != null && !AssetBundleManager.Instance.Initialized)
			{
				_logger.Warn("Loading indicator not loaded, AssetBundles not ready yet.");
				return;
			}
			string prefabPath = _assetLookupSystem.GetPrefabPath<LoadingPanelPrefab, GameObject>();
			if (prefabPath != null)
			{
				GameObject gameObject = AssetLoader.Instantiate(prefabPath, base.transform);
				if ((object)gameObject != null)
				{
					_reconnectIndicator = gameObject;
					goto IL_008b;
				}
			}
			_logger.Error("Loading indicator not loaded!");
			return;
		}
		goto IL_008b;
		IL_008b:
		if (_hideReconnectIndicatorCoroutine != null)
		{
			StopCoroutine(_hideReconnectIndicatorCoroutine);
		}
		if (shouldEnable)
		{
			_reconnectIndicator.UpdateActive(active: true);
			return;
		}
		_hideReconnectIndicatorTime = DateTime.UtcNow + TimeSpan.FromSeconds(0.5);
		_hideReconnectIndicatorCoroutine = StartCoroutine(HideReconnectIndicator());
	}

	private IEnumerator HideReconnectIndicator()
	{
		yield return new WaitUntil(() => DateTime.UtcNow >= _hideReconnectIndicatorTime);
		_reconnectIndicator.UpdateActive(active: false);
	}
}
