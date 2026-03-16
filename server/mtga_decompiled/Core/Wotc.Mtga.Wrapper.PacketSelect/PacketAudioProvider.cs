using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Wotc.Mtga.Wrapper.PacketSelect;

public class PacketAudioProvider : IPacketAudioProvider
{
	private readonly Dictionary<string, string> Translate = new Dictionary<string, string>
	{
		{ "Above_the_Clouds", "sfx_artifact_etb_glider" },
		{ "Angels", "sfx_c_type_etb_angel" },
		{ "Archaeology", "sfx_c_type_etb_construct" },
		{ "Basri", "vo_basriket_etb" },
		{ "Cats", "sfx_c_type_etb_cat" },
		{ "Chandra", "vo_chandra_torchofdefiance_etb" },
		{ "Devilish", "sfx_c_type_etb_devil" },
		{ "Dinosaur", "sfx_c_type_etb_dinosaur" },
		{ "Discarding", "sfx_spec_surveil_refraction" },
		{ "Doctor", "sfx_spec_zenith_flare_small_heal" },
		{ "Dogs", "sfx_c_type_etb_hound" },
		{ "Dragons", "sfx_c_type_etb_dragon" },
		{ "Elves", "sfx_c_type_etb_elf" },
		{ "Enchanted", "sfx_artifact_etb_inspiringstatuary" },
		{ "Feathered_Friends", "sfx_c_type_etb_bird" },
		{ "Garruk", "vo_garruk_etb" },
		{ "Goblins", "sfx_c_type_etb_goblin" },
		{ "Heavily_Armored", "sfx_artifact_etb_shiningarmor" },
		{ "Lands", "sfx_land_botanicalsanctum" },
		{ "Legion", "sfx_c_type_etb_soldier" },
		{ "Lightning", "sfx_spell_lightning_strike_hit" },
		{ "Liliana", "vo_liliana_etb" },
		{ "Milling", "sfx_spec_mill_stop" },
		{ "Minions", "sfx_c_type_etb_zombie" },
		{ "Minotaurs", "sfx_c_type_etb_minotaur" },
		{ "Phyrexian", "sfx_c_type_etb_horror" },
		{ "Pirates", "sfx_c_type_etb_pirate" },
		{ "Plus_One", "sfx_c_type_etb_hyrdra" },
		{ "Predatory", "sfx_c_type_etb_beast_beast" },
		{ "Rainbow", "sfx_artifact_etb_propheticprism" },
		{ "Reanimated", "sfx_spec_escape_back_from_grave" },
		{ "Rogues", "sfx_c_type_etb_rogue" },
		{ "Seismic", "sfx_c_type_etb_elemental_amplifire" },
		{ "Smashing", "sfx_instant_deal_damage_earth" },
		{ "Spellcasting", "sfx_spec_spell_flame_sweep" },
		{ "Spirits", "sfx_c_type_etb_spirit" },
		{ "Spooky", "sfx_c_type_etb_skeleton" },
		{ "Teferi", "vo_teferi_etb" },
		{ "Tree_hugging", "sfx_c_type_etb_treefolk" },
		{ "Under_the_Sea", "sfx_c_type_etb_merfolk" },
		{ "Unicorns", "sfx_c_type_etb_unicorn" },
		{ "Vampires", "sfx_c_type_etb_vampire" },
		{ "Walls", "sfx_c_type_etb_wall" },
		{ "Well_read", "sfx_artifact_etb_foliooffancies" },
		{ "Witchcraft", "sfx_artifact_etb_witchscauldron" },
		{ "Wizard", "sfx_c_type_etb_wizard" }
	};

	public string GetPacketAudio(string packetId)
	{
		string input = Regex.Replace(packetId, "JMP_M21_", "", RegexOptions.IgnoreCase);
		input = Regex.Replace(input, "_\\d", "", RegexOptions.IgnoreCase);
		if (Translate.TryGetValue(input, out var value))
		{
			return value;
		}
		return string.Empty;
	}
}
