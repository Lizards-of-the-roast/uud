using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleMessageView : MonoBehaviour
{
	[Serializable]
	public class LocEntry
	{
		public string Key;

		public string Value;

		public LocEntry(string key, string value)
		{
			Key = key;
			Value = value;
		}
	}

	public Text title;

	public Text message;

	public Text buttonText;

	public List<LocEntry> titleLoc = new List<LocEntry>
	{
		new LocEntry("en-US", "Error")
	};

	public List<LocEntry> messageLoc = new List<LocEntry>
	{
		new LocEntry("en-US", "Corrupt data found. Please uninstall and re-install app")
	};

	public List<LocEntry> butoonLoc = new List<LocEntry>
	{
		new LocEntry("en-US", "OK")
	};

	private void Awake()
	{
		string lang = (titleLoc.Exists((LocEntry x) => x.Key == MDNPlayerPrefs.PLAYERPREFS_ClientLanguage) ? MDNPlayerPrefs.PLAYERPREFS_ClientLanguage : "en-US");
		title.text = titleLoc.Find((LocEntry x) => x.Key == lang).Value;
		message.text = messageLoc.Find((LocEntry x) => x.Key == lang).Value;
		buttonText.text = butoonLoc.Find((LocEntry x) => x.Key == lang).Value;
	}
}
