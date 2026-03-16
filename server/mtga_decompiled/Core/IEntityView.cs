using UnityEngine;

public interface IEntityView
{
	uint InstanceId { get; }

	Transform ArrowRoot { get; }

	Transform EffectsRoot { get; }

	void UpdateHighlight(HighlightType highlightType);
}
