using System.Collections.Generic;

namespace AssetLookupTree.Evaluators.Utility;

public abstract class Utility_Compound : EvaluatorBase_Boolean
{
	public readonly List<IEvaluator> NestedEvaluators = new List<IEvaluator>(2);
}
