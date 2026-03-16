using System.Collections.Generic;
using System.Text;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.CardParts.FieldFillers;

public static class ColorIndicatorUtil
{
	private const string INDICATOR_PREFIX = "ColorIndicators_";

	private const string SPRITESHEET_NAME = "SpriteSheet_ColorIndicators";

	private static StringBuilder _builder = new StringBuilder();

	private static IComparer<CardColor> _wubrgComparer = new WUBRGColorComparer();

	private static List<CardColor> _sortedColors = new List<CardColor>();

	private static Dictionary<int, string> _knownConversions = new Dictionary<int, string>();

	public static bool TryGetIndicator(ICardDataAdapter model, AssetLookupSystem assetLookupSystem, out ColorIndicator.DisplayType indicatorType)
	{
		indicatorType = ColorIndicator.DisplayType.None;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(model);
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ColorIndicator> loadedTree))
		{
			ColorIndicator payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				indicatorType = payload.Type;
			}
		}
		return indicatorType != ColorIndicator.DisplayType.None;
	}

	public static string GetIndicatorSprite(ColorIndicator.DisplayType type, ICardDataAdapter model)
	{
		IReadOnlyList<CardColor> indicatorColors = GetIndicatorColors(type, model);
		int hashCode = indicatorColors.GetHashCode();
		if (_knownConversions.TryGetValue(hashCode, out var value))
		{
			return value;
		}
		string arg = ColorsToSpriteName(indicatorColors);
		string text = string.Format("<sprite=\"{0}\" name=\"{1}\">", "SpriteSheet_ColorIndicators", arg);
		_knownConversions.Add(hashCode, text);
		return text;
	}

	private static IReadOnlyList<CardColor> GetIndicatorColors(ColorIndicator.DisplayType type, ICardDataAdapter model)
	{
		if (type == ColorIndicator.DisplayType.PrintedIndicatorColors || type != ColorIndicator.DisplayType.CurrentColor)
		{
			return model.Printing.IndicatorColors;
		}
		return model.Colors;
	}

	private static string ColorsToSpriteName(IReadOnlyList<CardColor> colors)
	{
		_sortedColors.Clear();
		_builder.Clear();
		_builder.Append("ColorIndicators_");
		_sortedColors.AddRange(colors);
		_sortedColors.Sort(_wubrgComparer);
		foreach (CardColor sortedColor in _sortedColors)
		{
			_builder.Append(CardColorToStringCode(sortedColor));
		}
		return _builder.ToString();
	}

	private static string CardColorToStringCode(CardColor color)
	{
		return color switch
		{
			CardColor.White => "W", 
			CardColor.Blue => "U", 
			CardColor.Black => "B", 
			CardColor.Red => "R", 
			CardColor.Green => "G", 
			CardColor.Colorless => "C", 
			_ => string.Empty, 
		};
	}
}
