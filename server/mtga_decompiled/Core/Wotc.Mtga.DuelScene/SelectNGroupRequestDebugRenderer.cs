using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class SelectNGroupRequestDebugRenderer : BaseUserRequestDebugRenderer<SelectNGroupRequest>
{
	public SelectNGroupRequestDebugRenderer(SelectNGroupRequest request)
		: base(request)
	{
	}

	public override void Render()
	{
		for (int i = 0; i < _request.Groups.Count; i++)
		{
			if (GUILayout.Button("Group/Pile " + (i + 1)))
			{
				_request.SubmitGroupSelection((uint)_request.Groups[i].GroupId);
			}
		}
	}
}
