using System.Collections.Generic;
using System.Text;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class RoomStateHangerProvider : IHangerConfigProvider
{
	private readonly StringBuilder _sb = new StringBuilder();

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly IEntityNameProvider<MtgCardInstance> _cardNameProvider;

	public RoomStateHangerProvider(ICardDatabaseAdapter cardDatabase, IEntityNameProvider<MtgCardInstance> cardNameProvider)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_cardNameProvider = cardNameProvider ?? NullCardNameProvider.Default;
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		_sb.Clear();
		if (model == null)
		{
			yield break;
		}
		MtgCardInstance instance = model.Instance;
		MtgZone zone = instance.Zone;
		if (zone == null || zone.Type != ZoneType.Battlefield || !instance.IsRoomParent() || (instance.Designations.Exists((DesignationData x) => x.Type == Designation.LeftUnlocked) && instance.Designations.Exists((DesignationData x) => x.Type == Designation.RightUnlocked)))
		{
			yield break;
		}
		yield return new HangerConfig(_cardDatabase.ClientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/RoomState/Header"), LockedRoomsDetail(instance));
		if (!HasMismatchedColors(instance, _cardDatabase.CardDataProvider))
		{
			yield break;
		}
		if (instance.Colors.Count == 0)
		{
			_sb.Append(_cardDatabase.GreLocProvider.GetLocalizedTextForEnumValue(CardColor.Colorless));
		}
		else
		{
			for (int num = 0; num < instance.Colors.Count; num++)
			{
				if (num > 0)
				{
					_sb.Append(", ");
				}
				_sb.Append(_cardDatabase.GreLocProvider.GetLocalizedTextForEnumValue(instance.Colors[num]));
			}
		}
		string localizedText = _cardDatabase.ClientLocProvider.GetLocalizedText("AbilityHanger/ColorChange/ColorModified_Header");
		string localizedText2 = _cardDatabase.ClientLocProvider.GetLocalizedText("AbilityHanger/ColorChange/ColorModified_Body_Singular", ("color", _sb.ToString()));
		yield return new HangerConfig(localizedText, localizedText2);
		_sb.Clear();
	}

	private string LockedRoomsDetail(MtgCardInstance card)
	{
		_sb.Clear();
		if (!card.Designations.Exists((DesignationData x) => x.Type == Designation.LeftUnlocked))
		{
			string name = _cardNameProvider.GetName(card.LinkedFaceInstances.Find((MtgCardInstance x) => x.ObjectType == GameObjectType.RoomLeft));
			_sb.Append(_cardDatabase.ClientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/RoomState/Body", ("cardName", name)));
		}
		if (!card.Designations.Exists((DesignationData x) => x.Type == Designation.RightUnlocked))
		{
			if (_sb.Length > 0)
			{
				_sb.AppendLine();
			}
			string name2 = _cardNameProvider.GetName(card.LinkedFaceInstances.Find((MtgCardInstance x) => x.ObjectType == GameObjectType.RoomRight));
			_sb.Append(_cardDatabase.ClientLocProvider.GetLocalizedText("AbilityHanger/SpecialHangers/RoomState/Body", ("cardName", name2)));
		}
		string result = _sb.ToString();
		_sb.Clear();
		return result;
	}

	private static bool HasMismatchedColors(MtgCardInstance instance, ICardDataProvider cardDataProvider)
	{
		if (instance.ColorModifications.Count > 0)
		{
			return false;
		}
		if (cardDataProvider.TryGetCardPrintingById(instance.GrpId, out var card))
		{
			IReadOnlyList<CardColor> colors = card.Colors;
			if (colors.Count != instance.Colors.Count)
			{
				return true;
			}
			for (int i = 0; i < colors.Count; i++)
			{
				if (colors[i] != instance.Colors[i])
				{
					return true;
				}
			}
		}
		return false;
	}
}
