using System.Collections.Generic;
using System.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable.ManaSelections;

public abstract class ManaSelection
{
	protected readonly List<ManaColor> _selectionTotal;

	protected readonly List<ManaColor> _selectedColors;

	protected ManaColor _selected;

	protected List<ManaGroup> _manaGroups;

	private readonly List<(Action, ManaPaymentOption)> _manaActions;

	public abstract IEnumerable<ManaColor> SelectableColors { get; }

	public IEnumerable<ManaColor> SelectionTotal => _selectionTotal;

	public int SelectedCount => _selectedColors.Count;

	public virtual bool ShouldPrune => true;

	public abstract uint MaxColorCount { get; }

	public abstract bool HasConstantCount { get; }

	public abstract uint CountForColor(ManaColor color);

	public ManaSelection()
	{
		_selected = ManaColor.None;
		_selectedColors = new List<ManaColor>();
		_selectionTotal = new List<ManaColor>();
		_manaActions = new List<(Action, ManaPaymentOption)>();
	}

	public virtual void Init(List<ManaGroup> manaGroups)
	{
		_manaGroups = manaGroups;
	}

	public virtual void Prune(IEnumerable<(Action, ManaPaymentOption)> paymentOptions)
	{
		_manaGroups.RemoveAll((ManaGroup x) => !paymentOptions.Contains(x.ManaAction));
	}

	public void Init(ManaSelection parent)
	{
		_selectedColors.AddRange(parent._selectedColors);
		_selectionTotal.AddRange(parent._selectionTotal);
		List<ManaGroup> manaGroups = parent._manaGroups.ToList();
		PruneColorGroups(manaGroups, parent._selected, parent.CountForColor(parent._selected));
		Init(manaGroups);
	}

	private void PruneColorGroups(List<ManaGroup> manaGroups, ManaColor selected, uint count)
	{
		if (ShouldPrune)
		{
			manaGroups.RemoveAll((ManaGroup x) => !x.Contains(selected) || x.Count(selected) < count);
			manaGroups.ForEach(delegate(ManaGroup x)
			{
				x.Remove(selected);
			});
		}
	}

	public void Select(ManaColor color)
	{
		_selected = color;
		_selectedColors.Add(_selected);
		uint num = CountForColor(_selected);
		for (int i = 0; i < num; i++)
		{
			_selectionTotal.Add(_selected);
		}
	}

	public virtual (Action, ManaPaymentOption) GetManaAction()
	{
		IEnumerable<(Action, ManaPaymentOption)> allManaActions = GetAllManaActions();
		if (allManaActions.Count() > 1)
		{
			foreach (var item in allManaActions)
			{
				if (item.Item2.Mana.Count == SelectionTotal.Count())
				{
					return item;
				}
			}
		}
		return allManaActions.First();
	}

	public IEnumerable<(Action, ManaPaymentOption)> GetAllManaActions()
	{
		if (_manaActions.Count != _manaGroups.Count)
		{
			_manaActions.Clear();
			foreach (ManaGroup manaGroup in _manaGroups)
			{
				_manaActions.Add(manaGroup.ManaAction);
			}
		}
		return _manaActions;
	}

	public void ClearSelection()
	{
		if (_selected != ManaColor.None)
		{
			for (int i = 0; i < CountForColor(_selected); i++)
			{
				_selectionTotal.Remove(_selected);
			}
			_selectedColors.Remove(_selected);
			_selected = ManaColor.None;
		}
	}
}
