using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene;

public class CtoModalDebugRenderer : BaseUserRequestDebugRenderer<CastingTimeOption_ModalRequest>
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly List<uint> _selections = new List<uint>();

	public CtoModalDebugRenderer(CastingTimeOption_ModalRequest request, ICardDatabaseAdapter cardDatabaseAdapter)
		: base(request)
	{
		_cardDatabase = cardDatabaseAdapter ?? NullCardDatabaseAdapter.Default;
	}

	public override void Render()
	{
		bool flag = _selections.Count < _request.Max;
		GUILayout.Space(3f);
		foreach (uint modalOption in _request.ModalOptions)
		{
			string abilityTextForOption = GetAbilityTextForOption(modalOption);
			if (string.IsNullOrEmpty(abilityTextForOption))
			{
				continue;
			}
			GUI.enabled = flag || _selections.Contains(modalOption);
			GUI.color = (_selections.Contains(modalOption) ? Color.green : Color.white);
			if (GUILayout.Button(abilityTextForOption))
			{
				if (_selections.Contains(modalOption))
				{
					_selections.Remove(modalOption);
				}
				else
				{
					_selections.Add(modalOption);
				}
			}
			GUI.color = Color.white;
			GUI.enabled = true;
		}
		GUI.enabled = false;
		foreach (uint excludedOption in _request.ExcludedOptions)
		{
			string abilityTextForOption2 = GetAbilityTextForOption(excludedOption);
			if (!string.IsNullOrEmpty(abilityTextForOption2))
			{
				GUILayout.Button(abilityTextForOption2);
			}
		}
		GUI.enabled = true;
		GUILayout.Space(3f);
		GUI.enabled = _selections.Count >= _request.Min && _selections.Count <= _request.Max;
		if (GUILayout.Button("Submit Modal"))
		{
			_request.SubmitModal(_selections);
			_selections.Clear();
		}
		GUI.enabled = true;
	}

	private string GetAbilityTextForOption(uint id)
	{
		if (!_cardDatabase.AbilityDataProvider.TryGetAbilityPrintingById(id, out var ability))
		{
			return string.Empty;
		}
		return _cardDatabase.GreLocProvider.GetLocalizedText(ability.TextId);
	}
}
