using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtgo.Gre.External.Messaging;

[CreateAssetMenu(fileName = "CardColorTable", menuName = "ScriptableObject/Card Color Table", order = -1)]
public class CardColorTable : ScriptableObject
{
	[SerializeField]
	private UnityEngine.Color _colorless;

	[Space(10f)]
	[SerializeField]
	private UnityEngine.Color _white;

	[SerializeField]
	private UnityEngine.Color _blue;

	[SerializeField]
	private UnityEngine.Color _black;

	[SerializeField]
	private UnityEngine.Color _red;

	[SerializeField]
	private UnityEngine.Color _green;

	[Space(10f)]
	[SerializeField]
	private UnityEngine.Color _gold;

	[Space(10f)]
	[SerializeField]
	private bool _goldForTwoColor;

	public UnityEngine.Color GetColor(CardColor key)
	{
		return key switch
		{
			CardColor.White => _white, 
			CardColor.Blue => _blue, 
			CardColor.Black => _black, 
			CardColor.Red => _red, 
			CardColor.Green => _green, 
			_ => _colorless, 
		};
	}

	public UnityEngine.Color GetColor(IReadOnlyList<CardColor> key)
	{
		if (key == null || key.Count == 0)
		{
			return _colorless;
		}
		if (key.Count == 1)
		{
			return GetColor(key[0]);
		}
		return _gold;
	}

	public KeyValuePair<UnityEngine.Color, UnityEngine.Color> GetColors(IReadOnlyList<CardColor> key)
	{
		KeyValuePair<UnityEngine.Color, UnityEngine.Color> result;
		if (key == null || key.Count == 0)
		{
			result = new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(_colorless, _colorless);
		}
		else if (key.Count != 1)
		{
			result = ((key.Count != 2 || _goldForTwoColor) ? new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(_gold, _gold) : new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(GetColor(key[0]), GetColor(key[1])));
		}
		else
		{
			UnityEngine.Color color = GetColor(key[0]);
			result = new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(color, color);
		}
		return result;
	}

	public UnityEngine.Color GetColor(CardFrameKey key)
	{
		CardColor key2 = CardColor.Colorless;
		switch (key)
		{
		case CardFrameKey.White:
			key2 = CardColor.White;
			break;
		case CardFrameKey.Blue:
			key2 = CardColor.Blue;
			break;
		case CardFrameKey.Black:
			key2 = CardColor.Black;
			break;
		case CardFrameKey.Red:
			key2 = CardColor.Red;
			break;
		case CardFrameKey.Green:
			key2 = CardColor.Green;
			break;
		}
		return GetColor(key2);
	}

	public KeyValuePair<UnityEngine.Color, UnityEngine.Color> GetColors(IReadOnlyList<CardFrameKey> key)
	{
		KeyValuePair<UnityEngine.Color, UnityEngine.Color> result;
		if (key == null || key.Count == 0)
		{
			result = new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(_colorless, _colorless);
		}
		else if (key.Count != 1)
		{
			result = ((key.Count != 2 || _goldForTwoColor) ? new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(_gold, _gold) : new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(GetColor(key[0]), GetColor(key[1])));
		}
		else
		{
			UnityEngine.Color color = GetColor(key[0]);
			result = new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(color, color);
		}
		return result;
	}

	public UnityEngine.Color GetColor(ManaColor key)
	{
		CardColor key2 = CardColor.Colorless;
		switch (key)
		{
		case ManaColor.White:
			key2 = CardColor.White;
			break;
		case ManaColor.Blue:
			key2 = CardColor.Blue;
			break;
		case ManaColor.Black:
			key2 = CardColor.Black;
			break;
		case ManaColor.Red:
			key2 = CardColor.Red;
			break;
		case ManaColor.Green:
			key2 = CardColor.Green;
			break;
		}
		return GetColor(key2);
	}

	public KeyValuePair<UnityEngine.Color, UnityEngine.Color> GetColors(IReadOnlyList<ManaColor> key)
	{
		KeyValuePair<UnityEngine.Color, UnityEngine.Color> result;
		if (key == null || key.Count == 0)
		{
			result = new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(_colorless, _colorless);
		}
		else if (key.Count != 1)
		{
			result = ((key.Count != 2 || _goldForTwoColor) ? new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(_gold, _gold) : new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(GetColor(key[0]), GetColor(key[1])));
		}
		else
		{
			UnityEngine.Color color = GetColor(key[0]);
			result = new KeyValuePair<UnityEngine.Color, UnityEngine.Color>(color, color);
		}
		return result;
	}
}
