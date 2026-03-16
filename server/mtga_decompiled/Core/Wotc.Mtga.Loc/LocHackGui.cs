using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Loc;

public class LocHackGui : IDebugGUIPage
{
	private string title = string.Empty;

	private string message = string.Empty;

	private Dictionary<string, string> titleParams = new Dictionary<string, string>();

	private Dictionary<string, string> messageParams = new Dictionary<string, string>();

	private DebugInfoIMGUIOnGui _GUI;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.LocHacks;

	public string TabName => "Loc Hacks";

	public bool HiddenInTab => false;

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_GUI = gui;
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
		_GUI.ShowLabel("------- Localization Hacks -------");
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("Title: ");
		title = _GUI.ShowTextField(title);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("Message: ");
		message = _GUI.ShowTextField(message);
		GUILayout.EndHorizontal();
		if (_GUI.ShowDebugButton("Check For Params", 500f))
		{
			titleParams.Clear();
			messageParams.Clear();
			GetParams(title).ForEach(delegate(string x)
			{
				titleParams.Add(x, string.Empty);
			});
			GetParams(message).ForEach(delegate(string x)
			{
				messageParams.Add(x, string.Empty);
			});
		}
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		_GUI.ShowLabel("Title Params: ");
		foreach (string item in new List<string>(titleParams.Keys))
		{
			GUILayout.BeginHorizontal();
			_GUI.ShowLabel(item);
			titleParams[item] = _GUI.ShowTextField(titleParams[item]);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		GUILayout.BeginVertical();
		_GUI.ShowLabel("Message Params: ");
		foreach (string item2 in new List<string>(messageParams.Keys))
		{
			GUILayout.BeginHorizontal();
			_GUI.ShowLabel(item2);
			messageParams[item2] = _GUI.ShowTextField(messageParams[item2]);
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		if (_GUI.ShowDebugButton("Translate", 500f))
		{
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText(title, titleParams.AsTuples()), Languages.ActiveLocProvider.GetLocalizedText(message, messageParams.AsTuples()), showCancel: true);
		}
	}

	private List<string> GetParams(string key)
	{
		string localizedText = Languages.ActiveLocProvider.GetLocalizedText(key);
		List<string> list = new List<string>();
		if (localizedText.Contains('{') && localizedText.Contains('}'))
		{
			foreach (string item in localizedText.Split('{', '}').ToList())
			{
				if (item.Length > 1 && !item.Contains(" ") && !item.Contains("\n"))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}
}
