using StatsMonitor.Core;
using StatsMonitor.Util;
using UnityEngine;
using UnityEngine.UI;

namespace StatsMonitor.View;

internal class StatsView : View2D
{
	private readonly StatsMonitor _statsMonitor;

	private Text _text1;

	private Text _text2;

	private Text _text3;

	private Text _text4;

	private string[] _fpsTemplates;

	private string _fpsMinTemplate;

	private string _fpsMaxTemplate;

	private string _fpsAvgTemplate;

	private string _fxuTemplate;

	private string _msTemplate;

	private string _objTemplate;

	private string _memUnityTemplate;

	private string _memManagedTemplate;

	private string _memGfxTemplate;

	internal StatsView(StatsMonitor statsMonitor)
	{
		_statsMonitor = statsMonitor;
		Invalidate();
	}

	public override void Reset()
	{
		Text text = _text1;
		Text text2 = _text2;
		Text text3 = _text3;
		string text4 = (_text4.text = "");
		string text6 = (text3.text = text4);
		string text8 = (text2.text = text6);
		text.text = text8;
	}

	public override void Update()
	{
		_text1.text = _fpsTemplates[_statsMonitor.fpsLevel] + _statsMonitor.fps + "</color>";
		_text2.text = _fpsMinTemplate + ((_statsMonitor.fpsMin > -1) ? _statsMonitor.fpsMin : 0) + "</color>\n" + _fpsMaxTemplate + ((_statsMonitor.fpsMax > -1) ? _statsMonitor.fpsMax : 0) + "</color>";
		_text3.text = _fpsAvgTemplate + _statsMonitor.fpsAvg + "</color> " + _msTemplate + _statsMonitor.ms.ToString("F1") + "MS</color> " + _fxuTemplate + _statsMonitor.fixedUpdateRate + " </color>\n" + _objTemplate + "OBJ:" + _statsMonitor.renderedObjectCount + "/" + _statsMonitor.renderObjectCount + "/" + _statsMonitor.objectCount + "</color>";
		_text4.text = string.Format(_memUnityTemplate, _statsMonitor.memUnityReserved, _statsMonitor.memUnityUsed, _statsMonitor.memUnityFree) + string.Format(_memManagedTemplate, _statsMonitor.memManagedReserved, _statsMonitor.memManagedUsed, _statsMonitor.memManagedFree);
	}

	public override void Dispose()
	{
		View2D.Destroy(_text1);
		View2D.Destroy(_text2);
		View2D.Destroy(_text3);
		View2D.Destroy(_text4);
		_text1 = (_text2 = (_text3 = (_text4 = null)));
		base.Dispose();
	}

	protected override GameObject CreateChildren()
	{
		_fpsTemplates = new string[3];
		GameObject obj = new GameObject();
		obj.name = "StatsView";
		obj.transform.parent = _statsMonitor.transform;
		GraphicsFactory graphicsFactory = new GraphicsFactory(obj, _statsMonitor.colorFPS, _statsMonitor.fontFace, _statsMonitor.fontSizeSmall);
		_text1 = graphicsFactory.Text("Text1", "FPS:000");
		_text2 = graphicsFactory.Text("Text2", "MIN:000\nMAX:000");
		_text3 = graphicsFactory.Text("Text3", "AVG:000\n[000.0 MS]");
		_text4 = graphicsFactory.Text("Text4", "URSRV:000.0MB UUSED:000.0MB UFREE:000.0MB\nMRSRV:000.0MB MUSED:000.0MB MFREE:000.0MB\n");
		return obj;
	}

	protected override void UpdateStyle()
	{
		_text1.font = _statsMonitor.fontFace;
		_text1.fontSize = _statsMonitor.FontSizeLarge;
		_text2.font = _statsMonitor.fontFace;
		_text2.fontSize = _statsMonitor.FontSizeSmall;
		_text3.font = _statsMonitor.fontFace;
		_text3.fontSize = _statsMonitor.FontSizeSmall;
		_text4.font = _statsMonitor.fontFace;
		_text4.fontSize = _statsMonitor.FontSizeSmall;
		if (_statsMonitor.colorOutline.a > 0f)
		{
			GraphicsFactory.AddOutlineAndShadow(_text1.gameObject, _statsMonitor.colorOutline);
			GraphicsFactory.AddOutlineAndShadow(_text2.gameObject, _statsMonitor.colorOutline);
			GraphicsFactory.AddOutlineAndShadow(_text3.gameObject, _statsMonitor.colorOutline);
			GraphicsFactory.AddOutlineAndShadow(_text4.gameObject, _statsMonitor.colorOutline);
		}
		else
		{
			GraphicsFactory.RemoveEffects(_text1.gameObject);
			GraphicsFactory.RemoveEffects(_text2.gameObject);
			GraphicsFactory.RemoveEffects(_text3.gameObject);
			GraphicsFactory.RemoveEffects(_text4.gameObject);
		}
		_fpsTemplates[0] = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPS) + ">FPS:";
		_fpsTemplates[1] = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSWarning) + ">FPS:";
		_fpsTemplates[2] = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSCritical) + ">FPS:";
		_fpsMinTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSMin) + ">MIN:";
		_fpsMaxTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSMax) + ">MAX:";
		_fpsAvgTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFPSAvg) + ">AVG:";
		_fxuTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorFXD) + ">FXD:";
		_msTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorMS) + ">";
		_objTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorObjCount) + ">";
		_memUnityTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorMemUnityReserved) + ">URSRV:{0:F1}</color> <color=#" + Utils.Color32ToHex(_statsMonitor.colorMemUnityUsed) + ">UUSED:{1:F1}</color> <color=#" + Utils.Color32ToHex(_statsMonitor.colorMemUnityFree) + ">UFREE:{2:F1}</color>\n";
		_memManagedTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorMemManagedReserved) + ">MRSRV:{0:F1}</color> <color=#" + Utils.Color32ToHex(_statsMonitor.colorMemManagedUsed) + ">MUSED:{1:F1}</color> <color=#" + Utils.Color32ToHex(_statsMonitor.colorMemManagedFree) + ">MFREE:{2:F1}</color>\n";
		_memGfxTemplate = "<color=#" + Utils.Color32ToHex(_statsMonitor.colorMemGfx) + ">GFX:{0:F1}</color>\n";
	}

	protected override void UpdateLayout()
	{
		int padding = _statsMonitor.padding;
		int spacing = _statsMonitor.spacing;
		int num = _statsMonitor.spacing / 4;
		_text1.text = PadString(_text1.text, 7, 1);
		_text2.text = PadString(_text2.text.Split('\n')[0], 7, 2);
		_text3.text = PadString(_text3.text.Split('\n')[0], 20, 2);
		_text4.text = PadString(_text4.text, 39, 1);
		_text1.rectTransform.anchoredPosition = new Vector2(padding, -padding);
		int num2 = padding + (int)_text1.preferredWidth + spacing;
		_text2.rectTransform.anchoredPosition = new Vector2(num2, -padding);
		num2 += (int)_text2.preferredWidth + spacing;
		_text3.rectTransform.anchoredPosition = new Vector2(num2, -padding);
		num2 = padding;
		int num3 = (int)_text2.preferredHeight * 2;
		int num4 = padding + (((int)_text1.preferredHeight >= num3) ? ((int)_text1.preferredHeight) : num3) + num;
		_text4.rectTransform.anchoredPosition = new Vector2(num2, -num4);
		num4 += (int)_text4.preferredHeight + padding;
		float num5 = (float)padding + _text1.preferredWidth + (float)spacing + _text2.preferredWidth + (float)spacing + _text3.preferredWidth + (float)padding;
		float num6 = (float)padding + _text4.preferredWidth + (float)padding;
		int num7 = ((num5 > num6) ? ((int)num5) : ((int)num6));
		num7 = ((num7 % 2 == 0) ? num7 : (num7 + 1));
		SetRTransformValues(0f, 0f, num7, num4, Vector2.one);
	}

	private static string PadString(string s, int minChars, int numRows)
	{
		s = Utils.StripHTMLTags(s);
		if (s.Length >= minChars)
		{
			return s;
		}
		int num = minChars - s.Length;
		for (int i = 0; i < num; i++)
		{
			s += "_";
		}
		return s;
	}
}
