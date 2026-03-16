using System;
using System.Collections.Generic;
using AssetLookupTree;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

[RequireComponent(typeof(RectTransform))]
public class TooltipSystem : MonoBehaviour, ITooltipDisplay, IDisposable
{
	public enum TooltipStyle
	{
		Default,
		DefaultLeftAlignedText,
		Prompt,
		TextAlignedToLeft,
		TextAlignedToLeftNoAutoOff
	}

	public enum TooltipPositionAnchor
	{
		Center,
		TopLeft,
		TopCenter,
		TopRight,
		MiddleRight,
		BottomRight,
		BottomCenter,
		BottomLeft,
		MiddleLeft
	}

	private class Tooltip
	{
		private CanvasGroup _panel;

		private TextMeshProUGUI _text;

		public bool AutoOffWhenNoMouseInput = true;

		public RectTransform Rect { get; private set; }

		public TooltipPositionAnchor Anchor { get; private set; }

		public TooltipProperties Properties { get; private set; }

		public GameObject SourceObject { get; private set; }

		public Tooltip(RectTransform parent, GameObject prefab)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, parent);
			gameObject.transform.localScale = Vector3.one;
			Image componentInChildren = gameObject.GetComponentInChildren<Image>();
			Rect = (((object)componentInChildren != null) ? componentInChildren.rectTransform : gameObject.GetComponent<RectTransform>());
			_panel = gameObject.GetComponent<CanvasGroup>();
			_text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
			Display(show: false);
		}

		public void SetUp(GameObject sourceObject, string text, TooltipPositionAnchor anchor, TooltipProperties properties)
		{
			SourceObject = sourceObject;
			Anchor = anchor;
			Properties = properties;
			SetAndSizeTextRect(text, properties);
		}

		public void Display(bool show)
		{
			_panel.alpha = (show ? 1 : 0);
		}

		private void SetAndSizeTextRect(string text, TooltipProperties properties)
		{
			if (_text.maxVisibleLines != properties.MaxVisibleLines)
			{
				_text.maxVisibleLines = properties.MaxVisibleLines;
			}
			if (_text.fontSize != properties.FontSize)
			{
				_text.fontSize = properties.FontSize;
			}
			_text.SetText(text);
			Vector2 preferredValues = _text.GetPreferredValues(text);
			preferredValues.x += properties.Padding.x;
			preferredValues.y += properties.Padding.y;
			_text.rectTransform.sizeDelta = preferredValues;
			Rect.sizeDelta = preferredValues;
		}
	}

	[SerializeField]
	private GameObject _defaultTooltipPrefab;

	[SerializeField]
	private GameObject _defaultLeftAlignedTextTooltipPrefab;

	[SerializeField]
	private GameObject _promptTooltipPrefab;

	[SerializeField]
	private GameObject _textAlignedToLeftTooltipPrefab;

	private RectTransform _rect;

	private IBILogger _biLogger;

	private Dictionary<TooltipStyle, Tooltip> _tooltipObjects;

	private Dictionary<TooltipStyle, TooltipProperties> _defaultProperties = new Dictionary<TooltipStyle, TooltipProperties>();

	private Dictionary<GameObject, TooltipData> _dynamicTooltipData = new Dictionary<GameObject, TooltipData>();

	private Dictionary<GameObject, TooltipProperties> _dynamicTooltipProperties = new Dictionary<GameObject, TooltipProperties>();

	private Tooltip _pendingTooltip;

	private Tooltip _activeTooltip;

	private float _pendingTooltipHoverTime;

	protected IClientLocProvider _locMan;

	public bool IsDisplaying
	{
		get
		{
			if (_pendingTooltip == null && _activeTooltip == null)
			{
				return _dynamicTooltipData.Count > 0;
			}
			return true;
		}
	}

	public AssetLookupSystem AssetLookupSystem { get; private set; }

	private void Awake()
	{
		_rect = GetComponent<RectTransform>();
	}

	public void Init(AssetLookupSystem assetLookupSystem, IBILogger biLogger, IClientLocProvider locMan)
	{
		AssetLookupSystem = assetLookupSystem;
		_biLogger = biLogger;
		_locMan = locMan;
	}

	private void Start()
	{
		_tooltipObjects = new Dictionary<TooltipStyle, Tooltip>
		{
			{
				TooltipStyle.Default,
				new Tooltip(_rect, _defaultTooltipPrefab)
			},
			{
				TooltipStyle.DefaultLeftAlignedText,
				new Tooltip(_rect, _defaultLeftAlignedTextTooltipPrefab)
			},
			{
				TooltipStyle.Prompt,
				new Tooltip(_rect, _promptTooltipPrefab)
			},
			{
				TooltipStyle.TextAlignedToLeft,
				new Tooltip(_rect, _textAlignedToLeftTooltipPrefab)
			},
			{
				TooltipStyle.TextAlignedToLeftNoAutoOff,
				new Tooltip(_rect, _textAlignedToLeftTooltipPrefab)
				{
					AutoOffWhenNoMouseInput = false
				}
			}
		};
		_defaultProperties = new Dictionary<TooltipStyle, TooltipProperties>
		{
			{
				TooltipStyle.Default,
				new TooltipProperties()
			},
			{
				TooltipStyle.Prompt,
				new TooltipProperties()
			}
		};
	}

	private void Update()
	{
		if (!IsDisplaying)
		{
			return;
		}
		PointerEventData lastPointerEventData = CustomInputModule.GetLastPointerEventData();
		if (lastPointerEventData == null)
		{
			return;
		}
		TooltipProperties tooltipProperties;
		Camera camera;
		if (_activeTooltip != null)
		{
			if (shouldDisableTooltip(_activeTooltip))
			{
				DisableTooltip();
			}
		}
		else if (_pendingTooltip != null)
		{
			if (shouldDisableTooltip(_pendingTooltip))
			{
				DisableTooltip();
				return;
			}
			tooltipProperties = _pendingTooltip.Properties;
			if (tooltipProperties != null)
			{
				if (_pendingTooltipHoverTime >= tooltipProperties.HoverDurationUntilShow)
				{
					camera = lastPointerEventData.enterEventCamera;
					_pendingTooltipHoverTime = 0f;
					_activeTooltip = _pendingTooltip;
					_pendingTooltip = null;
					_activeTooltip.Rect.position = calcTooltipPosition(_activeTooltip);
					_activeTooltip.Display(show: true);
				}
				else
				{
					_pendingTooltipHoverTime += Time.deltaTime;
				}
			}
		}
		else
		{
			if (_dynamicTooltipData.Count <= 0)
			{
				return;
			}
			foreach (GameObject key in _dynamicTooltipData.Keys)
			{
				if (lastPointerEventData.hovered.Contains(key))
				{
					_dynamicTooltipProperties.TryGetValue(key, out var value);
					DisplayTooltip(key, _dynamicTooltipData[key], value);
				}
			}
		}
		Vector2 calcTooltipPosition(Tooltip tooltip)
		{
			Vector3[] array = new Vector3[4];
			bool flag = false;
			if (tooltipProperties.UseMousePosition)
			{
				flag = true;
			}
			else
			{
				RectTransform component = tooltip.SourceObject.GetComponent<RectTransform>();
				if (component != null)
				{
					Canvas componentInParent = tooltip.SourceObject.GetComponentInParent<Canvas>();
					if (componentInParent != null && componentInParent.renderMode == RenderMode.ScreenSpaceOverlay)
					{
						component.GetWorldCorners(array);
					}
					else if (camera == null)
					{
						flag = true;
					}
					else
					{
						Vector3[] array2 = new Vector3[4];
						component.GetWorldCorners(array2);
						for (int i = 0; i < array2.Length; i++)
						{
							array[i] = camera.WorldToScreenPoint(array2[i]);
						}
					}
				}
				else if (camera == null)
				{
					flag = true;
				}
				else
				{
					Collider componentInChildren = tooltip.SourceObject.GetComponentInChildren<Collider>();
					Bounds bounds = new Bounds(tooltip.SourceObject.transform.position, Vector3.zero);
					if (componentInChildren != null)
					{
						bounds.Encapsulate(componentInChildren.bounds);
					}
					array[0] = camera.WorldToScreenPoint(bounds.min);
					array[1] = camera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y));
					array[2] = camera.WorldToScreenPoint(bounds.max);
					array[3] = camera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y));
				}
			}
			if (flag)
			{
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = CustomInputModule.GetPointerPosition();
				}
			}
			Vector2 vector = new Vector2((array[0].x + array[2].x) * 0.5f, (array[0].y + array[2].y) * 0.5f);
			float width = array[2].x - array[0].x;
			float height = array[2].y - array[0].y;
			Vector2 vector2 = getAnchorOffset(width, height, tooltip.Anchor);
			Vector3[] array3 = new Vector3[4];
			tooltip.Rect.GetWorldCorners(array3);
			float width2 = array3[2].x - array3[0].x;
			float height2 = array3[2].y - array3[0].y;
			Vector2 vector3 = getAlignOffset(width2, height2, tooltipProperties.TooltipAlignment);
			Vector2 screenPoint = clampPosition(width2, height2, vector + vector2 + vector3 + tooltipProperties.Offset);
			RectTransformUtility.ScreenPointToWorldPointInRectangle(_rect, screenPoint, null, out var worldPoint);
			return worldPoint;
			Vector2 getAlignOffset(float num2, float num4, TooltipProperties.Alignment alignment)
			{
				if (alignment == TooltipProperties.Alignment.Default)
				{
					alignment = getDefaultAlignment(tooltip.Anchor);
				}
				float num = num2 * 0.5f;
				float num3 = num4 * 0.5f;
				return alignment switch
				{
					TooltipProperties.Alignment.Center => Vector2.zero, 
					TooltipProperties.Alignment.TopLeft => new Vector2(num, 0f - num3), 
					TooltipProperties.Alignment.TopCenter => new Vector2(0f, 0f - num3), 
					TooltipProperties.Alignment.TopRight => new Vector2(0f - num, 0f - num3), 
					TooltipProperties.Alignment.MiddleRight => new Vector2(0f - num, 0f), 
					TooltipProperties.Alignment.BottomRight => new Vector2(0f - num, num3), 
					TooltipProperties.Alignment.BottomCenter => new Vector2(0f, num3), 
					TooltipProperties.Alignment.BottomLeft => new Vector2(num, num3), 
					TooltipProperties.Alignment.MiddleLeft => new Vector2(num, 0f), 
					_ => Vector2.zero, 
				};
			}
		}
		static Vector2 clampPosition(float width, float height, Vector3 pos)
		{
			float num = width * 0.5f;
			pos.x = Mathf.Clamp(pos.x, num, (float)Screen.width - num);
			float num2 = height * 0.5f;
			pos.y = Mathf.Clamp(pos.y, num2, (float)Screen.height - num2);
			return pos;
		}
		static Vector2 getAnchorOffset(float width, float height, TooltipPositionAnchor anchor)
		{
			float num = width * 0.5f;
			float num2 = height * 0.5f;
			return anchor switch
			{
				TooltipPositionAnchor.TopLeft => new Vector2(0f - num, num2), 
				TooltipPositionAnchor.TopCenter => new Vector2(0f, num2), 
				TooltipPositionAnchor.TopRight => new Vector2(num, num2), 
				TooltipPositionAnchor.MiddleRight => new Vector2(num, 0f), 
				TooltipPositionAnchor.BottomRight => new Vector2(num, 0f - num2), 
				TooltipPositionAnchor.BottomCenter => new Vector2(0f, 0f - num2), 
				TooltipPositionAnchor.BottomLeft => new Vector2(0f - num, 0f - num2), 
				TooltipPositionAnchor.MiddleLeft => new Vector2(0f - num, 0f), 
				_ => Vector2.zero, 
			};
		}
		TooltipProperties.Alignment getDefaultAlignment(TooltipPositionAnchor positionAnchor)
		{
			switch (positionAnchor)
			{
			case TooltipPositionAnchor.Center:
				return TooltipProperties.Alignment.Center;
			case TooltipPositionAnchor.TopLeft:
				return TooltipProperties.Alignment.BottomRight;
			case TooltipPositionAnchor.TopCenter:
				return TooltipProperties.Alignment.BottomCenter;
			case TooltipPositionAnchor.TopRight:
				return TooltipProperties.Alignment.BottomLeft;
			case TooltipPositionAnchor.MiddleRight:
				return TooltipProperties.Alignment.MiddleLeft;
			case TooltipPositionAnchor.BottomRight:
				return TooltipProperties.Alignment.TopLeft;
			case TooltipPositionAnchor.BottomCenter:
				return TooltipProperties.Alignment.TopCenter;
			case TooltipPositionAnchor.BottomLeft:
				return TooltipProperties.Alignment.TopRight;
			case TooltipPositionAnchor.MiddleLeft:
				return TooltipProperties.Alignment.MiddleRight;
			default:
			{
				UnknownTooltipAnchor payload = new UnknownTooltipAnchor
				{
					EventTime = DateTime.UtcNow,
					Anchor = positionAnchor.ToString(),
					Error = "Unimplemented anchor to alignment translation"
				};
				_biLogger.Send(ClientBusinessEventType.UnknownTooltipAnchor, payload);
				return TooltipProperties.Alignment.Center;
			}
			}
		}
		static bool shouldDisableTooltip(Tooltip tooltip)
		{
			if ((bool)tooltip.SourceObject)
			{
				if (tooltip.AutoOffWhenNoMouseInput)
				{
					return !CustomInputModule.GetHovered().Exists((GameObject gameObject) => gameObject == tooltip.SourceObject);
				}
				return false;
			}
			return true;
		}
	}

	public void DisplayTooltip(GameObject source, TooltipData data, TooltipProperties properties = null)
	{
		data.LocMan = _locMan;
		if (!string.IsNullOrEmpty(data.Text))
		{
			if (properties == null)
			{
				properties = _defaultProperties[data.TooltipStyle];
			}
			DisableTooltip();
			_pendingTooltip = _tooltipObjects[data.TooltipStyle];
			string text = data.Text;
			if (!string.IsNullOrEmpty(text))
			{
				text = text.Replace("\\r", "\r").Replace("\\n", "\n");
			}
			_pendingTooltip.SetUp(source, text, data.RelativePosition, properties);
		}
	}

	public void AddDynamicTooltip(GameObject source, TooltipData data, TooltipProperties properties = null)
	{
		if (_dynamicTooltipData.ContainsKey(source))
		{
			_dynamicTooltipData[source] = data;
		}
		else
		{
			_dynamicTooltipData.Add(source, data);
		}
		if (properties != null)
		{
			if (_dynamicTooltipProperties.ContainsKey(source))
			{
				_dynamicTooltipProperties[source] = properties;
			}
			else
			{
				_dynamicTooltipProperties.Add(source, properties);
			}
		}
	}

	public void RemoveDynamicTooltip(GameObject source)
	{
		if (_dynamicTooltipData.ContainsKey(source))
		{
			_dynamicTooltipData.Remove(source);
		}
		if (_dynamicTooltipProperties.ContainsKey(source))
		{
			_dynamicTooltipProperties.Remove(source);
		}
		if (_activeTooltip != null && _activeTooltip.SourceObject == source)
		{
			DisableTooltip();
		}
		else if (_pendingTooltip != null && _pendingTooltip.SourceObject == source)
		{
			DisableTooltip();
		}
	}

	public void DisableTooltip()
	{
		_pendingTooltipHoverTime = 0f;
		if (_pendingTooltip != null)
		{
			_pendingTooltip = null;
		}
		if (_activeTooltip != null)
		{
			_activeTooltip.Display(show: false);
			_activeTooltip = null;
		}
	}

	public void Dispose()
	{
		if ((bool)base.gameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
