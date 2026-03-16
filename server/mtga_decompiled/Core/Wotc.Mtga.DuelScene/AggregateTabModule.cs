using System.Collections.Generic;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class AggregateTabModule : DebugModule
{
	private readonly string _name;

	private readonly string _description;

	private readonly List<DebugModule> _modules = new List<DebugModule>();

	private int _selectedIdx;

	public IEnumerable<DebugModule> NestedModules => _modules;

	public override string Name => _name;

	public override string Description => _description;

	public AggregateTabModule(string name, string description, IEnumerable<DebugModule> modules)
	{
		_name = name ?? string.Empty;
		_description = description ?? string.Empty;
		_modules.AddRange(modules);
	}

	public override void Render()
	{
		int selectionIndex = DrawModuleSelections();
		_modules[_selectedIdx].Render();
		SetSelectionIndex(selectionIndex);
	}

	public void SetSelectionIndex(int idx)
	{
		if (idx < _modules.Count)
		{
			_selectedIdx = idx;
		}
	}

	private int DrawModuleSelections()
	{
		int result = _selectedIdx;
		GUILayout.BeginHorizontal();
		for (int i = 0; i < _modules.Count; i++)
		{
			DebugModule debugModule = _modules[i];
			GUI.backgroundColor = ((_selectedIdx == i) ? Color.green : Color.white);
			if (GUILayout.Button(debugModule.Name))
			{
				result = i;
			}
			GUI.backgroundColor = Color.white;
		}
		GUILayout.EndVertical();
		return result;
	}
}
