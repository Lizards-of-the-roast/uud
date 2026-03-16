using System;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

[Serializable]
public abstract class BoardstateHeuristic : ScriptableObject
{
	[SerializeField]
	private string _designerNotes;

	[SerializeField]
	private string _designerDescription;

	public abstract bool IsMet(MtgGameState gameState, ICardDatabaseAdapter cardDatabase);
}
