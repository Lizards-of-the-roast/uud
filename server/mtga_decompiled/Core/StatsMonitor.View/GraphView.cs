using System;
using StatsMonitor.Core;
using StatsMonitor.Util;
using UnityEngine;
using UnityEngine.UI;

namespace StatsMonitor.View;

internal class GraphView : View2D
{
	private readonly StatsMonitor _statsMonitor;

	private RawImage _image;

	private Bitmap2D _graph;

	private int _oldWidth;

	private int _width;

	private int _height;

	private int _graphStartX;

	private int _graphMaxY;

	private int _memCeiling;

	private int _lastGCCollectionCount = -1;

	private Color?[] _fpsColors;

	public GraphView(StatsMonitor statsMonitor)
	{
		_statsMonitor = statsMonitor;
		Invalidate();
	}

	public override void Reset()
	{
		if (_graph != null)
		{
			_graph.Clear();
		}
	}

	public override void Update()
	{
		if (_graph != null)
		{
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, (int)Mathf.Ceil(_statsMonitor.memUnityReserved / (float)_memCeiling * (float)_height)), _statsMonitor.colorMemUnityReserved);
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, (int)Mathf.Ceil(_statsMonitor.memUnityUsed / (float)_memCeiling * (float)_height)), _statsMonitor.colorMemUnityUsed);
			int b = (int)Mathf.Ceil(_statsMonitor.memManagedReserved / (float)_memCeiling * (float)_height);
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, b), _statsMonitor.colorMemManagedReserved);
			int num = (int)Mathf.Ceil(_statsMonitor.memManagedUsed / (float)_memCeiling * (float)_height);
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, num), _statsMonitor.colorMemManagedUsed);
			int num2 = (int)_statsMonitor.ms >> 1;
			if (num2 == num)
			{
				num2++;
			}
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, num2), _statsMonitor.colorMS);
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, _statsMonitor.fps / ((_statsMonitor.fpsMax > 60) ? _statsMonitor.fpsMax : 60) * _graphMaxY - 1), _statsMonitor.colorFPS);
			if (_lastGCCollectionCount != GC.CollectionCount(0))
			{
				_lastGCCollectionCount = GC.CollectionCount(0);
				_graph.FillColumn(_graphStartX, 0, 5, _statsMonitor.colorGCBlip);
			}
			_graph.Scroll(-1, 0, _fpsColors[_statsMonitor.fpsLevel]);
			_graph.Apply();
		}
	}

	public override void Dispose()
	{
		if (_graph != null)
		{
			_graph.Dispose();
		}
		_graph = null;
		View2D.Destroy(_image);
		_image = null;
		base.Dispose();
	}

	internal void SetWidth(float width)
	{
		_width = (int)width;
	}

	protected override GameObject CreateChildren()
	{
		_fpsColors = new Color?[3];
		GameObject gameObject = new GameObject();
		gameObject.name = "GraphView";
		gameObject.transform.parent = _statsMonitor.transform;
		_graph = new Bitmap2D(10, 10, _statsMonitor.colorGraphBG);
		_image = gameObject.AddComponent<RawImage>();
		_image.rectTransform.sizeDelta = new Vector2(10f, 10f);
		_image.color = Color.white;
		_image.texture = _graph.texture;
		int systemMemorySize = SystemInfo.systemMemorySize;
		if (systemMemorySize <= 1024)
		{
			_memCeiling = 512;
		}
		else if (systemMemorySize > 1024 && systemMemorySize <= 2048)
		{
			_memCeiling = 1024;
		}
		else
		{
			_memCeiling = 2048;
		}
		return gameObject;
	}

	protected override void UpdateStyle()
	{
		if (_graph != null)
		{
			_graph.color = _statsMonitor.colorGraphBG;
		}
		if (_statsMonitor.colorOutline.a > 0f)
		{
			GraphicsFactory.AddOutlineAndShadow(_image.gameObject, _statsMonitor.colorOutline);
		}
		else
		{
			GraphicsFactory.RemoveEffects(_image.gameObject);
		}
		_fpsColors[0] = null;
		_fpsColors[1] = new Color(_statsMonitor.colorFPSWarning.r, _statsMonitor.colorFPSWarning.g, _statsMonitor.colorFPSWarning.b, _statsMonitor.colorFPSWarning.a / 4f);
		_fpsColors[2] = new Color(_statsMonitor.ColorFPSCritical.r, _statsMonitor.ColorFPSCritical.g, _statsMonitor.ColorFPSCritical.b, _statsMonitor.ColorFPSCritical.a / 4f);
	}

	protected override void UpdateLayout()
	{
		if (_width > 0 && _statsMonitor.graphHeight > 0 && (_statsMonitor.graphHeight != _height || _oldWidth != _width))
		{
			_oldWidth = _width;
			_height = _statsMonitor.graphHeight;
			_height = ((_height % 2 == 0) ? _height : (_height + 1));
			_graphStartX = _width - 1;
			_graphMaxY = _height - 1;
			_image.rectTransform.sizeDelta = new Vector2(_width, _height);
			_graph.Resize(_width, _height);
			_graph.Clear();
			SetRTransformValues(0f, 0f, _width, _height, Vector2.one);
		}
	}
}
