using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Wizards.Arena.Promises;

namespace Wizards.Mtga.Npe.CustomNodes;

[UnitCategory("NPE")]
public class PrepopulateNpeFlagStatusAsync : Unity.VisualScripting.Unit
{
	[DoNotSerialize]
	[PortLabel("NPE Progression Flag")]
	private ValueInput _npeProgressionFlag;

	[PortLabelHidden]
	[DoNotSerialize]
	public ControlInput _enter { get; private set; }

	[PortLabelHidden]
	[DoNotSerialize]
	public ControlOutput _exit { get; private set; }

	protected override void Definition()
	{
		_enter = ControlInputCoroutine("_enter", Run);
		_exit = ControlOutput("_exit");
		_npeProgressionFlag = ValueInput<NpeProgressionFlag[]>("_npeProgressionFlag", null);
		Succession(_enter, _exit);
	}

	private IEnumerator Run(Flow flow)
	{
		NpeProgressionFlag[] value = flow.GetValue<NpeProgressionFlag[]>(_npeProgressionFlag);
		if (value == null)
		{
			yield break;
		}
		List<Promise<bool>> progressionFlags = new List<Promise<bool>>();
		NpeProgressionFlag[] array = value;
		foreach (NpeProgressionFlag npeProgressionFlag in array)
		{
			progressionFlags.Add(npeProgressionFlag.FlagHasBeenCompleted());
		}
		while (progressionFlags.Any((Promise<bool> x) => !x.IsDone))
		{
			yield return null;
		}
		foreach (Promise<bool> item in progressionFlags)
		{
			if (item.State == PromiseState.Error)
			{
				Debug.LogError($"Error prepopulating NPE flag status: {item.Error.Message}\nError Code: {item.Error.Code}\nException: {item.Error.Exception}");
			}
		}
		flow.Invoke(_exit);
	}
}
