using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardTextColorTable", menuName = "ScriptableObject/Card Text Color Table", order = 0)]
public class CardTextColorTable : ScriptableObject
{
	public enum ColorSchemes
	{
		LightBackgrounds = 0,
		DarkBackgrounds = 1,
		PowerToughnessLightBgs = 2,
		PowerToughnessDarkBgs = 3,
		ArtistCredit = 4,
		Custom = 999
	}

	[Serializable]
	public class FieldTypeOverride
	{
		public CDCFieldFillerFieldType FieldType;

		public ColorSchemes ColorScheme;

		public CardTextColorSettings Settings;
	}

	public ColorSchemes ColorScheme;

	public CardTextColorSettings DefaultSettings = new CardTextColorSettings();

	public List<FieldTypeOverride> FieldTypeOverrides = new List<FieldTypeOverride>();
}
