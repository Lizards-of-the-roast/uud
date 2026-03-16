using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Replays;

public class Debug_ReplayLauncher : MonoBehaviour
{
	[SerializeField]
	private Dropdown _replayDropdown;

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _startReplayButton;

	[SerializeField]
	private Button _confluenceButton;

	private List<ReplayInfo> _replayList = new List<ReplayInfo>();

	public event Action CloseClicked;

	public event Action<ReplayInfo> StartReplayClicked;

	private void Awake()
	{
		_replayList = ReplayUtilities.FindReplayInfo(ReplayUtilities.GetReplayFolder(), new ReplayFormat[4]
		{
			ReplayFormat.Compressed,
			ReplayFormat.Text,
			ReplayFormat.TimedReplay,
			ReplayFormat.JsonFilesInFolder
		});
		List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
		foreach (ReplayInfo replay in _replayList)
		{
			list.Add(new Dropdown.OptionData(Path.GetFileNameWithoutExtension(replay.ReplayPath)));
		}
		_replayDropdown.ClearOptions();
		_replayDropdown.AddOptions(list);
		_closeButton.onClick.AddListener(OnCloseClicked);
		_startReplayButton.onClick.AddListener(OnStartReplayButtonClicked);
		_confluenceButton.onClick.AddListener(OnConfluenceClicked);
	}

	private void OnCloseClicked()
	{
		this.CloseClicked?.Invoke();
	}

	private void OnStartReplayButtonClicked()
	{
		int value = _replayDropdown.value;
		if (value >= 0 && _replayList.Count > 0)
		{
			this.StartReplayClicked?.Invoke(_replayList[value]);
		}
	}

	private void OnConfluenceClicked()
	{
		Application.OpenURL("https://wizardsofthecoast.atlassian.net/wiki/display/MDN/Client+Side+Replays");
	}

	private void OnDestroy()
	{
		_replayDropdown.ClearOptions();
		_closeButton.onClick.RemoveAllListeners();
		_startReplayButton.onClick.RemoveAllListeners();
		this.CloseClicked = null;
		this.StartReplayClicked = null;
		_replayList.Clear();
	}
}
