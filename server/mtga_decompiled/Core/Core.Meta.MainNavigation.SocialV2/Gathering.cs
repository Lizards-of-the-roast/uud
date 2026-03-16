using System;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MetaNavigation.SocialV2;
using Wizards.Arena.Enums;
using Wizards.Arena.Enums.Gathering;
using Wizards.Arena.Models.Network;

namespace Core.Meta.MainNavigation.SocialV2;

public struct Gathering
{
	private string _id;

	private string _name;

	private string _ownerId;

	private string _icon;

	private string _description;

	private string _inviteCode;

	private Language _language;

	private Core.Meta.MetaNavigation.SocialV2.GatheringFocus _focus;

	private List<GatheringPlayer> _players;

	public string Id => _id;

	public string Name => _name;

	public string OwnerId => _ownerId;

	public string Icon => _icon;

	public string Description => _description;

	public string InviteCode => _inviteCode;

	public Language Language => _language;

	public Core.Meta.MetaNavigation.SocialV2.GatheringFocus Focus => _focus;

	public IReadOnlyCollection<GatheringPlayer> Players => _players.AsReadOnly();

	public Gathering(string id, string name, string ownerId, string icon, string description, string inviteCode, IEnumerable<GatheringPlayer> players, Language language = Language.English, Core.Meta.MetaNavigation.SocialV2.GatheringFocus focus = Core.Meta.MetaNavigation.SocialV2.GatheringFocus.Standard)
	{
		_id = id;
		_name = name;
		_ownerId = ownerId;
		_icon = icon;
		_description = description;
		_inviteCode = inviteCode;
		_language = language;
		_focus = focus;
		_players = new List<GatheringPlayer>(players);
	}

	public static explicit operator Gathering(Wizards.Arena.Models.Network.Gathering networkGathering)
	{
		List<GatheringPlayer> players = new List<GatheringPlayer>(networkGathering.Players.Cast<GatheringPlayer>());
		return new Gathering(networkGathering.GatheringId, networkGathering.GatheringName, networkGathering.OwnerPlayerId, networkGathering.Icon, networkGathering.Description, networkGathering.InviteCode, players, networkGathering.Language, ConvertListOfFocuses(networkGathering.Focuses));
	}

	private static Core.Meta.MetaNavigation.SocialV2.GatheringFocus ConvertListOfFocuses(ICollection<Wizards.Arena.Enums.Gathering.GatheringFocus> focuses)
	{
		Core.Meta.MetaNavigation.SocialV2.GatheringFocus gatheringFocus = Core.Meta.MetaNavigation.SocialV2.GatheringFocus.None;
		foreach (Wizards.Arena.Enums.Gathering.GatheringFocus focuse in focuses)
		{
			gatheringFocus = focuse switch
			{
				Wizards.Arena.Enums.Gathering.GatheringFocus.Standard => gatheringFocus | Core.Meta.MetaNavigation.SocialV2.GatheringFocus.Standard, 
				Wizards.Arena.Enums.Gathering.GatheringFocus.Limited => gatheringFocus | Core.Meta.MetaNavigation.SocialV2.GatheringFocus.Limited, 
				Wizards.Arena.Enums.Gathering.GatheringFocus.HangingOut => gatheringFocus | Core.Meta.MetaNavigation.SocialV2.GatheringFocus.HangingOut, 
				Wizards.Arena.Enums.Gathering.GatheringFocus.SocialCause => gatheringFocus | Core.Meta.MetaNavigation.SocialV2.GatheringFocus.SocialCause, 
				Wizards.Arena.Enums.Gathering.GatheringFocus.Brawl => gatheringFocus | Core.Meta.MetaNavigation.SocialV2.GatheringFocus.Brawl, 
				Wizards.Arena.Enums.Gathering.GatheringFocus.Historic => gatheringFocus | Core.Meta.MetaNavigation.SocialV2.GatheringFocus.Historic, 
				Wizards.Arena.Enums.Gathering.GatheringFocus.Timeless => gatheringFocus | Core.Meta.MetaNavigation.SocialV2.GatheringFocus.Timeless, 
				Wizards.Arena.Enums.Gathering.GatheringFocus.Pioneer => gatheringFocus | Core.Meta.MetaNavigation.SocialV2.GatheringFocus.Pioneer, 
				Wizards.Arena.Enums.Gathering.GatheringFocus.Alchemy => gatheringFocus | Core.Meta.MetaNavigation.SocialV2.GatheringFocus.Alchemy, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		return gatheringFocus;
	}
}
