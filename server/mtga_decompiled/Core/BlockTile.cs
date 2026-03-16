using System;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class BlockTile : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _labelName;

	[SerializeField]
	private Button _buttonRemoveBlock;

	public Action<Block> Callback_RemoveBlock;

	public Block Block { get; private set; }

	private void Awake()
	{
		_buttonRemoveBlock.onClick.AddListener(OnButton_RemoveBlock);
	}

	public void Init(Block block)
	{
		base.gameObject.UpdateActive(active: true);
		Block = block;
		SetName(block.BlockedPlayer.DisplayName);
	}

	private void OnButton_RemoveBlock()
	{
		AudioManager.PlayAudio("sfx_ui_friends_click", base.gameObject);
		Callback_RemoveBlock?.Invoke(Block);
	}

	private void SetName(string newName)
	{
		_labelName.text = newName;
	}

	public void OnDestroy()
	{
		Block = null;
		Callback_RemoveBlock = null;
	}
}
