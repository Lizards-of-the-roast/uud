using System.Collections.Generic;
using System.Linq;
using GreClient.Rules;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Duel;
using Wotc.Mtga.Wrapper;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Blackboard;

public static class BlackboardUtils
{
	public static JObject CreateBlackboardSnapshot(IBlackboard bb)
	{
		JObject jObject = new JObject(new JProperty("ContentVersion", bb.ContentVersion));
		if (bb.InWrapper)
		{
			jObject.Add("InWrapper", bb.InWrapper);
		}
		if (bb.InHorizontalDeckBuilder)
		{
			jObject.Add("InHorizontalDeckBuilder", bb.InHorizontalDeckBuilder);
		}
		if (bb.InDuelScene)
		{
			jObject.Add("InDuelScene", bb.InDuelScene);
		}
		if ((bool)bb.Material)
		{
			object[] obj = new object[3]
			{
				new JProperty("name", bb.Material.name),
				new JProperty("shader", bb.Material.shader.name),
				null
			};
			object[] shaderKeywords = bb.Material.shaderKeywords;
			obj[2] = new JProperty("shaderKeywords", new JArray(shaderKeywords));
			jObject.Add("Material", new JObject(obj));
		}
		if (!string.IsNullOrWhiteSpace(bb.MaterialName))
		{
			jObject.Add("MaterialName", bb.MaterialName);
		}
		if ((bool)bb.Texture)
		{
			jObject.Add("Texture", new JObject(new JProperty("name", bb.Texture.name), new JProperty("format", bb.Texture.format.ToString())));
		}
		if (!string.IsNullOrWhiteSpace(bb.TextureName))
		{
			jObject.Add("TextureName", bb.TextureName);
		}
		if (bb.CardIsHovered)
		{
			jObject.Add("CardIsHovered", bb.CardIsHovered);
		}
		if (bb.CardData != null)
		{
			JObject jObject2 = new JObject(new JProperty("InstanceId", bb.CardData.InstanceId), new JProperty("TitleId", bb.CardData.TitleId), new JProperty("OwnerNum", bb.CardData.OwnerNum.ToString()), new JProperty("ControllerNum", bb.CardData.ControllerNum.ToString()), new JProperty("ZoneType", bb.CardData.ZoneType.ToString()), new JProperty("Supertypes", bb.CardData.Supertypes), new JProperty("CardTypes", bb.CardData.CardTypes), new JProperty("Subtypes", bb.CardData.Subtypes), new JProperty("Abilities", bb.CardData.AbilityIds), new JProperty("Colors", bb.CardData.Colors), new JProperty("GetFrameColors", bb.CardData.GetFrameColors), new JProperty("PresentationColor", bb.CardData.PresentationColor.ToString()));
			if (bb.CardDatabase != null)
			{
				jObject2.Add("Title", bb.CardDatabase.GreLocProvider.GetLocalizedText(bb.CardData.TitleId));
			}
			jObject.Add("CardData", jObject2);
		}
		if (bb.CardHolder != null)
		{
			jObject.Add("CardHolder", new JObject(new JProperty("CardHolderType", bb.CardHolder.CardHolderType.ToString()), new JProperty("PlayerNum", bb.CardHolder.PlayerNum.ToString()), new JProperty("CardViews", bb.CardHolder.CardViews.Count)));
		}
		if (bb.CardHolderType != CardHolderType.Invalid)
		{
			jObject.Add("CardHolderType", bb.CardHolderType.ToString());
		}
		if (bb.CounterType != CounterType.None)
		{
			jObject.Add("CounterType", bb.CounterType.ToString());
		}
		if (bb.FieldFillerType != CDCFieldFillerFieldType.None)
		{
			jObject.Add("FieldFillerType", bb.FieldFillerType.ToString());
		}
		if (bb.SpriteFillerType != CDCSpriteFiller.FieldType.None)
		{
			jObject.Add("SpriteFillerType", bb.SpriteFillerType.ToString());
		}
		if (bb.SpriteSdfFillerType != CDCSpriteFillerSDF.FieldType.None)
		{
			jObject.Add("SpriteSdfFillerType", bb.SpriteSdfFillerType.ToString());
		}
		if (bb.Ability != null)
		{
			JObject jObject3 = new JObject(new JProperty("Id", bb.Ability.Id), new JProperty("BaseId", bb.Ability.BaseId), new JProperty("Category", bb.Ability.Category.ToString()), new JProperty("SubCategory", bb.Ability.SubCategory.ToString()), new JProperty("ReferencedAbilityTypes", new JArray(bb.Ability.ReferencedAbilityTypes.Select((AbilityType x) => x.ToString()))));
			if (bb.CardDatabase != null)
			{
				jObject3.Add("Text", bb.CardDatabase.GreLocProvider.GetLocalizedText(bb.Ability.TextId));
			}
			jObject.Add("Ability", jObject3);
		}
		if (bb.BadgeData != null)
		{
			jObject.Add("BadgeData", new JObject(new JProperty("Category", bb.BadgeData.Category.ToString()), new JProperty("GetActivationWord", bb.BadgeData.GetActivationWord()), new JProperty("ActivationCalculator", bb.BadgeData.ActivationCalculator.GetType().Name), new JProperty("NumberCalculator", bb.BadgeData.NumberCalculator.GetType().Name)));
		}
		if (bb.PlayerInfoMatch != null)
		{
			jObject.Add("PlayerInfoMatch", new JObject(new JProperty("AvatarSelection", bb.PlayerInfoMatch.AvatarSelection), new JProperty("SleeveSelection", bb.PlayerInfoMatch.SleeveSelection), new JProperty("TitleSelection", bb.PlayerInfoMatch.TitleSelection), new JProperty("EmoteSelection", new JArray(bb.PlayerInfoMatch.EmoteSelection)), new JProperty("PetSelection", new JObject(new JProperty("name", bb.PlayerInfoMatch.PetSelection?.name), new JProperty("variant", bb.PlayerInfoMatch.PetSelection?.variant)))));
		}
		if (bb.PlayerInfoGame != null)
		{
			jObject.Add("PlayerInfoGame", new JObject(new JProperty("LifeTotal", bb.PlayerInfoGame.LifeTotal), new JProperty("MaxHandSize", bb.PlayerInfoGame.MaxHandSize)));
		}
		if (bb.Player != null)
		{
			jObject.Add("Player", new JObject(new JProperty("ClientPlayerEnum", bb.Player.ClientPlayerEnum.ToString()), new JProperty("LifeTotal", bb.Player.LifeTotal), new JProperty("CommanderIds", bb.Player.CommanderIds), new JProperty("CompanionId", bb.Player.CompanionId)));
		}
		if (bb.Request != null)
		{
			jObject.Add("Request", new JObject(new JProperty("Type", bb.Request.Type.ToString()), new JProperty("SourceId", bb.Request.SourceId), new JProperty("Prompt", bb.Request.Prompt?.PromptId)));
		}
		if (bb.Interaction != null)
		{
			jObject.Add("Interaction", new JObject(new JProperty("Type", bb.Interaction.Type.ToString()), new JProperty("SourceId", bb.Interaction.SourceId)));
		}
		if (bb.IsHoverCopy)
		{
			jObject.Add("IsHoverCopy", bb.IsHoverCopy);
		}
		if (bb.IsExaminedCard)
		{
			jObject.Add("IsExaminedCard", bb.IsExaminedCard);
		}
		if (bb.MouseOverType != MouseOverType.None)
		{
			jObject.Add("MouseOverType", bb.MouseOverType.ToString());
		}
		if (bb.ActiveResolution != null)
		{
			JObject jObject4 = new JObject();
			jObject4.Add("InstanceId", bb.ActiveResolution.InstanceId);
			jObject.Add("ActiveResolution", jObject4);
		}
		if (!string.IsNullOrWhiteSpace(bb.Language))
		{
			jObject.Add("Language", bb.Language);
		}
		_ = bb.DateTimeUtc;
		jObject.Add("DateTimeUtc", bb.DateTimeUtc);
		if (bb.HighlightType != HighlightType.None)
		{
			jObject.Add("HighlightType", bb.HighlightType.ToString());
		}
		if (bb.NavContentType != NavContentType.None)
		{
			jObject.Add("NavContentType", bb.NavContentType.ToString());
		}
		if (bb.DeviceType != DeviceType.Unknown)
		{
			jObject.Add("DeviceType", bb.DeviceType.ToString());
		}
		if (bb.AspectRatio != 0f)
		{
			jObject.Add("AspectRatio", bb.AspectRatio);
		}
		if (!string.IsNullOrWhiteSpace(bb.BattlefieldId))
		{
			jObject.Add("BattlefieldId", bb.BattlefieldId);
		}
		if (bb.DamageAmount.HasValue)
		{
			jObject.Add("DamageAmount", bb.DamageAmount.Value);
		}
		if (bb.DamageType != DamageType.None)
		{
			jObject.Add("DamageType", bb.DamageType.ToString());
		}
		if (bb.CardReactionType != CardReactionEnum.None)
		{
			jObject.Add("CardReactionType", bb.CardReactionType.ToString());
		}
		HashSet<DecoratorType> decoratorTypes = bb.DecoratorTypes;
		if (decoratorTypes != null && decoratorTypes.Count > 0)
		{
			jObject.Add("DecoratorTypes", new JArray(bb.DecoratorTypes));
		}
		if (bb.CardBrowserType != DuelSceneBrowserType.Invalid)
		{
			jObject.Add("CardBrowserType", bb.CardBrowserType.ToString());
		}
		if (!string.IsNullOrWhiteSpace(bb.CardBrowserElementID))
		{
			jObject.Add("CardBrowserElementID", bb.CardBrowserElementID);
		}
		if (!string.IsNullOrWhiteSpace(bb.CardBrowserLayoutID))
		{
			jObject.Add("CardBrowserLayoutID", bb.CardBrowserLayoutID);
		}
		if (bb.CardBrowserCardCount.HasValue)
		{
			jObject.Add("CardBrowserCardCount", bb.CardBrowserCardCount.Value);
		}
		if (bb.SelectCardBrowserMinMax.HasValue)
		{
			jObject.Add("SelectCardBrowserMinMax", new JObject(new JProperty("min", bb.SelectCardBrowserMinMax.Value.min), new JProperty("max", bb.SelectCardBrowserMinMax.Value.max)));
		}
		if (bb.GreActionType != ActionType.None)
		{
			jObject.Add("GreActionType", bb.GreActionType.ToString());
		}
		if (bb.LoyaltyValence != LoyaltyValence.Invalid)
		{
			jObject.Add("LoyaltyValence", bb.LoyaltyValence.ToString());
		}
		if (!string.IsNullOrWhiteSpace(bb.LocalNotificationUID))
		{
			jObject.Add("LocalNotificationUID", bb.LocalNotificationUID);
		}
		if (bb.LifeChange != 0)
		{
			jObject.Add("LifeChange", bb.LifeChange);
		}
		if (!bb.ZonePair.Equals(default(ZonePair)))
		{
			jObject.Add("ZonePair", new JObject(new JProperty("FromZone", bb.ZonePair.FromZone.ToString()), new JProperty("FromHolder", bb.ZonePair.FromHolder.ToString()), new JProperty("FromOwner", bb.ZonePair.FromOwner.ToString()), new JProperty("ToZone", bb.ZonePair.ToZone.ToString()), new JProperty("ToHolder", bb.ZonePair.ToHolder.ToString()), new JProperty("ToOwner", bb.ZonePair.ToOwner.ToString())));
		}
		if (!bb.RegionPair.Equals(default(RegionPair)))
		{
			jObject.Add("RegionPair", new JObject(new JProperty("FromRegion", bb.RegionPair.FromRegion.ToString()), new JProperty("FromOwner", bb.RegionPair.FromOwner.ToString()), new JProperty("ToRegion", bb.RegionPair.ToRegion.ToString()), new JProperty("ToOwner", bb.RegionPair.ToOwner.ToString())));
		}
		if (bb.BoosterCollationMapping != CollationMapping.None)
		{
			jObject.Add("BoosterCollationMapping", bb.BoosterCollationMapping.ToString());
		}
		if (bb.Event != null)
		{
			JObject jObject5 = new JObject();
			jObject5.Add("DeckSelectContext", bb.Event.DeckSelectContext.ToString());
			if (bb.Event.PlayerEvent != null)
			{
				jObject5.Add("PlayerEvent", new JObject(new JProperty("Format", bb.Event.PlayerEvent.Format?.FormatName ?? ""), new JProperty("CurrentWins", bb.Event.PlayerEvent.CurrentWins), new JProperty("CurrentLosses", bb.Event.PlayerEvent.CurrentLosses), new JProperty("GamesPlayed", bb.Event.PlayerEvent.GamesPlayed), new JProperty("MaxWins", bb.Event.PlayerEvent.MaxWins), new JProperty("MaxLosses", bb.Event.PlayerEvent.MaxLosses)));
			}
			if (bb.Event.PostMatchContext != null)
			{
				jObject5.Add("PostMatchContext", new JObject(new JProperty("GamesWon", bb.Event.PostMatchContext.GamesWon)));
			}
			jObject.Add("Event", jObject5);
		}
		if (bb.ConstructedRank.rank != RankingClassType.None)
		{
			jObject.Add("ConstructedRank", new JObject(new JProperty("rank", bb.ConstructedRank.rank), new JProperty("tier", bb.ConstructedRank.tier)));
		}
		if (bb.LimitedRank.rank != RankingClassType.None)
		{
			jObject.Add("LimitedRank", new JObject(new JProperty("rank", bb.LimitedRank.rank), new JProperty("tier", bb.LimitedRank.tier)));
		}
		if (bb.UpdatedProperties != null)
		{
			jObject.Add("UpdatedProperties", new JArray(bb.UpdatedProperties));
		}
		if (bb.ManaMovement.IsValid)
		{
			jObject.Add("ManaMovement", new JObject(new JProperty("SourceType", bb.ManaMovement.SourceType.ToString()), new JProperty("SinkType", bb.ManaMovement.SinkType.ToString()), new JProperty("Color", bb.ManaMovement.Color.ToString()), new JProperty("SubstitutionGrpId", bb.ManaMovement.SubstitutionGrpId)));
		}
		if (bb.HoverFaceHangerCount != 0)
		{
			jObject.Add("HoverFaceHangerCount", bb.HoverFaceHangerCount);
		}
		if (bb.ExamineFaceHangerCount != 0)
		{
			jObject.Add("ExamineFaceHangerCount", bb.ExamineFaceHangerCount);
		}
		if (bb.CanCraft)
		{
			jObject.Add("CanCraft", bb.CanCraft);
		}
		if (!string.IsNullOrEmpty(bb.EmoteId))
		{
			jObject.Add("EmoteId", bb.EmoteId);
		}
		if (!string.IsNullOrEmpty(bb.IncomingEmoteId))
		{
			jObject.Add("IncomingEmoteId", bb.IncomingEmoteId);
		}
		return jObject;
	}
}
