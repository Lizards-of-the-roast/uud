using UnityEngine;

public class CardLayoutData
{
	public DuelScene_CDC Card { get; set; }

	public GameObject CardGameObject { get; set; }

	public Vector3 Position { get; set; } = Vector3.zero;

	public Quaternion Rotation { get; set; } = Quaternion.identity;

	public Vector3 Scale { get; set; } = Vector3.one;

	public bool IsVisibleInLayout { get; set; } = true;

	public CardLayoutData()
	{
	}

	public CardLayoutData(DuelScene_CDC card)
		: this()
	{
		Card = card;
		CardGameObject = card.Root.gameObject;
	}

	public CardLayoutData(DuelScene_CDC card, Vector3 pos)
		: this(card)
	{
		Position = pos;
	}

	public CardLayoutData(DuelScene_CDC card, Vector3 pos, Quaternion rot)
		: this(card, pos)
	{
		Rotation = rot;
	}

	public CardLayoutData(DuelScene_CDC card, Vector3 pos, Quaternion rot, Vector3 scale)
		: this(card, pos, rot)
	{
		Scale = scale;
	}

	public CardLayoutData(DuelScene_CDC card, Vector3 pos, Quaternion rot, Vector3 scale, bool isVisibleInLayout)
		: this(card, pos, rot, scale)
	{
		IsVisibleInLayout = isVisibleInLayout;
	}
}
