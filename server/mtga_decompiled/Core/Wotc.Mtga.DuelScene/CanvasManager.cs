using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene;

public class CanvasManager
{
	private Camera _cam;

	private const float DEFAULT_SCREEN_SPACE_CAMERA_DEPTH = 38f;

	private const float DEFAULT_SCREEN_SPACE_CAMERA_DEPTH_MOBILE = 37f;

	private const float STACK_DEPTH = 22.75f;

	private readonly Transform _uiRoot;

	private Transform _overlayRoot;

	private Transform _screenSpaceRoot;

	private Dictionary<RenderMode, Dictionary<CanvasLayer, Dictionary<string, Transform>>> _roots = new Dictionary<RenderMode, Dictionary<CanvasLayer, Dictionary<string, Transform>>>();

	private Dictionary<CanvasLayer, GameObject> _screenSpaceCanvasRoots = new Dictionary<CanvasLayer, GameObject>();

	private readonly HashSet<CanvasGroup> _canvasGroups = new HashSet<CanvasGroup>();

	private readonly Dictionary<CanvasLayer, RenderMode> _canavsLayerToRenderModeMap = new Dictionary<CanvasLayer, RenderMode>
	{
		{
			CanvasLayer.ScreenSpace_Default,
			RenderMode.ScreenSpaceCamera
		},
		{
			CanvasLayer.ScreenSpace_UnderStack,
			RenderMode.ScreenSpaceCamera
		},
		{
			CanvasLayer.ScreenSpace_UnderStack_ScaledCorrectly,
			RenderMode.ScreenSpaceCamera
		},
		{
			CanvasLayer.Overlay,
			RenderMode.ScreenSpaceOverlay
		}
	};

	private const string SORTLAYERNAME_FOREGROUND = "Foreground";

	private const string SORTLAYERNAME_DEFAULT = "Default";

	private readonly Dictionary<CanvasLayer, string> _canvasLayerToSortingLayer = new Dictionary<CanvasLayer, string>
	{
		{
			CanvasLayer.ScreenSpace_Default,
			"Default"
		},
		{
			CanvasLayer.ScreenSpace_UnderStack,
			"Default"
		},
		{
			CanvasLayer.ScreenSpace_UnderStack_ScaledCorrectly,
			"Foreground"
		}
	};

	private readonly Dictionary<CanvasLayer, int> _canvasLayerToSortingOrder = new Dictionary<CanvasLayer, int>
	{
		{
			CanvasLayer.ScreenSpace_Default,
			100
		},
		{
			CanvasLayer.ScreenSpace_UnderStack,
			0
		},
		{
			CanvasLayer.ScreenSpace_UnderStack_ScaledCorrectly,
			0
		}
	};

	public CanvasManager(Camera cam)
	{
		_cam = cam;
		_uiRoot = new GameObject("UIRoot").transform;
		_uiRoot.ZeroOut();
	}

	public Transform GetCanvasRoot(CanvasLayer canvasLayer, string rootKey = "Default", bool allowScreenSafe = true)
	{
		RenderMode renderMode = _canavsLayerToRenderModeMap[canvasLayer];
		if (!_roots.ContainsKey(renderMode))
		{
			return CreateCanvas(renderMode, canvasLayer, rootKey, allowScreenSafe);
		}
		if (!_roots[renderMode].ContainsKey(canvasLayer))
		{
			return CreateCanvas(renderMode, canvasLayer, rootKey, allowScreenSafe);
		}
		if (!_roots[renderMode][canvasLayer].ContainsKey(rootKey))
		{
			return CreateCanvasRoot(renderMode, canvasLayer, rootKey, allowScreenSafe);
		}
		return _roots[renderMode][canvasLayer][rootKey];
	}

	public void SetCanvasInputEnabled(bool enabled)
	{
		foreach (CanvasGroup canvasGroup in _canvasGroups)
		{
			canvasGroup.interactable = enabled;
			canvasGroup.blocksRaycasts = enabled;
		}
	}

	private Transform CreateCanvas(RenderMode renderMode, CanvasLayer canvasLayer, string rootKey, bool allowScreenSafe = true)
	{
		Transform transform = null;
		if (renderMode == RenderMode.ScreenSpaceCamera && _screenSpaceRoot == null)
		{
			_screenSpaceRoot = new GameObject("Screen Space - Camera").transform;
			_screenSpaceRoot.SetParent(_uiRoot);
			_screenSpaceRoot.ZeroOut();
		}
		switch (canvasLayer)
		{
		case CanvasLayer.ScreenSpace_Default:
		{
			GameObject gameObject2 = new GameObject("Default");
			gameObject2.transform.SetParent(_screenSpaceRoot);
			gameObject2.transform.ZeroOut();
			Canvas canvas2 = CreateScreenSpaceCameraCanvas(gameObject2, PlatformUtils.IsHandheld() ? 37f : 38f, _cam, getReferenceResolution());
			if (_canvasLayerToSortingLayer.TryGetValue(canvasLayer, out var value3))
			{
				canvas2.sortingLayerName = value3;
			}
			if (_canvasLayerToSortingOrder.TryGetValue(canvasLayer, out var value4))
			{
				canvas2.sortingOrder = value4;
			}
			transform = canvas2.transform;
			_screenSpaceCanvasRoots.Add(canvasLayer, gameObject2);
			break;
		}
		case CanvasLayer.ScreenSpace_UnderStack:
		case CanvasLayer.ScreenSpace_UnderStack_ScaledCorrectly:
		{
			GameObject gameObject = new GameObject(getRootName());
			gameObject.transform.SetParent(_screenSpaceRoot);
			gameObject.transform.ZeroOut();
			Canvas canvas = CreateScreenSpaceCameraCanvas(gameObject, 22.75f, _cam, getReferenceResolution());
			if (_canvasLayerToSortingLayer.TryGetValue(canvasLayer, out var value))
			{
				canvas.sortingLayerName = value;
			}
			if (_canvasLayerToSortingOrder.TryGetValue(canvasLayer, out var value2))
			{
				canvas.sortingOrder = value2;
			}
			transform = canvas.transform;
			MaskableGraphicRaycaster component = transform.GetComponent<MaskableGraphicRaycaster>();
			component.blockingObjects = GraphicRaycaster.BlockingObjects.All;
			LayerMask layerMask = LayerMask.GetMask("Hand");
			LayerMask layerMask2 = LayerMask.GetMask("CardsExamine");
			LayerMask layerMask3 = LayerMask.GetMask("Browser");
			LayerMask layerMask4 = (int)layerMask | (int)layerMask2 | (int)layerMask3;
			component.SetLayerMask(layerMask4);
			_screenSpaceCanvasRoots.Add(canvasLayer, gameObject);
			break;
		}
		default:
			_overlayRoot = CreateScreenSpaceOverlayCanvas(_uiRoot).transform;
			transform = _overlayRoot;
			break;
		}
		Transform transform2 = CreateStretchedRectTransform(transform, rootKey, allowScreenSafe);
		if (!_roots.ContainsKey(renderMode))
		{
			_roots.Add(renderMode, new Dictionary<CanvasLayer, Dictionary<string, Transform>>());
		}
		if (!_roots[renderMode].ContainsKey(canvasLayer))
		{
			_roots[renderMode].Add(canvasLayer, new Dictionary<string, Transform>());
		}
		if (!_roots[renderMode][canvasLayer].ContainsKey(rootKey))
		{
			_roots[renderMode][canvasLayer].Add(rootKey, transform2);
		}
		return transform2;
		Vector2 getReferenceResolution()
		{
			if (canvasLayer == CanvasLayer.ScreenSpace_UnderStack_ScaledCorrectly)
			{
				return new Vector2(1920f, 1080f);
			}
			if (canvasLayer == CanvasLayer.ScreenSpace_Default && PlatformUtils.IsHandheld())
			{
				return new Vector2(1920f, 1080f);
			}
			return new Vector2(800f, 600f);
		}
		string getRootName()
		{
			if (canvasLayer == CanvasLayer.ScreenSpace_UnderStack_ScaledCorrectly)
			{
				return "UnderStack_ScaledCorrectly";
			}
			return "UnderStack";
		}
	}

	private Transform CreateCanvasRoot(RenderMode renderMode, CanvasLayer canvasLayer, string rootKey, bool allowScreenSafe = true)
	{
		Transform transform = null;
		transform = ((renderMode == RenderMode.ScreenSpaceOverlay || renderMode != RenderMode.ScreenSpaceCamera) ? CreateStretchedRectTransform(_overlayRoot, rootKey, allowScreenSafe) : CreateStretchedRectTransform(_screenSpaceCanvasRoots[canvasLayer].transform, rootKey, allowScreenSafe));
		if (!_roots.ContainsKey(renderMode))
		{
			_roots.Add(renderMode, new Dictionary<CanvasLayer, Dictionary<string, Transform>>());
		}
		if (!_roots[renderMode].ContainsKey(canvasLayer))
		{
			_roots[renderMode].Add(canvasLayer, new Dictionary<string, Transform>());
		}
		_roots[renderMode][canvasLayer].Add(rootKey, transform);
		return transform;
	}

	private Canvas CreateScreenSpaceOverlayCanvas(Transform root, string canvasName = "Screen Space - Overlay")
	{
		GameObject gameObject = new GameObject(canvasName);
		gameObject.transform.SetParent(root);
		gameObject.transform.ZeroOut();
		Canvas canvas = gameObject.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
		canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		canvasScaler.referenceResolution = new Vector2(800f, 600f);
		canvasScaler.defaultSpriteDPI = 100f;
		canvasScaler.fallbackScreenDPI = 100f;
		canvasScaler.matchWidthOrHeight = 1f;
		canvasScaler.referencePixelsPerUnit = 100f;
		gameObject.AddComponent<GraphicRaycaster>();
		_canvasGroups.Add(gameObject.AddComponent<CanvasGroup>());
		return canvas;
	}

	private Canvas CreateScreenSpaceCameraCanvas(GameObject rootObj, float depth, Camera mainCamera, Vector2 referenceResolution)
	{
		Canvas canvas = rootObj.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.planeDistance = depth;
		canvas.worldCamera = mainCamera;
		CanvasScaler canvasScaler = rootObj.AddComponent<CanvasScaler>();
		canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		canvasScaler.referenceResolution = referenceResolution;
		canvasScaler.defaultSpriteDPI = 100f;
		canvasScaler.fallbackScreenDPI = 100f;
		canvasScaler.matchWidthOrHeight = 1f;
		canvasScaler.referencePixelsPerUnit = 100f;
		rootObj.AddComponent<MaskableGraphicRaycaster>();
		_canvasGroups.Add(rootObj.AddComponent<CanvasGroup>());
		return canvas;
	}

	private RectTransform CreateStretchedRectTransform(Transform parent, string name, bool allowScreenSafe = true)
	{
		RectTransform rectTransform = new GameObject(name).AddComponent<RectTransform>();
		rectTransform.SetParent(parent);
		rectTransform.ZeroOut();
		rectTransform.anchorMin = new Vector2(0f, 0f);
		rectTransform.anchorMax = new Vector2(1f, 1f);
		rectTransform.offsetMin = Vector2.zero;
		rectTransform.offsetMax = Vector2.zero;
		if (PlatformUtils.IsHandheld() && allowScreenSafe)
		{
			ScreenSafeArea screenSafeArea = rectTransform.gameObject.AddComponent<ScreenSafeArea>();
			screenSafeArea.ConformY = false;
			screenSafeArea.Refresh(ScreenEventController.Instance.GetSafeArea());
		}
		return rectTransform;
	}
}
