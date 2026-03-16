using System;

[Serializable]
public class SmartAnchorRelationship
{
	public enum RelationshipType
	{
		Informational,
		Affects_Left,
		Affects_Right,
		Affects_Top,
		Affects_Bottom
	}

	public AnchorPointType AnchorType;

	public RelationshipType Relationship;

	public float OverlapAffordance;
}
