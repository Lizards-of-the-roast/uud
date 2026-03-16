using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HasbroGo;

public class FriendTileHeader : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private TextMeshProUGUI headerTitle;

	[SerializeField]
	private Color titleHighlightedColor;

	private List<FriendTile> friendTiles = new List<FriendTile>();

	private Color titleNormalColor;

	private void Awake()
	{
		titleNormalColor = headerTitle.color;
	}

	private void OnDestroy()
	{
		friendTiles.Clear();
	}

	public void Init(string _headerTitle)
	{
		headerTitle.text = _headerTitle;
	}

	public void AddFriendTile(FriendTile tile)
	{
		friendTiles.Add(tile);
	}

	public void ToggleFriendTileGroup()
	{
		friendTiles.ForEach(delegate(FriendTile tile)
		{
			tile.gameObject.SetActive(!tile.gameObject.activeSelf);
		});
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		headerTitle.color = titleHighlightedColor;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		headerTitle.color = titleNormalColor;
	}
}
