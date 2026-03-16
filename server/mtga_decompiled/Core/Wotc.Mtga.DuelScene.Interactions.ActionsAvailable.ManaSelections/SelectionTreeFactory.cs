using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public static class SelectionTreeFactory
{
	public static IManaSelectionTree GetTree(ManaSourceCollection collection)
	{
		if (collection.BranchingCollections.Count > 0)
		{
			return new BranchingManaSelectionTree(getAllTrees(collection));
		}
		return GetTreeForSelectionType(collection);
		static IEnumerable<IManaSelectionTree> getAllTrees(ManaSourceCollection sourceCollection)
		{
			yield return GetTreeForSelectionType(sourceCollection);
			foreach (ManaSourceCollection value in sourceCollection.BranchingCollections.Values)
			{
				yield return GetTreeForSelectionType(value);
			}
		}
	}

	private static IManaSelectionTree GetTreeForSelectionType(ManaSourceCollection collection)
	{
		return collection.SelectionType switch
		{
			SelectionType.AnyColor => new ManaSelectionTree<AnyColorSelection>(collection), 
			SelectionType.SpecificColorCombination => new ManaSelectionTree<ColorCombinationSelection>(collection), 
			SelectionType.Standard => new ManaSelectionTree<StandardSelection>(collection), 
			_ => throw new ArgumentException($"No tree defined for {collection.SelectionType}, did you forget to add a case here?"), 
		};
	}
}
