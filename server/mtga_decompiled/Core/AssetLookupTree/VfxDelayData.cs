using System;
using AssetLookupTree.Evaluators;

namespace AssetLookupTree;

[Serializable]
public class VfxDelayData
{
	public float Time;

	public IEvaluator Condition;
}
