using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterLibrary", menuName = "ScriptableObject/CharacterLibrary", order = 10)]
public class CharacterLibrary : ScriptableObject
{
	[SerializeField]
	public List<AvatarCharacterData> Characters;
}
