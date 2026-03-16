using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public static class BrowserConstants
{
	public const string DISMISS_BUTTON = "DismissButton";

	public const string VIEW_BATTLEFIELD_BUTTON = "ViewBattlefield";

	public const string SHOW_ALL_BUTTON = "ShowAllButton";

	public const string DONE_BUTTON = "DoneButton";

	public const string CANCEL_BUTTON = "CancelButton";

	public const string YES_BUTTON = "YesButton";

	public const string NO_BUTTON = "NoButton";

	public const string DECLINE_BUTTON = "DeclineButton";

	public const string SUBMIT_BUTTON = "SubmitButton";

	public const string GROUP_A_BUTTON = "GroupAButton";

	public const string GROUP_B_BUTTON = "GroupBButton";

	public const string BUTTON_DEFAULT = "ButtonDefault";

	public const string TWOBUTTON_RIGHT = "2Button_Right";

	public const string TWOBUTTON_LEFT = "2Button_Left";

	public const string SINGLE_BUTTON = "SingleButton";

	public const string KEEP_BUTTON = "KeepButton";

	public const string MULLIGAN_BUTTON = "MulliganButton";

	public const string LAYOUT_KEY_DEFAULT = "Default";

	public const string LAYOUT_KEY_MODAL = "Modal";

	public const string LAYOUT_KEY_OPTIONAL_ACTION = "OptionalAction";

	public const string LAYOUT_KEY_ORDER = "Order";

	public const string LAYOUT_KEY_ASSIGN_DAMAGE = "AssignDamage";

	public const string LAYOUT_KEY_MULTIZONE = "MultiZone";

	public const string LAYOUT_KEY_MULLIGAN = "Mulligan";

	public const string LAYOUT_KEY_BLOCKING = "Blocking";

	public const string LAYOUT_KEY_SCRY = "Scry";

	public const string LAYOUT_KEY_SEARCH = "Search";

	public const string LAYOUT_KEY_DUNGEON_ROOM_SELECT = "DungeonRoomSelect";

	public const string LAYOUT_KEY_TRIGGERED_ORDER = "TriggerOrder";

	public static readonly IReadOnlyList<HashSet<CardColor>> BROWSER_COLOR_COMBINATION_ORDER = new List<HashSet<CardColor>>
	{
		new HashSet<CardColor> { CardColor.White },
		new HashSet<CardColor> { CardColor.Blue },
		new HashSet<CardColor> { CardColor.Black },
		new HashSet<CardColor> { CardColor.Red },
		new HashSet<CardColor> { CardColor.Green },
		new HashSet<CardColor>
		{
			CardColor.Blue,
			CardColor.White
		},
		new HashSet<CardColor>
		{
			CardColor.White,
			CardColor.Black
		},
		new HashSet<CardColor>
		{
			CardColor.Blue,
			CardColor.Black
		},
		new HashSet<CardColor>
		{
			CardColor.Blue,
			CardColor.Red
		},
		new HashSet<CardColor>
		{
			CardColor.Red,
			CardColor.Black
		},
		new HashSet<CardColor>
		{
			CardColor.Black,
			CardColor.Green
		},
		new HashSet<CardColor>
		{
			CardColor.Red,
			CardColor.Green
		},
		new HashSet<CardColor>
		{
			CardColor.Red,
			CardColor.White
		},
		new HashSet<CardColor>
		{
			CardColor.Green,
			CardColor.White
		},
		new HashSet<CardColor>
		{
			CardColor.Blue,
			CardColor.Green
		},
		new HashSet<CardColor>
		{
			CardColor.Green,
			CardColor.White,
			CardColor.Blue
		},
		new HashSet<CardColor>
		{
			CardColor.White,
			CardColor.Blue,
			CardColor.Black
		},
		new HashSet<CardColor>
		{
			CardColor.Blue,
			CardColor.Black,
			CardColor.Red
		},
		new HashSet<CardColor>
		{
			CardColor.Black,
			CardColor.Red,
			CardColor.Green
		},
		new HashSet<CardColor>
		{
			CardColor.Red,
			CardColor.Green,
			CardColor.White
		},
		new HashSet<CardColor>
		{
			CardColor.Red,
			CardColor.White,
			CardColor.Black
		},
		new HashSet<CardColor>
		{
			CardColor.Green,
			CardColor.Blue,
			CardColor.Red
		},
		new HashSet<CardColor>
		{
			CardColor.White,
			CardColor.Black,
			CardColor.Green
		},
		new HashSet<CardColor>
		{
			CardColor.Blue,
			CardColor.Red,
			CardColor.White
		},
		new HashSet<CardColor>
		{
			CardColor.Black,
			CardColor.Green,
			CardColor.Blue
		},
		new HashSet<CardColor>
		{
			CardColor.White,
			CardColor.Blue,
			CardColor.Black,
			CardColor.Red
		},
		new HashSet<CardColor>
		{
			CardColor.White,
			CardColor.Blue,
			CardColor.Black,
			CardColor.Green
		},
		new HashSet<CardColor>
		{
			CardColor.White,
			CardColor.Blue,
			CardColor.Red,
			CardColor.Green
		},
		new HashSet<CardColor>
		{
			CardColor.White,
			CardColor.Black,
			CardColor.Red,
			CardColor.Green
		},
		new HashSet<CardColor>
		{
			CardColor.Blue,
			CardColor.Black,
			CardColor.Red,
			CardColor.Green
		},
		new HashSet<CardColor>
		{
			CardColor.White,
			CardColor.Blue,
			CardColor.Black,
			CardColor.Red,
			CardColor.Green
		},
		new HashSet<CardColor> { CardColor.Colorless }
	};
}
