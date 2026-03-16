using StatsMonitor.Core;
using StatsMonitor.Util;
using UnityEngine;

namespace StatsMonitor;

[DisallowMultipleComponent]
public class StatsMonitorWrapper : MonoBehaviour
{
	public const string WRAPPER_NAME = "Stats Monitor";

	internal static readonly Anchors anchors = new Anchors();

	private StatsMonitor _statsMonitor;

	private Canvas _canvas;

	private static StatsMonitorWrapper _instance;

	private static StatsMonitorWrapper InternalInstance
	{
		get
		{
			if (_instance == null)
			{
				StatsMonitorWrapper statsMonitorWrapper = Object.FindObjectOfType<StatsMonitorWrapper>();
				if (statsMonitorWrapper != null)
				{
					_instance = statsMonitorWrapper;
				}
				else
				{
					new GameObject("Stats Monitor").AddComponent<StatsMonitorWrapper>();
				}
			}
			return _instance;
		}
	}

	public static StatsMonitor TargetInstance
	{
		get
		{
			if (!(InternalInstance != null))
			{
				return null;
			}
			return InternalInstance._statsMonitor;
		}
	}

	private StatsMonitorWrapper()
	{
	}

	public static StatsMonitorWrapper AddToScene()
	{
		return InternalInstance;
	}

	public void SetRenderMode(RenderMode renderMode)
	{
		switch (renderMode)
		{
		case RenderMode.Overlay:
			_canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
			_canvas.sortingOrder = 32767;
			break;
		case RenderMode.Camera:
			_canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;
			_canvas.worldCamera = Camera.current ?? Camera.main;
			_canvas.sortingLayerName = SortingLayer.layers[SortingLayer.layers.Length - 1].name;
			_canvas.sortingOrder = 32767;
			break;
		}
	}

	private void CreateUI()
	{
		_canvas = base.gameObject.AddComponent<Canvas>();
		_canvas.pixelPerfect = true;
		RectTransform component = base.gameObject.GetComponent<RectTransform>();
		component.pivot = Vector2.up;
		component.anchorMin = Vector2.up;
		component.anchorMax = Vector2.up;
		component.anchoredPosition = new Vector2(0f, 0f);
		base.transform.Find("Stats Monitor").gameObject.SetActive(value: true);
		_statsMonitor = Object.FindObjectOfType<StatsMonitor>();
		_statsMonitor.wrapper = this;
	}

	private void DisposeInternal()
	{
		if (_statsMonitor != null)
		{
			_statsMonitor.Dispose();
		}
		Object.Destroy(this);
		if (_instance == this)
		{
			_instance = null;
		}
	}

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			CreateUI();
			SetRenderMode(_statsMonitor.RenderMode);
			Utils.AddToUILayer(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}
}
