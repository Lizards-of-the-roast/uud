using System;
using UnityEngine;

namespace Wotc.Mtga.Cards.Parts.Textbox;

[CreateAssetMenu(fileName = "StationAbilityColorTable", menuName = "ScriptableObject/StationAbilityColorTable", order = -1)]
public class StationAbilityColorTable : ScriptableObject
{
	[Serializable]
	private class StationColors
	{
		[SerializeField]
		private Color _first;

		[SerializeField]
		private Color _second;

		public Color GetColor(bool firstAbility)
		{
			if (!firstAbility)
			{
				return _second;
			}
			return _first;
		}
	}

	[SerializeField]
	private StationColors _colorless;

	[SerializeField]
	private StationColors _white;

	[SerializeField]
	private StationColors _blue;

	[SerializeField]
	private StationColors _black;

	[SerializeField]
	private StationColors _red;

	[SerializeField]
	private StationColors _green;

	[SerializeField]
	private StationColors _gold;

	public Color GetColor(CardFrameKey color, bool firstAbility)
	{
		return GetStationColor(color).GetColor(firstAbility);
	}

	private StationColors GetStationColor(CardFrameKey color)
	{
		return color switch
		{
			CardFrameKey.Colorless => _colorless, 
			CardFrameKey.White => _white, 
			CardFrameKey.Blue => _blue, 
			CardFrameKey.Black => _black, 
			CardFrameKey.Red => _red, 
			CardFrameKey.Green => _green, 
			CardFrameKey.Gold => _gold, 
			_ => _colorless, 
		};
	}
}
