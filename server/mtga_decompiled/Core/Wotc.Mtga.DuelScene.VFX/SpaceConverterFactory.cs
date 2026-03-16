using System.Collections.Generic;
using AssetLookupTree;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public static class SpaceConverterFactory
{
	public static IReadOnlyDictionary<RelativeSpace, ISpaceConverter> Create(IGameStateProvider gameStateProvider, ICardHolderProvider cardHolderProvider, IEntityViewProvider entityViewProvider, Transform battlefieldTransform)
	{
		SortedDictionary<RelativeSpace, ISpaceConverter> sortedDictionary = new SortedDictionary<RelativeSpace, ISpaceConverter>();
		sortedDictionary[RelativeSpace.World] = new WorldSpaceConverter(battlefieldTransform);
		sortedDictionary[RelativeSpace.Local] = new SourceSpaceConverter();
		sortedDictionary[RelativeSpace.Stack] = new HolderSpaceConverterStatic(cardHolderProvider, GREPlayerNum.Invalid, CardHolderType.Stack);
		sortedDictionary[RelativeSpace.Source] = new SourceSpaceConverter();
		sortedDictionary[RelativeSpace.Source_Parent] = new ParentSpaceConverterSource(entityViewProvider);
		sortedDictionary[RelativeSpace.Source_Controller_Exile] = new HolderSpaceConverterRelative(cardHolderProvider, HolderSpaceConverterRelative.RelativeContext.Local, CardHolderType.Exile);
		sortedDictionary[RelativeSpace.Source_Controller_Graveyard] = new HolderSpaceConverterRelative(cardHolderProvider, HolderSpaceConverterRelative.RelativeContext.Local, CardHolderType.Graveyard);
		sortedDictionary[RelativeSpace.Source_Controller_Hand] = new HolderSpaceConverterRelative(cardHolderProvider, HolderSpaceConverterRelative.RelativeContext.Local, CardHolderType.Hand);
		sortedDictionary[RelativeSpace.Source_Controller_Library] = new HolderSpaceConverterRelative(cardHolderProvider, HolderSpaceConverterRelative.RelativeContext.Local, CardHolderType.Library);
		sortedDictionary[RelativeSpace.Source_Controller_AllCreatures] = new AllCreaturesConverterRelative(entityViewProvider, gameStateProvider, AllCreaturesConverterRelative.RelativeContext.Local);
		sortedDictionary[RelativeSpace.Source_Controller_CreatureRow] = new CreatureRowConverterRelative(cardHolderProvider, CreatureRowConverterRelative.RelativeContext.Local);
		sortedDictionary[RelativeSpace.Source_Controller_Avatar] = new AvatarSpaceConverterRelative(entityViewProvider, AvatarSpaceConverterRelative.RelativeContext.Local);
		sortedDictionary[RelativeSpace.Source_Opponent_Exile] = new HolderSpaceConverterRelative(cardHolderProvider, HolderSpaceConverterRelative.RelativeContext.Opponent, CardHolderType.Exile);
		sortedDictionary[RelativeSpace.Source_Opponent_Graveyard] = new HolderSpaceConverterRelative(cardHolderProvider, HolderSpaceConverterRelative.RelativeContext.Opponent, CardHolderType.Graveyard);
		sortedDictionary[RelativeSpace.Source_Opponent_Hand] = new HolderSpaceConverterRelative(cardHolderProvider, HolderSpaceConverterRelative.RelativeContext.Opponent, CardHolderType.Hand);
		sortedDictionary[RelativeSpace.Source_Opponent_Library] = new HolderSpaceConverterRelative(cardHolderProvider, HolderSpaceConverterRelative.RelativeContext.Opponent, CardHolderType.Library);
		sortedDictionary[RelativeSpace.Source_Opponent_AllCreatures] = new AllCreaturesConverterRelative(entityViewProvider, gameStateProvider, AllCreaturesConverterRelative.RelativeContext.Opponent);
		sortedDictionary[RelativeSpace.Source_Opponent_CreatureRow] = new CreatureRowConverterRelative(cardHolderProvider, CreatureRowConverterRelative.RelativeContext.Opponent);
		sortedDictionary[RelativeSpace.Source_Opponent_Avatar] = new AvatarSpaceConverterRelative(entityViewProvider, AvatarSpaceConverterRelative.RelativeContext.Opponent);
		sortedDictionary[RelativeSpace.Target] = new TargetSpaceConverter(entityViewProvider);
		sortedDictionary[RelativeSpace.Target_CardsOnly] = new TargetCardsSpaceConverter(entityViewProvider);
		sortedDictionary[RelativeSpace.Target_Parent] = new ParentSpaceConverterTarget(entityViewProvider);
		sortedDictionary[RelativeSpace.Target_Controller_Exile] = new HolderSpaceConverterTarget(cardHolderProvider, CardHolderType.Exile);
		sortedDictionary[RelativeSpace.Target_Controller_Graveyard] = new HolderSpaceConverterTarget(cardHolderProvider, CardHolderType.Graveyard);
		sortedDictionary[RelativeSpace.Target_Controller_Hand] = new HolderSpaceConverterTarget(cardHolderProvider, CardHolderType.Hand);
		sortedDictionary[RelativeSpace.Target_Controller_Library] = new HolderSpaceConverterTarget(cardHolderProvider, CardHolderType.Library);
		sortedDictionary[RelativeSpace.Target_Controller_AllCreatures] = new AllCreaturesConverterTarget(entityViewProvider, gameStateProvider);
		sortedDictionary[RelativeSpace.Target_Controller_CreatureRow] = new CreatureRowConverterTarget(cardHolderProvider);
		sortedDictionary[RelativeSpace.Target_Controller_Avatar] = new AvatarSpaceConverterTarget(entityViewProvider);
		sortedDictionary[RelativeSpace.Target_AttachedTo] = new AttachedSpaceConverterTarget(entityViewProvider);
		sortedDictionary[RelativeSpace.LocalPlayer] = new AvatarSpaceConverterStatic(entityViewProvider, GREPlayerNum.LocalPlayer);
		sortedDictionary[RelativeSpace.LocalPlayer_Exile] = new HolderSpaceConverterStatic(cardHolderProvider, GREPlayerNum.LocalPlayer, CardHolderType.Exile);
		sortedDictionary[RelativeSpace.LocalPlayer_Graveyard] = new HolderSpaceConverterStatic(cardHolderProvider, GREPlayerNum.LocalPlayer, CardHolderType.Graveyard);
		sortedDictionary[RelativeSpace.LocalPlayer_Hand] = new HolderSpaceConverterStatic(cardHolderProvider, GREPlayerNum.LocalPlayer, CardHolderType.Hand);
		sortedDictionary[RelativeSpace.LocalPlayer_Library] = new HolderSpaceConverterStatic(cardHolderProvider, GREPlayerNum.LocalPlayer, CardHolderType.Library);
		sortedDictionary[RelativeSpace.LocalPlayer] = new AvatarSpaceConverterStatic(entityViewProvider, GREPlayerNum.LocalPlayer);
		sortedDictionary[RelativeSpace.LocalPlayer_CreatureRow] = new CreatureRowConverterStatic(cardHolderProvider, GREPlayerNum.LocalPlayer);
		sortedDictionary[RelativeSpace.LocalPlayer_AllCreatures] = new AllCreaturesConverterStatic(entityViewProvider, gameStateProvider, GREPlayerNum.LocalPlayer);
		sortedDictionary[RelativeSpace.Opponent] = new AvatarSpaceConverterStatic(entityViewProvider, GREPlayerNum.Opponent);
		sortedDictionary[RelativeSpace.Opponent_Exile] = new HolderSpaceConverterStatic(cardHolderProvider, GREPlayerNum.Opponent, CardHolderType.Exile);
		sortedDictionary[RelativeSpace.Opponent_Graveyard] = new HolderSpaceConverterStatic(cardHolderProvider, GREPlayerNum.Opponent, CardHolderType.Graveyard);
		sortedDictionary[RelativeSpace.Opponent_Hand] = new HolderSpaceConverterStatic(cardHolderProvider, GREPlayerNum.Opponent, CardHolderType.Hand);
		sortedDictionary[RelativeSpace.Opponent_Library] = new HolderSpaceConverterStatic(cardHolderProvider, GREPlayerNum.Opponent, CardHolderType.Library);
		sortedDictionary[RelativeSpace.Opponent_AllCreatures] = new AllCreaturesConverterStatic(entityViewProvider, gameStateProvider, GREPlayerNum.Opponent);
		sortedDictionary[RelativeSpace.Opponent_CreatureRow] = new CreatureRowConverterStatic(cardHolderProvider, GREPlayerNum.Opponent);
		sortedDictionary[RelativeSpace.Opponent] = new AvatarSpaceConverterStatic(entityViewProvider, GREPlayerNum.Opponent);
		return sortedDictionary;
	}
}
