using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizards.Mtga.AssetBundles.Watcher;

public class AssetBundleWatcherGUI : IWatcherGUI, IDebugGUIPage
{
	private bool _isEditorGUI;

	private GUIStyle _guiStyleActiveTabButtonBk;

	private string _currentTab = "Watcher";

	private string _assetBundleFilter = "";

	private bool _recordLogs;

	private Vector2 _scrollPosition = Vector2.zero;

	private List<bool> toggled = new List<bool>();

	private const float ITEM_HEIGHT = 200f;

	private List<bool> toggledBundles = new List<bool>();

	private Vector2 _scrollPosition3 = Vector2.zero;

	private Vector2 _scrollPosition2 = Vector2.zero;

	private const float ITEM_HEIGHT_BUNDLE_EXPAND = 200f;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Watcher;

	public string TabName => "AssetBundle Watcher";

	public bool HiddenInTab => true;

	public AssetBundleWatcherGUI(bool isEditorGUI)
	{
		_isEditorGUI = isEditorGUI;
	}

	public void Init(DebugInfoIMGUIOnGui gui)
	{
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		return true;
	}

	public void OnGUI()
	{
		_recordLogs = GUILayout.Toggle(_recordLogs, "Record Requests");
		if (!Application.isPlaying || AssetBundleManager.Instance == null)
		{
			return;
		}
		AssetBundleManager.Instance.ABLogger.RecordLogs = _recordLogs;
		GUILayout.BeginHorizontal();
		ShowDebugTabButton("Watcher");
		ShowDebugTabButton("Loaded Bundles");
		GUILayout.EndHorizontal();
		GUILayout.Space(15f);
		string currentTab = _currentTab;
		if (!(currentTab == "Watcher"))
		{
			if (currentTab == "Loaded Bundles")
			{
				OnGUI_DrawBundleControls();
				OnGUI_DrawBundles(GetAssetBundlesList());
			}
		}
		else
		{
			OnGUI_DrawLogControls(AssetBundleManager.Instance.ABLogger);
			OnGUI_DrawLogs(AssetBundleManager.Instance.ABLogger);
		}
	}

	private void OnGUI_DrawLogControls(AssetBundleLogger abLogger)
	{
		GUILayout.BeginHorizontal(GUI.skin.box);
		if (GUILayout.Button("Clear Logs"))
		{
			abLogger.Logs.Clear();
		}
		GUILayout.EndHorizontal();
	}

	public void GetScrollViewFillerValues(float yScrollPos, int totalElementCount, float averageItemHeight, int maxVisibleCount, out int firstVisibleIndex, out int lastVisibleIndex, out float beginningFiller, out float endingFiller)
	{
		int num = Mathf.Min(totalElementCount, maxVisibleCount);
		firstVisibleIndex = Mathf.FloorToInt(yScrollPos / averageItemHeight);
		if (firstVisibleIndex > totalElementCount - num)
		{
			firstVisibleIndex = totalElementCount - num;
		}
		lastVisibleIndex = firstVisibleIndex + num;
		if (lastVisibleIndex >= totalElementCount)
		{
			lastVisibleIndex = totalElementCount - 1;
		}
		beginningFiller = (float)firstVisibleIndex * averageItemHeight;
		endingFiller = (float)(totalElementCount - lastVisibleIndex) * averageItemHeight;
	}

	private void OnGUI_DrawLogs(AssetBundleLogger abLogger)
	{
		float yScrollPos = 0f;
		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
		GetScrollViewFillerValues(yScrollPos, abLogger.Logs.Count, 15f, 200, out var firstVisibleIndex, out var lastVisibleIndex, out var beginningFiller, out var endingFiller);
		GUILayout.Space(beginningFiller);
		for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
		{
			if (i >= toggled.Count)
			{
				toggled.Add(item: false);
			}
			AssetBundleLog log = abLogger.Logs[i];
			if (OnGUI_DrawLogs_CollapsedEntry(log, i))
			{
				OnGUI_DrawLogs_ExpandedEntry(log);
			}
		}
		GUILayout.Space(endingFiller);
		GUILayout.EndScrollView();
	}

	private bool OnGUI_DrawLogs_CollapsedEntry(AssetBundleLog log, int i)
	{
		GUI.backgroundColor = getColorFromType(log.Operation);
		GUILayout.BeginHorizontal(GUI.skin.box);
		toggled[i] = GUILayout.Toggle(toggled[i], toggled[i] ? "+" : "-");
		GUILayout.Label("[" + log.Header + "]");
		GUILayout.Label("[" + log.TimeStamp.ToLongTimeString() + "]");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUI.backgroundColor = Color.white;
		return toggled[i];
		static Color getColorFromType(ABOperation op)
		{
			return op switch
			{
				ABOperation.Unloaded => Color.red, 
				ABOperation.RefRemoved => Color.magenta, 
				_ => Color.grey, 
			};
		}
	}

	private void OnGUI_DrawLogs_ExpandedEntry(AssetBundleLog log)
	{
		GUI.backgroundColor = Color.grey;
		GUILayout.BeginVertical(GUI.skin.box);
		GUI.backgroundColor = Color.white;
		GUILayout.Label("Reference Count: " + log.TotalRefCount);
		if (log.Operation != ABOperation.Unloaded)
		{
			GUILayout.Label("Asset:" + log.AssetName.ToString());
			if (log.Operation != ABOperation.Loaded)
			{
				GUILayout.Label("Reference- Count:" + log.AssetCount);
			}
		}
		GUILayout.EndVertical();
	}

	private void OnGUI_DrawBundleControls()
	{
		GUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Space(30f);
		_assetBundleFilter = GUILayout.TextField(_assetBundleFilter, 100, "Filter");
		GUILayout.EndHorizontal();
	}

	private IReadOnlyList<AssetBundleManager.LoadedAssetBundle> GetAssetBundlesList()
	{
		if (string.IsNullOrWhiteSpace(_assetBundleFilter))
		{
			return AssetBundleManager.Instance.AllLoadedAssetBundles;
		}
		List<AssetBundleManager.LoadedAssetBundle> list = new List<AssetBundleManager.LoadedAssetBundle>();
		foreach (AssetBundleManager.LoadedAssetBundle allLoadedAssetBundle in AssetBundleManager.Instance.AllLoadedAssetBundles)
		{
			if (allLoadedAssetBundle.AssetBundle.name.Contains(_assetBundleFilter))
			{
				list.Insert(0, allLoadedAssetBundle);
			}
			else if (!string.IsNullOrWhiteSpace(allLoadedAssetBundle.AssetsRefCount.Keys.FirstOrDefault((string k) => k.Contains(_assetBundleFilter))))
			{
				list.Add(allLoadedAssetBundle);
			}
		}
		return list;
	}

	private void OnGUI_DrawBundles(IReadOnlyList<AssetBundleManager.LoadedAssetBundle> assetBundles)
	{
		assetBundles = assetBundles.OrderByDescending((AssetBundleManager.LoadedAssetBundle a) => a.Dependencies.DepOfDepsCount).ToList();
		GUILayout.Label("Asset Bundles Loaded: " + AssetBundleManager.Instance.AllLoadedAssetBundles.Count);
		float yScrollPos = 0f;
		_scrollPosition3 = GUILayout.BeginScrollView(_scrollPosition3);
		GetScrollViewFillerValues(yScrollPos, assetBundles.Count, 200f, 200, out var firstVisibleIndex, out var lastVisibleIndex, out var _, out var _);
		for (int num = firstVisibleIndex; num <= lastVisibleIndex; num++)
		{
			if (num >= toggledBundles.Count)
			{
				toggledBundles.Add(item: false);
			}
			AssetBundleManager.LoadedAssetBundle assetBundle = assetBundles[num];
			if (OnGUI_DrawAssetBundle_CollapsedEntry(assetBundle, num))
			{
				OnGUI_DrawAssetBundle_ExpandedEntry(assetBundle);
			}
		}
		GUILayout.Space(2000f);
		GUILayout.EndScrollView();
	}

	private bool OnGUI_DrawAssetBundle_CollapsedEntry(AssetBundleManager.LoadedAssetBundle assetBundle, int i)
	{
		GUI.backgroundColor = Color.grey;
		GUILayout.BeginHorizontal(GUI.skin.box);
		toggledBundles[i] = GUILayout.Toggle(toggledBundles[i], toggledBundles[i] ? "+" : "-");
		if (!string.IsNullOrWhiteSpace(_assetBundleFilter) && assetBundle.AssetBundle.name.Contains(_assetBundleFilter))
		{
			GUI.contentColor = Color.yellow;
		}
		else
		{
			GUI.contentColor = Color.white;
		}
		GUILayout.Label("[" + assetBundle.RefCount + "]");
		GUILayout.Label(assetBundle.AssetBundle.name ?? "");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUI.backgroundColor = Color.white;
		GUI.contentColor = Color.white;
		return toggledBundles[i];
	}

	private void OnGUI_DrawAssetBundle_ExpandedEntry(AssetBundleManager.LoadedAssetBundle assetBundle)
	{
		GUI.backgroundColor = Color.grey;
		GUILayout.BeginVertical(GUI.skin.box);
		GUI.backgroundColor = Color.white;
		List<KeyValuePair<string, int>> list = assetBundle.AssetsRefCount.OrderByDescending((KeyValuePair<string, int> kvp) => kvp.Value).ToList();
		if (assetBundle.Dependencies.Loaded)
		{
			GUILayout.Label("[Dependencies] Count: " + assetBundle.Dependencies.DirectCount + " | Dep of Deps: " + assetBundle.Dependencies.DepOfDepsCount);
		}
		float yScrollPos = 0f;
		_scrollPosition2 = GUILayout.BeginScrollView(_scrollPosition2, GUILayout.Width(800f), GUILayout.Height(list.Count * 25));
		GetScrollViewFillerValues(yScrollPos, list.Count, 200f, 200, out var firstVisibleIndex, out var lastVisibleIndex, out var beginningFiller, out var _);
		GUILayout.Space(beginningFiller);
		for (int num = firstVisibleIndex; num <= lastVisibleIndex; num++)
		{
			KeyValuePair<string, int> keyValuePair = list[num];
			if (!string.IsNullOrWhiteSpace(_assetBundleFilter) && keyValuePair.Key.Contains(_assetBundleFilter))
			{
				GUI.contentColor = Color.yellow;
			}
			else
			{
				GUI.contentColor = Color.white;
			}
			GUILayout.Label("[" + keyValuePair.Value + "] " + keyValuePair.Key);
			GUI.contentColor = Color.white;
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	private void ShowDebugTabButton(string buttonName)
	{
		if (_guiStyleActiveTabButtonBk == null)
		{
			_guiStyleActiveTabButtonBk = new GUIStyle(GUI.skin.button);
			_guiStyleActiveTabButtonBk.fontStyle = FontStyle.BoldAndItalic;
			_guiStyleActiveTabButtonBk.normal.textColor = Color.green;
			_guiStyleActiveTabButtonBk.active.textColor = Color.green;
			_guiStyleActiveTabButtonBk.hover.textColor = Color.green;
			_guiStyleActiveTabButtonBk.focused.textColor = Color.green;
		}
		if (_currentTab == buttonName)
		{
			GUILayout.Button(buttonName, _guiStyleActiveTabButtonBk, GUILayout.Width(100f));
		}
		else if (GUILayout.Button(buttonName, GUILayout.Width(100f)))
		{
			_currentTab = buttonName;
		}
	}
}
