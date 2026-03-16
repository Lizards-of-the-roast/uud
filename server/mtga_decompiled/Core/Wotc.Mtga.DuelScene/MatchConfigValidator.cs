using Wotc.Mtga.DuelScene.UI.DEBUG;

namespace Wotc.Mtga.DuelScene;

public static class MatchConfigValidator
{
	public static IMatchConfigValidator CreateFamiliarValidator(IBattlefieldDataProvider battlefieldDataProvider)
	{
		return new MatchConfigValidatorAggregate(new FamiliarConfigValidator(), CreateSharedValidators(battlefieldDataProvider));
	}

	public static IMatchConfigValidator CreatePlayerVsPlayerValidator(IBattlefieldDataProvider battlefieldDataProvider)
	{
		return new MatchConfigValidatorAggregate(new PlayerVsPlayerConfigValidator(), CreateSharedValidators(battlefieldDataProvider));
	}

	private static IMatchConfigValidator CreateSharedValidators(IBattlefieldDataProvider battlefieldDataProvider)
	{
		return new MatchConfigValidatorAggregate(new YouCountConfigValidator(), new GameTypetValidator(), new GameVariantValidator(), new CommanderDeckValidator(), new SymmetricTeamValidator(), new PlayerCountValidator(), new BattlefieldSelectionValidator(battlefieldDataProvider));
	}
}
