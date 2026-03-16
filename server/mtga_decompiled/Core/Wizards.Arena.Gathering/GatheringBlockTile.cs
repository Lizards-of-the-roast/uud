using AK.Wwise;
using MTGA.Social;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;

namespace Wizards.Arena.Gathering;

public class GatheringBlockTile : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _username;

	[SerializeField]
	private Button _buttonRemoveBlock;

	[SerializeField]
	private AK.Wwise.Event _removeBlockAudioEvent;

	private Block _block;

	private ISocialManager _socialManager;

	private void Awake()
	{
		_socialManager = Pantry.Get<ISocialManager>();
		_buttonRemoveBlock.onClick.AddListener(OnButton_RemoveBlock);
	}

	public void Init(Block block)
	{
		_block = block;
		_username.text = block.BlockedPlayer.DisplayName;
	}

	private void OnButton_RemoveBlock()
	{
		AudioManager.PlayAudio(_removeBlockAudioEvent.Name, base.gameObject);
		_socialManager.RemoveBlock(_block);
	}

	private void OnDestroy()
	{
		_block = null;
		_buttonRemoveBlock.onClick.RemoveListener(OnButton_RemoveBlock);
	}
}
