using System.Collections.Generic;
using UnityEngine;

public class CardLayout_General : ICardLayout
{
	public enum SplayDirection
	{
		None,
		Up,
		Down,
		Left,
		Right
	}

	private float _minDegreesRotationOffset = -15f;

	private float _maxDegreesRotationOffset = 15f;

	private float _minZTranslationOffset;

	private float _maxZTranslationOffset = 0.05f;

	private float _minXTranslationOffset = -0.1f;

	private float _maxXTranslationOffset = 0.1f;

	private float _cardThickness = 0.04f;

	private float _strictSplayOffset = 0.2f;

	private SplayDirection _splayDirection = SplayDirection.Down;

	private bool _isReversedDisplay;

	private readonly List<Vector3> _cardRotations = new List<Vector3>();

	private readonly Dictionary<int, List<CardLayoutData>> _preCalculatedLayouts = new Dictionary<int, List<CardLayoutData>>();

	public float MinDegreesRotationOffset
	{
		get
		{
			return _minDegreesRotationOffset;
		}
		set
		{
			_minDegreesRotationOffset = value;
			_preCalculatedLayouts.Clear();
		}
	}

	public float MaxDegreesRotationOffset
	{
		get
		{
			return _maxDegreesRotationOffset;
		}
		set
		{
			_maxDegreesRotationOffset = value;
			_preCalculatedLayouts.Clear();
		}
	}

	public float MinZTranslationOffset
	{
		get
		{
			return _minZTranslationOffset;
		}
		set
		{
			_minZTranslationOffset = value;
			_preCalculatedLayouts.Clear();
		}
	}

	public float MaxZTranslationOffset
	{
		get
		{
			return _maxZTranslationOffset;
		}
		set
		{
			_maxZTranslationOffset = value;
			_preCalculatedLayouts.Clear();
		}
	}

	public float MinXTranslationOffset
	{
		get
		{
			return _minXTranslationOffset;
		}
		set
		{
			_minXTranslationOffset = value;
			_preCalculatedLayouts.Clear();
		}
	}

	public float MaxXTranslationOffset
	{
		get
		{
			return _maxXTranslationOffset;
		}
		set
		{
			_maxXTranslationOffset = value;
			_preCalculatedLayouts.Clear();
		}
	}

	public float CardThickness
	{
		get
		{
			return _cardThickness;
		}
		set
		{
			_cardThickness = value;
			_preCalculatedLayouts.Clear();
		}
	}

	public float StrictSplayOffset
	{
		get
		{
			return _strictSplayOffset;
		}
		set
		{
			_strictSplayOffset = value;
			_preCalculatedLayouts.Clear();
		}
	}

	public SplayDirection Direction
	{
		get
		{
			return _splayDirection;
		}
		set
		{
			_splayDirection = value;
			_preCalculatedLayouts.Clear();
		}
	}

	public bool IsReversedDisplay
	{
		get
		{
			return _isReversedDisplay;
		}
		set
		{
			_isReversedDisplay = value;
			_preCalculatedLayouts.Clear();
		}
	}

	private Vector3 GetSplayVector()
	{
		return _splayDirection switch
		{
			SplayDirection.Up => Vector3.up, 
			SplayDirection.Down => Vector3.down, 
			SplayDirection.Left => Vector3.left, 
			SplayDirection.Right => Vector3.right, 
			_ => Vector3.zero, 
		};
	}

	private Vector3 GetSplayPerpendicularVector()
	{
		return _splayDirection switch
		{
			SplayDirection.Up => Vector3.left, 
			SplayDirection.Down => Vector3.right, 
			SplayDirection.Left => Vector3.down, 
			SplayDirection.Right => Vector3.up, 
			_ => Vector3.zero, 
		};
	}

	public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		int count = allCardViews.Count;
		if (_preCalculatedLayouts.TryGetValue(count, out var value) && value != null && value.Count == count)
		{
			for (int i = 0; i < value.Count; i++)
			{
				int index = (IsReversedDisplay ? (value.Count - 1 - i) : i);
				DuelScene_CDC card = allCardViews[index];
				CardLayoutData cardLayoutData = value[i];
				allData.Add(new CardLayoutData(card, cardLayoutData.Position, cardLayoutData.Rotation, cardLayoutData.Scale));
			}
			return;
		}
		value = (_preCalculatedLayouts[count] = new List<CardLayoutData>(count));
		Vector3 splayVector = GetSplayVector();
		Vector3 splayPerpendicularVector = GetSplayPerpendicularVector();
		Vector3 vector = center;
		while (count >= _cardRotations.Count)
		{
			_cardRotations.Add(Vector3.forward * Random.Range(_minDegreesRotationOffset, _maxDegreesRotationOffset));
		}
		for (int j = 0; j < count; j++)
		{
			int index2 = (IsReversedDisplay ? (count - 1 - j) : j);
			DuelScene_CDC card2 = allCardViews[index2];
			Vector3 pos = vector + splayVector * Random.Range(_minZTranslationOffset, _maxZTranslationOffset) + splayPerpendicularVector * Random.Range(_minXTranslationOffset, _maxXTranslationOffset);
			Vector3 euler = _cardRotations[j];
			allData.Add(new CardLayoutData(card2, pos, Quaternion.Euler(euler)));
			vector += splayVector * _strictSplayOffset;
			vector += Vector3.back * _cardThickness;
		}
		value.AddRange(allData);
	}
}
