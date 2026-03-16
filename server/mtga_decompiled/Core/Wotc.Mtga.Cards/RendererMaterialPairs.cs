using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wotc.Mtga.Cards;

[Serializable]
public struct RendererMaterialPairs
{
	public Renderer Renderer;

	public List<int> MaterialIndexes;

	public List<MaterialOverrideType> MaterialOverrideTypes;
}
