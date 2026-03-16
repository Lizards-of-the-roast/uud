using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Hangers;

namespace AssetLookupTree.Payloads.Ability.Metadata;

public class ParameterizedHangerEntryData
{
	public string Title;

	public string Term;

	public ParameterizedInjectors Injectors;

	public Dictionary<string, string> InjectorData = new Dictionary<string, string>();

	public int keySelectedIndex;

	public string[] keyOptions = new string[1] { "Mana Symbol" };

	public int manaSymbolSelectedIndex;

	public string[] manaSymbolStringOptions = new string[6] { "oW", "oU", "oB", "oR", "oG", "oC" };

	public readonly AltAssetReference<Sprite> SpriteRef = new AltAssetReference<Sprite>();
}
