using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wotc.Mtga.TimedReplays;

namespace Wotc.Mtga.Replays;

public class ReplayGUI : IDebugGUIPage
{
	private string _replayName = "";

	private DebugInfoIMGUIOnGui _gui;

	private List<ReplayInfo> _files = new List<ReplayInfo>();

	private ReplayInfo _selectedReplay;

	private string _selectedReplayData;

	private readonly string _replayFolderCache;

	private Vector2 _scroll;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Replays;

	public string TabName => "Replays";

	public bool HiddenInTab => false;

	public ReplayGUI()
	{
		_replayFolderCache = ReplayUtilities.GetReplayFolder();
	}

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_gui = gui;
		UpdateFiles();
	}

	private void UpdateFiles()
	{
		ReplayFormat[] formats = new ReplayFormat[4]
		{
			ReplayFormat.TimedReplay,
			ReplayFormat.Compressed,
			ReplayFormat.Text,
			ReplayFormat.JsonFilesInFolder
		};
		_files = ReplayUtilities.FindReplayInfo(_replayFolderCache, formats);
	}

	public bool OnUpdate()
	{
		return false;
	}

	public void OnGUI()
	{
		GUILayout.BeginHorizontal();
		DrawDebugButtons();
		if (_selectedReplay != null)
		{
			DrawReplayInfo();
		}
		GUILayout.EndHorizontal();
		void DrawDebugButtons()
		{
			GUILayout.BeginVertical();
			GUILayout.Label("Replay folder: " + _replayFolderCache);
			if (_gui.ShowDebugButton("Open Confluence", 500f))
			{
				Application.OpenURL("https://wizardsofthecoast.atlassian.net/wiki/display/MDN/Recording+Timed+Replays");
			}
			if (UnityUtilities.CanOpenDirectory() && _gui.ShowDebugButton("Open Replay Folder", 500f))
			{
				try
				{
					UnityUtilities.OpenDirectory(_replayFolderCache);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
			if (_gui.ShowDebugButton($"Record matches: {MDNPlayerPrefs.SaveDSReplays}", 500f))
			{
				MDNPlayerPrefs.SaveDSReplays = !MDNPlayerPrefs.SaveDSReplays;
			}
			GUILayout.BeginHorizontal(GUILayout.Width((float)Screen.width / 2f));
			_replayName = _gui.ShowTextField(_replayName);
			if (_gui.ShowDebugButton("Set Replay Name", 500f))
			{
				MDNPlayerPrefs.ReplayName = _replayName;
			}
			GUILayout.EndHorizontal();
			if (_gui.ShowDebugButton("Clear Replay Name", 500f))
			{
				MDNPlayerPrefs.ReplayName = "";
			}
			if (_gui.ShowDebugButton("Refresh Replays", 500f))
			{
				UpdateFiles();
			}
			GUILayout.Space(7f);
			GUILayout.Label("Replays:");
			_scroll = _gui.BeginScrollView(_scroll, GUILayout.Width(500f));
			foreach (ReplayInfo file in _files)
			{
				if (_gui.ShowDebugButton(file.Name, 500f))
				{
					SelectReplay(file);
				}
				GUILayout.Space(3f);
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
		}
		void DrawReplayInfo()
		{
			GUILayout.BeginVertical();
			GUILayout.Label(_selectedReplayData ?? "");
			if (_gui.ShowDebugButton("Play", 500f))
			{
				PAPA pAPA = UnityEngine.Object.FindObjectOfType<PAPA>();
				bool flag = false;
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					if (SceneManager.GetSceneAt(i).name == "DuelSceneDebugLauncher")
					{
						flag = true;
						break;
					}
				}
				ReplayUtilities.StartReplay(_selectedReplay, pAPA, pAPA.MatchManager, pAPA.CardDatabase, pAPA.CardViewBuilder, pAPA.CardMaterialBuilder, delegate
				{
				}, flag ? "DuelSceneDebugLauncher" : null);
			}
			if (_selectedReplay.Format == ReplayFormat.TimedReplay && TimedReplayPlayer.NextReplay != _selectedReplay.ReplayPath && _gui.ShowDebugButton("Load Replay for NPE/Color Challenge", 500f))
			{
				TimedReplayPlayer.NextReplay = _selectedReplay.ReplayPath;
			}
			GUILayout.EndVertical();
		}
	}

	private void SelectReplay(ReplayInfo file)
	{
		if (file.Format == ReplayFormat.TimedReplay)
		{
			if (ReplayReader.TryCreateReplayFromPath(file.ReplayPath, out var replayReader).ResultType == ReplayReader.Result.ResultTypes.Success)
			{
				using (replayReader)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(file.Name);
					var (info, info2) = replayReader.GetPlayerInfo();
					BuildPlayerInfoString("Local Player", stringBuilder, info);
					BuildPlayerInfoString("Opponent", stringBuilder, info2);
					stringBuilder.AppendLine("Battlefield: " + replayReader.GetBattlefield());
					stringBuilder.Append($"Recorded at: {new FileInfo(file.ReplayPath).CreationTime}");
					_selectedReplayData = stringBuilder.ToString();
					_selectedReplay = file;
					return;
				}
			}
			_selectedReplay = null;
			_selectedReplayData = null;
		}
		else
		{
			_selectedReplay = file;
			_selectedReplayData = $"Name: {file.Name}\nReplay format: {file.Format}";
		}
		static void BuildPlayerInfoString(string name, StringBuilder s, PlayerCosmetics playerCosmetics)
		{
			s.AppendLine(name + " Name: " + playerCosmetics.ScreenName);
			if (!string.IsNullOrEmpty(playerCosmetics.PetSelectionName))
			{
				s.AppendLine(name + " Pet: Pet: " + playerCosmetics.PetSelectionName + "-" + playerCosmetics.PetSelectionVariant + ".");
			}
			if (!string.IsNullOrEmpty(playerCosmetics.SleeveSelection))
			{
				s.AppendLine(name + " Sleeve: " + playerCosmetics.SleeveSelection + ".");
			}
			if (!string.IsNullOrEmpty(playerCosmetics.TitleSelection))
			{
				s.AppendLine(name + " Title: " + playerCosmetics.TitleSelection + ".");
			}
		}
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}
}
