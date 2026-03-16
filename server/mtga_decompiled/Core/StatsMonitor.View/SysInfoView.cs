using StatsMonitor.Core;
using StatsMonitor.Util;
using UnityEngine;
using UnityEngine.UI;

namespace StatsMonitor.View;

internal class SysInfoView : View2D
{
	private readonly StatsMonitor _statsMonitor;

	private int _width;

	private int _height;

	private Text _text;

	private bool _isDirty;

	internal SysInfoView(StatsMonitor statsMonitor)
	{
		_statsMonitor = statsMonitor;
		Invalidate();
	}

	public override void Reset()
	{
		_text.text = "";
	}

	public override void Update()
	{
		if (_isDirty)
		{
			string text = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorSysInfoOdd) + ">OS:" + SystemInfo.operatingSystem + "</color>\n<color=#" + Utils.Color32ToHex(_statsMonitor.colorSysInfoEven) + ">CPU:" + SystemInfo.processorType + " [" + SystemInfo.processorCount + " cores]</color>\n<color=#" + Utils.Color32ToHex(_statsMonitor.colorSysInfoOdd) + ">GRAPHICS:" + SystemInfo.graphicsDeviceName + "\nAPI:" + SystemInfo.graphicsDeviceVersion + "\nShader Level:" + SystemInfo.graphicsShaderLevel + ", Video RAM:" + SystemInfo.graphicsMemorySize + " MB</color>\n<color=#" + Utils.Color32ToHex(_statsMonitor.colorSysInfoEven) + ">SYSTEM RAM:" + SystemInfo.systemMemorySize + " MB</color>\n<color=#" + Utils.Color32ToHex(_statsMonitor.colorSysInfoOdd) + ">SCREEN:" + Screen.currentResolution.width + " x " + Screen.currentResolution.height + " @" + Screen.currentResolution.refreshRate + "Hz,\nwindow size:" + Screen.width + " x " + Screen.height + " " + Screen.dpi + "dpi</color>";
			_text.text = text;
			_height = _statsMonitor.padding + (int)_text.preferredHeight + _statsMonitor.padding;
			Invalidate(ViewInvalidationType.Layout);
			_statsMonitor.Invalidate(ViewInvalidationType.Layout, InvalidationFlag.Text, invalidateChildren: false);
			_isDirty = false;
		}
	}

	public override void Dispose()
	{
		View2D.Destroy(_text);
		_text = null;
		base.Dispose();
	}

	internal void SetWidth(float width)
	{
		_width = (int)width;
	}

	protected override GameObject CreateChildren()
	{
		GameObject obj = new GameObject();
		obj.name = "SysInfoView";
		obj.transform.parent = _statsMonitor.transform;
		GraphicsFactory graphicsFactory = new GraphicsFactory(obj, _statsMonitor.colorFPS, _statsMonitor.fontFace, _statsMonitor.fontSizeSmall);
		_text = graphicsFactory.Text("Text", "", null, 0, null, fitH: false);
		return obj;
	}

	protected override void UpdateStyle()
	{
		_text.font = _statsMonitor.fontFace;
		_text.fontSize = _statsMonitor.FontSizeSmall;
		if (_statsMonitor.colorOutline.a > 0f)
		{
			GraphicsFactory.AddOutlineAndShadow(_text.gameObject, _statsMonitor.colorOutline);
		}
		else
		{
			GraphicsFactory.RemoveEffects(_text.gameObject);
		}
		_isDirty = true;
	}

	protected override void UpdateLayout()
	{
		int padding = _statsMonitor.padding;
		_text.rectTransform.anchoredPosition = new Vector2(padding, -padding);
		_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _width - padding * 2);
		_height = padding + (int)_text.preferredHeight + padding;
		SetRTransformValues(0f, 0f, _width, _height, Vector2.one);
		_isDirty = true;
	}
}
