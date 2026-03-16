using System.Collections.Generic;
using System.Linq;

namespace Wizards.Mtga;

public class DeckFolderStatesDataProvider
{
	private HashSet<string> _expandedFolders;

	private bool _initialized;

	public static DeckFolderStatesDataProvider Create()
	{
		return new DeckFolderStatesDataProvider();
	}

	public void Initialize()
	{
		_expandedFolders = MDNPlayerPrefs.ExpandedDeckFolders.ToHashSet();
		_initialized = true;
	}

	public void SetFolderState(string key, bool open)
	{
		if (!_initialized)
		{
			SimpleLog.LogError("Attempting to set key before deck folder state data provider is initialized: " + key);
		}
		else if (open)
		{
			_expandedFolders.Add(key);
		}
		else
		{
			_expandedFolders.Remove(key);
		}
	}

	public bool GetFolderState(string key)
	{
		if (!_initialized)
		{
			SimpleLog.LogError("Attempting to get key before deck folder state data provider is initialized: " + key);
			return false;
		}
		return _expandedFolders.Contains(key);
	}

	public void Save()
	{
		if (!_initialized)
		{
			SimpleLog.LogError("Attempting to save deck folder state before data provider is initialized");
		}
		else
		{
			MDNPlayerPrefs.ExpandedDeckFolders = _expandedFolders.ToArray();
		}
	}
}
