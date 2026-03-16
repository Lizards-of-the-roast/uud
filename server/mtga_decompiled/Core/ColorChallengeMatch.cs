using UnityEngine;

[CreateAssetMenu(fileName = "ColorChallengeMatch", menuName = "ScriptableObject/Brassman/ColorChallengeMatch")]
public class ColorChallengeMatch : ScriptableObject
{
	[Header("Lesson Options")]
	public string Title;

	public string Description;

	public uint FeaturedGRPID;

	public string HintTitle;

	public string HintDescription;

	public Sprite HintDiagram;

	[Header("Player Options")]
	public string PlayersAvatar;

	[ChallengeMatchDeck]
	public string Player_Deck;

	public uint PlayerDeckGRPID;

	[Header("AI Options")]
	public string AIsAvatar;

	public Sprite AIsAvatarImage;

	[ChallengeMatchDeck]
	public string AI_Deck;

	public DeckHeuristic AIHeuristic;

	public bool AIGoesFirst;
}
