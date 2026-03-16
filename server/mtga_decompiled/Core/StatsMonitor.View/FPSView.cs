using StatsMonitor.Core;
using StatsMonitor.Util;
using UnityEngine;
using UnityEngine.UI;

namespace StatsMonitor.View;

internal class FPSView : View2D
{
	private readonly StatsMonitor _statsMonitor;

	private Text _text;

	private string[] _fpsTemplates;

	internal FPSView(StatsMonitor statsMonitor)
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
		_text.text = _fpsTemplates[_statsMonitor.fpsLevel] + _statsMonitor.fps + "FPS</color>";
	}

	public override void Dispose()
	{
		View2D.Destroy(_text);
		_text = null;
		base.Dispose();
	}

	protected override GameObject CreateChildren()
	{
		_fpsTemplates = new string[3];
		GameObject obj = new GameObject();
		obj.name = "FPSView";
		obj.transform.parent = _statsMonitor.transform;
		GraphicsFactory graphicsFactory = new GraphicsFactory(obj, _statsMonitor.colorFPS, _statsMonitor.fontFace, _statsMonitor.fontSizeSmall);
		_text = graphicsFactory.Text("Text", "000FPS");
		_text.alignment = TextAnchor.MiddleCenter;
		return obj;
	}

	protected override void UpdateStyle()
	{
		_text.font = _statsMonitor.fontFace;
		_text.fontSize = _statsMonitor.FontSizeLarge;
		_text.color = _statsMonitor.colorFPS;
		if (_statsMonitor.colorOutline.a > 0f)
		{
			GraphicsFactory.AddOutlineAndShadow(_text.gameObject, _statsMonitor.colorOutline);
		}
		else
		{
			GraphicsFactory.RemoveEffects(_text.gameObject);
		}
		_fpsTemplates[0] = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPS) + ">";
		_fpsTemplates[1] = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSWarning) + ">";
		_fpsTemplates[2] = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSCritical) + ">";
	}

	protected override void UpdateLayout()
	{
		int padding = _statsMonitor.padding;
		_text.rectTransform.anchoredPosition = new Vector2(padding, -padding);
		_text.rectTransform.anchoredPosition = Vector2.zero;
		RectTransform rectTransform = _text.rectTransform;
		RectTransform rectTransform2 = _text.rectTransform;
		Vector2 vector = (_text.rectTransform.pivot = new Vector2(0.5f, 0.5f));
		Vector2 anchorMin = (rectTransform2.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		int num = padding + (int)_text.preferredWidth + padding;
		int num2 = padding + (int)_text.preferredHeight + padding;
		num = ((num % 2 == 0) ? num : (num + 1));
		SetRTransformValues(0f, 0f, num, num2, Vector2.one);
	}
}
