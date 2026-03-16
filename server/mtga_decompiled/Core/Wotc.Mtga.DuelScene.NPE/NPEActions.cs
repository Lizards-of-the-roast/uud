using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.NPE;

public static class NPEActions
{
	public static Action Pass = new Action
	{
		ActionType = ActionType.Pass
	};

	public static Action PlayForest = new Action
	{
		ActionType = ActionType.Play,
		GrpId = 69447u
	};

	public static Action CastTreetopWarden = new Action
	{
		ActionType = ActionType.Cast,
		GrpId = 69119u
	};

	public static Action CastRumblingBaloth = new Action
	{
		ActionType = ActionType.Cast,
		GrpId = 68801u
	};

	public static Action CastFeralRoar = new Action
	{
		ActionType = ActionType.Cast,
		GrpId = 68771u
	};

	public static Action PlayMountain = new Action
	{
		ActionType = ActionType.Play,
		GrpId = 69446u
	};

	public static Action CastRagingGoblin = new Action
	{
		ActionType = ActionType.Cast,
		GrpId = 68784u
	};

	public static Action CastGoblinBruiser = new Action
	{
		ActionType = ActionType.Cast,
		GrpId = 68773u
	};

	public static Action CastGoblinGrenade = new Action
	{
		ActionType = ActionType.Cast,
		GrpId = 68802u
	};

	public static Action CastGoblinGangLeader = new Action
	{
		ActionType = ActionType.Cast,
		GrpId = 69117u
	};

	public static Action CastOgrePainbringer = new Action
	{
		ActionType = ActionType.Cast,
		GrpId = 69118u
	};

	public static IEnumerable<Action> GetGame1Actions()
	{
		yield return PlayForest;
		yield return PlayForest;
		yield return Pass;
		yield return Pass;
		yield return PlayForest;
		yield return CastTreetopWarden;
		yield return PlayForest;
		yield return CastRumblingBaloth;
		yield return CastFeralRoar;
		yield return Pass;
		yield return Pass;
		yield return CastTreetopWarden;
		yield return CastTreetopWarden;
	}

	public static IEnumerable<Action> GetGame2Actions()
	{
		yield return PlayMountain;
		yield return CastRagingGoblin;
		yield return PlayMountain;
		yield return CastRagingGoblin;
		yield return CastRagingGoblin;
		yield return PlayMountain;
		yield return Pass;
		yield return CastGoblinBruiser;
		yield return PlayMountain;
		yield return Pass;
		yield return CastGoblinGrenade;
		yield return Pass;
		yield return CastGoblinGangLeader;
		yield return PlayMountain;
		yield return Pass;
		yield return CastOgrePainbringer;
		yield return Pass;
		yield return Pass;
		yield return CastRagingGoblin;
	}
}
