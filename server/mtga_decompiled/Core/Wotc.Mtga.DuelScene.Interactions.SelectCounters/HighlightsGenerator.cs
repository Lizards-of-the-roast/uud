using System;
using Pooling;
using WorkflowVisuals;

namespace Wotc.Mtga.DuelScene.Interactions.SelectCounters;

public class HighlightsGenerator : IHighlightsGenerator, IDisposable
{
	private readonly IObjectPool _pool;

	private readonly Func<(uint, uint)> _getCurrentCounterSelection;

	private readonly Highlights _highlights;

	public HighlightsGenerator(IObjectPool pool, Func<(uint, uint)> getCounterSelection)
	{
		_pool = pool ?? NullObjectPool.Default;
		_getCurrentCounterSelection = getCounterSelection ?? ((Func<(uint, uint)>)(() => (0u, 0u)));
		_highlights = _pool.PopObject<Highlights>();
	}

	public void Dispose()
	{
		_highlights.Clear();
		_pool.PushObject(_highlights);
	}

	public Highlights GetHighlights()
	{
		_highlights.Clear();
		uint item = _getCurrentCounterSelection().Item1;
		if (item != 0)
		{
			_highlights.IdToHighlightType_Workflow[item] = HighlightType.Selected;
		}
		return _highlights;
	}
}
