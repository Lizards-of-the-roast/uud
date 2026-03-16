using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using Wizards.Arena.Models.Network;
using Wizards.Mtga;

namespace Core.Meta.Social.Tables;

public class TablesPopupPlayersList : MonoBehaviour
{
	[SerializeField]
	private TablePlayerListTile PlayerTilePrefab;

	[SerializeField]
	private Transform PlayerListParent;

	private TablesCornerUI _tablesCornerUI;

	private readonly List<TablePlayerListTile> _currentPlayerTiles = new List<TablePlayerListTile>();

	private IAccountClient AccountClient => Pantry.Get<IAccountClient>();

	private AssetLookupSystem AssetLookupSystem => Pantry.Get<AssetLookupManager>().AssetLookupSystem;

	public void UpdatePlayerListViews(TablesCornerUI tablesCornerUI, List<LobbyPlayer> newPlayers, string lobbyId)
	{
		_tablesCornerUI = tablesCornerUI;
		foreach (TablePlayerListTile currentPlayerTile in _currentPlayerTiles)
		{
			Object.Destroy(currentPlayerTile.gameObject);
		}
		_currentPlayerTiles.Clear();
		foreach (LobbyPlayer item in newPlayers.OrderByDescending((LobbyPlayer p) => IsSelf(AccountClient, p)))
		{
			TablePlayerListTile tablePlayerListTile = Object.Instantiate(PlayerTilePrefab, PlayerListParent);
			tablePlayerListTile.SetState(_tablesCornerUI, AssetLookupSystem, item, lobbyId);
			_currentPlayerTiles.Add(tablePlayerListTile);
		}
	}

	private static bool IsSelf(IAccountClient accountClient, LobbyPlayer player)
	{
		return player.PlayerId == accountClient.AccountInformation.PersonaID;
	}
}
