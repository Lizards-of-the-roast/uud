using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[Serializable]
public sealed class WwiseEvents
{
	public enum EAudioType
	{
		Generic,
		Interaction,
		ETB
	}

	public static readonly WwiseEvents INVALID = new WwiseEvents("INVALID", 0);

	public static readonly WwiseEvents pet_raptor_footstep = new WwiseEvents("pet_raptor_foot", 0, EAudioType.Interaction);

	public static readonly WwiseEvents pet_raptor_anim_dance = new WwiseEvents("pet_raptor_anim_dance", 0, EAudioType.Interaction);

	public static readonly WwiseEvents pet_raptor_anim_idle = new WwiseEvents("pet_raptor_anim_idle", 0, EAudioType.Interaction);

	public static readonly WwiseEvents pet_raptor_anim_investigate = new WwiseEvents("pet_raptor_anim_investigate", 0, EAudioType.Interaction);

	public static readonly WwiseEvents pet_raptor_anim_react = new WwiseEvents("pet_raptor_anim_react", 0, EAudioType.Interaction);

	public static readonly WwiseEvents pet_raptor_anim_scream = new WwiseEvents("pet_raptor_anim_scream", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sparky_global_stop = new WwiseEvents("sfx_npe_sparky_select_highlight_stop", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_01 = new WwiseEvents("vo_sparky_sep_01", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_02 = new WwiseEvents("vo_sparky_sep_02", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_03 = new WwiseEvents("vo_sparky_sep_03", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_04 = new WwiseEvents("vo_sparky_sep_04", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_05 = new WwiseEvents("vo_sparky_sep_05", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_06 = new WwiseEvents("vo_sparky_sep_06", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_07 = new WwiseEvents("vo_sparky_sep_07", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_10 = new WwiseEvents("vo_sparky_sep_10", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_11 = new WwiseEvents("vo_sparky_sep_11", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_12 = new WwiseEvents("vo_sparky_sep_12", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_13 = new WwiseEvents("vo_sparky_sep_13", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_14 = new WwiseEvents("vo_sparky_sep_14", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_16 = new WwiseEvents("vo_sparky_sep_16", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_17 = new WwiseEvents("vo_sparky_sep_17", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_18 = new WwiseEvents("vo_sparky_sep_18", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_19 = new WwiseEvents("vo_sparky_sep_19", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_21 = new WwiseEvents("vo_sparky_sep_21", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_23 = new WwiseEvents("vo_sparky_sep_23", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_24 = new WwiseEvents("vo_sparky_sep_24", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_26 = new WwiseEvents("vo_sparky_sep_26", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_27 = new WwiseEvents("vo_sparky_sep_27", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_30 = new WwiseEvents("vo_sparky_sep_30", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_08 = new WwiseEvents("vo_elf_sep_08", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_09 = new WwiseEvents("vo_elf_sep_09", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_09_alt_a = new WwiseEvents("vo_elf_sep_09_alt_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_09_alt_b = new WwiseEvents("vo_elf_sep_09_alt_b", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_09_alt_c = new WwiseEvents("vo_elf_sep_09_alt_c", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_15 = new WwiseEvents("vo_elf_sep_15", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_15_alt = new WwiseEvents("vo_elf_sep_15_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_20 = new WwiseEvents("vo_elf_sep_20", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_22 = new WwiseEvents("vo_elf_sep_22", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_25 = new WwiseEvents("vo_elf_sep_25", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_28 = new WwiseEvents("vo_elf_sep_28", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_29 = new WwiseEvents("vo_elf_sep_29", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_sep_31 = new WwiseEvents("vo_elf_sep_31", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_npe_intro_blackspace = new WwiseEvents("sfx_npe_intro_blackspace", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_010 = new WwiseEvents("vo_sparky_010", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_010_a = new WwiseEvents("vo_sparky_010_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_011_a_v1 = new WwiseEvents("vo_sparky_011_a_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_011_v1 = new WwiseEvents("vo_sparky_011_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_012 = new WwiseEvents("vo_sparky_012", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_013 = new WwiseEvents("vo_sparky_013", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_013_a = new WwiseEvents("vo_sparky_013_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_014 = new WwiseEvents("vo_sparky_014", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_014_a = new WwiseEvents("vo_sparky_014_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_015 = new WwiseEvents("vo_sparky_015", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_016 = new WwiseEvents("vo_sparky_016", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_016_a = new WwiseEvents("vo_sparky_016_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_017_v1 = new WwiseEvents("vo_sparky_017_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_019_a = new WwiseEvents("vo_sparky_019_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_019_v1 = new WwiseEvents("vo_sparky_019_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_021_a_v1 = new WwiseEvents("vo_sparky_021_a_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_021_v1 = new WwiseEvents("vo_sparky_021_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_023 = new WwiseEvents("vo_sparky_023", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_024 = new WwiseEvents("vo_sparky_024", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_024_a = new WwiseEvents("vo_sparky_024_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_024_b_v1 = new WwiseEvents("vo_sparky_024_b_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_026 = new WwiseEvents("vo_sparky_026", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_026_a = new WwiseEvents("vo_sparky_026_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_027 = new WwiseEvents("vo_sparky_027", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_030 = new WwiseEvents("vo_sparky_030", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_018 = new WwiseEvents("vo_elf_018", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_018_a = new WwiseEvents("vo_elf_018_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_018_b = new WwiseEvents("vo_elf_018_b", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_020 = new WwiseEvents("vo_elf_020", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_020_a = new WwiseEvents("vo_elf_020_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_020_a1 = new WwiseEvents("vo_elf_020_a1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_022 = new WwiseEvents("vo_elf_022", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_025 = new WwiseEvents("vo_elf_025", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_028 = new WwiseEvents("vo_elf_028", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_028_a = new WwiseEvents("vo_elf_028_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_029_v1 = new WwiseEvents("vo_elf_029_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_elf_031 = new WwiseEvents("vo_elf_031", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_37 = new WwiseEvents("vo_sparky_sep_37", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_40 = new WwiseEvents("vo_sparky_sep_40", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_41 = new WwiseEvents("vo_sparky_sep_41", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_44 = new WwiseEvents("vo_sparky_sep_44", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_46 = new WwiseEvents("vo_sparky_sep_46", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_47 = new WwiseEvents("vo_sparky_sep_47", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_54 = new WwiseEvents("vo_sparky_sep_54", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_58 = new WwiseEvents("vo_sparky_sep_58", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_36 = new WwiseEvents("vo_goblin_sep_36", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_38 = new WwiseEvents("vo_goblin_sep_38", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_42 = new WwiseEvents("vo_goblin_sep_42", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_45 = new WwiseEvents("vo_goblin_sep_45", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_48 = new WwiseEvents("vo_goblin_sep_48", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_49_alt_v2 = new WwiseEvents("vo_goblin_sep_49_alt_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_49_v2 = new WwiseEvents("vo_goblin_sep_49_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_51 = new WwiseEvents("vo_goblin_sep_51", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_52 = new WwiseEvents("vo_goblin_sep_52", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_53 = new WwiseEvents("vo_goblin_sep_53", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_55 = new WwiseEvents("vo_goblin_sep_55", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_56 = new WwiseEvents("vo_goblin_sep_56", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_59_alt_v2 = new WwiseEvents("vo_goblin_sep_59_alt_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_59_v2 = new WwiseEvents("vo_goblin_sep_59_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_60 = new WwiseEvents("vo_goblin_sep_60", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_sep_61_v4 = new WwiseEvents("vo_goblin_sep_61_v4", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_gobwaitwha_01 = new WwiseEvents("vo_gobwaitwha_01", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_036 = new WwiseEvents("vo_sparky_036", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_037 = new WwiseEvents("vo_sparky_037", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_037_a = new WwiseEvents("vo_sparky_037_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_040 = new WwiseEvents("vo_sparky_040", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_042 = new WwiseEvents("vo_sparky_042", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_049_b = new WwiseEvents("vo_sparky_049_b", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_049_v1 = new WwiseEvents("vo_sparky_049_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_052 = new WwiseEvents("vo_sparky_052", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_058_v1 = new WwiseEvents("vo_sparky_058_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_061 = new WwiseEvents("vo_sparky_061", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_061_c_v1 = new WwiseEvents("vo_sparky_061_c_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_32 = new WwiseEvents("vo_goblin_32", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_33_v1 = new WwiseEvents("vo_goblin_33_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_33_v2 = new WwiseEvents("vo_goblin_33_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_34 = new WwiseEvents("vo_goblin_34", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_35_a = new WwiseEvents("vo_goblin_35_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_35_b = new WwiseEvents("vo_goblin_35_b", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_35_c = new WwiseEvents("vo_goblin_35_c", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_35_d = new WwiseEvents("vo_goblin_35_d", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_35_e = new WwiseEvents("vo_goblin_35_e", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_38_v1 = new WwiseEvents("vo_goblin_38_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_38_v2 = new WwiseEvents("vo_goblin_38_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_39_a = new WwiseEvents("vo_goblin_39_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_39_b = new WwiseEvents("vo_goblin_39_b", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_41_v1 = new WwiseEvents("vo_goblin_41_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_41_v2 = new WwiseEvents("vo_goblin_41_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_43 = new WwiseEvents("vo_goblin_43", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_44 = new WwiseEvents("vo_goblin_44", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_46_ouch = new WwiseEvents("vo_goblin_46_ouch", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_46_ouch_single = new WwiseEvents("vo_goblin_46_ouch_single", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_47 = new WwiseEvents("vo_goblin_47", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_48 = new WwiseEvents("vo_goblin_48", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_50 = new WwiseEvents("vo_goblin_50", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_51_v1 = new WwiseEvents("vo_goblin_51_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_55_v1 = new WwiseEvents("vo_goblin_55_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_56_v1 = new WwiseEvents("vo_goblin_56_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_57 = new WwiseEvents("vo_goblin_57", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_59 = new WwiseEvents("vo_goblin_59", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_60 = new WwiseEvents("vo_goblin_60", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_62_d = new WwiseEvents("vo_goblin_62_d", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_62_v1 = new WwiseEvents("vo_goblin_62_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_goblin_whimper = new WwiseEvents("vo_goblin_whimper", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_64 = new WwiseEvents("vo_sparky_sep_64", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_65 = new WwiseEvents("vo_sparky_sep_65", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_69 = new WwiseEvents("vo_sparky_sep_69", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_71 = new WwiseEvents("vo_sparky_sep_71", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_74 = new WwiseEvents("vo_sparky_sep_74", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_75 = new WwiseEvents("vo_sparky_sep_75", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_78 = new WwiseEvents("vo_sparky_sep_78", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_80 = new WwiseEvents("vo_sparky_sep_80", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_82 = new WwiseEvents("vo_sparky_sep_82", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_84 = new WwiseEvents("vo_sparky_sep_84", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_85 = new WwiseEvents("vo_sparky_sep_85", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_86 = new WwiseEvents("vo_sparky_sep_86", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_89 = new WwiseEvents("vo_sparky_sep_89", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_66 = new WwiseEvents("vo_merfolk_sep_66", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_67 = new WwiseEvents("vo_merfolk_sep_67", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_68 = new WwiseEvents("vo_merfolk_sep_68", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_70 = new WwiseEvents("vo_merfolk_sep_70", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_72 = new WwiseEvents("vo_merfolk_sep_72", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_73 = new WwiseEvents("vo_merfolk_sep_73", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_76 = new WwiseEvents("vo_merfolk_sep_76", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_77_v2 = new WwiseEvents("vo_merfolk_sep_77_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_79 = new WwiseEvents("vo_merfolk_sep_79", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_80 = new WwiseEvents("vo_merfolk_sep_80", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_81 = new WwiseEvents("vo_merfolk_sep_81", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_83 = new WwiseEvents("vo_merfolk_sep_83", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_87 = new WwiseEvents("vo_merfolk_sep_87", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_88 = new WwiseEvents("vo_merfolk_sep_88", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_90 = new WwiseEvents("vo_merfolk_sep_90", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_sep_90_alt = new WwiseEvents("vo_merfolk_sep_90_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_065 = new WwiseEvents("vo_sparky_065", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_067_v1 = new WwiseEvents("vo_sparky_067_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_069 = new WwiseEvents("vo_sparky_069", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_069_a_v1 = new WwiseEvents("vo_sparky_069_a_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_072_v1 = new WwiseEvents("vo_sparky_072_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_076_v1 = new WwiseEvents("vo_sparky_076_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_079 = new WwiseEvents("vo_sparky_079", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_080_a_v1 = new WwiseEvents("vo_sparky_080_a_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_080_v1 = new WwiseEvents("vo_sparky_080_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_63 = new WwiseEvents("vo_merfolk_63", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_64 = new WwiseEvents("vo_merfolk_64", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_66 = new WwiseEvents("vo_merfolk_66", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_68 = new WwiseEvents("vo_merfolk_68", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_70 = new WwiseEvents("vo_merfolk_70", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_70_a_v1 = new WwiseEvents("vo_merfolk_70_a_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_71 = new WwiseEvents("vo_merfolk_71", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_73_a_v1 = new WwiseEvents("vo_merfolk_73_a_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_73_v1 = new WwiseEvents("vo_merfolk_73_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_74 = new WwiseEvents("vo_merfolk_74", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_75_a = new WwiseEvents("vo_merfolk_75_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_75_v1 = new WwiseEvents("vo_merfolk_75_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_77 = new WwiseEvents("vo_merfolk_77", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_78_v1 = new WwiseEvents("vo_merfolk_78_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_81 = new WwiseEvents("vo_merfolk_81", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_82 = new WwiseEvents("vo_merfolk_82", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_merfolk_82_a = new WwiseEvents("vo_merfolk_82_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_94 = new WwiseEvents("vo_sparky_sep_94", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_99 = new WwiseEvents("vo_sparky_sep_99", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_102 = new WwiseEvents("vo_sparky_sep_102", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_105 = new WwiseEvents("vo_sparky_sep_105", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_106 = new WwiseEvents("vo_sparky_sep_106", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_107 = new WwiseEvents("vo_sparky_sep_107", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_111 = new WwiseEvents("vo_sparky_sep_111", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_113 = new WwiseEvents("vo_sparky_sep_113", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_117 = new WwiseEvents("vo_sparky_sep_117", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_119 = new WwiseEvents("vo_sparky_sep_119", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_95 = new WwiseEvents("vo_assassin_npe_95", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_95_alt_a = new WwiseEvents("vo_assassin_npe_95_alt_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_95_alt_b = new WwiseEvents("vo_assassin_npe_95_alt_b", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_96 = new WwiseEvents("vo_assassin_npe_96", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_96_alt = new WwiseEvents("vo_assassin_npe_96_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_97 = new WwiseEvents("vo_assassin_npe_97", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_98 = new WwiseEvents("vo_assassin_npe_98", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_100 = new WwiseEvents("vo_assassin_npe_100", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_100_alt = new WwiseEvents("vo_assassin_npe_100_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_101 = new WwiseEvents("vo_assassin_npe_101", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_103 = new WwiseEvents("vo_assassin_npe_103", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_104 = new WwiseEvents("vo_assassin_npe_104", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_108 = new WwiseEvents("vo_assassin_npe_108", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_108_alt = new WwiseEvents("vo_assassin_npe_108_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_109 = new WwiseEvents("vo_assassin_npe_109", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_110 = new WwiseEvents("vo_assassin_npe_110", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_112 = new WwiseEvents("vo_assassin_npe_112", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_114 = new WwiseEvents("vo_assassin_npe_114", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_115 = new WwiseEvents("vo_assassin_npe_115", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_115_alt = new WwiseEvents("vo_assassin_npe_115_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_116 = new WwiseEvents("vo_assassin_npe_116", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_116_alt = new WwiseEvents("vo_assassin_npe_116_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_118 = new WwiseEvents("vo_assassin_npe_118", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_118_alt = new WwiseEvents("vo_assassin_npe_118_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_120 = new WwiseEvents("vo_assassin_npe_120", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_npe_121 = new WwiseEvents("vo_assassin_npe_121", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_085_v1 = new WwiseEvents("vo_sparky_085_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_088 = new WwiseEvents("vo_sparky_088", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_093_v1 = new WwiseEvents("vo_sparky_093_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_094_v1 = new WwiseEvents("vo_sparky_094_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_095_v1 = new WwiseEvents("vo_sparky_095_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_099_v1 = new WwiseEvents("vo_sparky_099_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_104 = new WwiseEvents("vo_sparky_104", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_106_v1 = new WwiseEvents("vo_sparky_106_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_107 = new WwiseEvents("vo_sparky_107", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_083 = new WwiseEvents("vo_assassin_083", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_083_a = new WwiseEvents("vo_assassin_083_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_084 = new WwiseEvents("vo_assassin_084", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_086 = new WwiseEvents("vo_assassin_086", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_087 = new WwiseEvents("vo_assassin_087", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_087_a = new WwiseEvents("vo_assassin_087_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_089 = new WwiseEvents("vo_assassin_089", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_090 = new WwiseEvents("vo_assassin_090", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_090_a_v1 = new WwiseEvents("vo_assassin_090_a_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_091 = new WwiseEvents("vo_assassin_091", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_092 = new WwiseEvents("vo_assassin_092", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_092_a = new WwiseEvents("vo_assassin_092_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_096_a = new WwiseEvents("vo_assassin_096_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_096_v1 = new WwiseEvents("vo_assassin_096_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_097 = new WwiseEvents("vo_assassin_097", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_097_a = new WwiseEvents("vo_assassin_097_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_098 = new WwiseEvents("vo_assassin_098", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_100 = new WwiseEvents("vo_assassin_100", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_100_a = new WwiseEvents("vo_assassin_100_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_101 = new WwiseEvents("vo_assassin_101", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_102 = new WwiseEvents("vo_assassin_102", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_103 = new WwiseEvents("vo_assassin_103", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_105 = new WwiseEvents("vo_assassin_105", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_105_a = new WwiseEvents("vo_assassin_105_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_108 = new WwiseEvents("vo_assassin_108", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_assassin_109 = new WwiseEvents("vo_assassin_109", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_132 = new WwiseEvents("vo_sparky_sep_132", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_132_alt_a = new WwiseEvents("vo_sparky_sep_132_alt_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_132_alt_b = new WwiseEvents("vo_sparky_sep_132_alt_b", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_134 = new WwiseEvents("vo_sparky_sep_134", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_135 = new WwiseEvents("vo_sparky_sep_135", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_135_alt_a = new WwiseEvents("vo_sparky_sep_135_alt_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_135_alt_b = new WwiseEvents("vo_sparky_sep_135_alt_b", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_136 = new WwiseEvents("vo_sparky_sep_136", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_142 = new WwiseEvents("vo_sparky_sep_142", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_145 = new WwiseEvents("vo_sparky_sep_145", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_147 = new WwiseEvents("vo_sparky_sep_147", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_147_alt = new WwiseEvents("vo_sparky_sep_147_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_149 = new WwiseEvents("vo_sparky_sep_149", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_154 = new WwiseEvents("vo_sparky_sep_154", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_154_alt = new WwiseEvents("vo_sparky_sep_154_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_159 = new WwiseEvents("vo_sparky_sep_159", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_159_alt = new WwiseEvents("vo_sparky_sep_159_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_sep_161 = new WwiseEvents("vo_sparky_sep_161", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_125_v3 = new WwiseEvents("vo_nicolbolas_sep_125_v3", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_126 = new WwiseEvents("vo_nicolbolas_sep_126", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_129 = new WwiseEvents("vo_nicolbolas_sep_129", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_130 = new WwiseEvents("vo_nicolbolas_sep_130", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_131_v2 = new WwiseEvents("vo_nicolbolas_sep_131_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_133 = new WwiseEvents("vo_nicolbolas_sep_133", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_133_alt = new WwiseEvents("vo_nicolbolas_sep_133_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_137 = new WwiseEvents("vo_nicolbolas_sep_137", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_137_alt = new WwiseEvents("vo_nicolbolas_sep_137_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_138 = new WwiseEvents("vo_nicolbolas_sep_138", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_138_alt = new WwiseEvents("vo_nicolbolas_sep_138_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_139 = new WwiseEvents("vo_nicolbolas_sep_139", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_139_alt = new WwiseEvents("vo_nicolbolas_sep_139_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_140 = new WwiseEvents("vo_nicolbolas_sep_140", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_140_alt_v3 = new WwiseEvents("vo_nicolbolas_sep_140_alt_v3", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_141 = new WwiseEvents("vo_nicolbolas_sep_141", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_141_alt = new WwiseEvents("vo_nicolbolas_sep_141_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_143 = new WwiseEvents("vo_nicolbolas_sep_143", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_143_alt_v3 = new WwiseEvents("vo_nicolbolas_sep_143_alt_v3", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_144 = new WwiseEvents("vo_nicolbolas_sep_144", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_146 = new WwiseEvents("vo_nicolbolas_sep_146", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_148_v2 = new WwiseEvents("vo_nicolbolas_sep_148_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_150 = new WwiseEvents("vo_nicolbolas_sep_150", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_151_v2 = new WwiseEvents("vo_nicolbolas_sep_151_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_152 = new WwiseEvents("vo_nicolbolas_sep_152", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_153 = new WwiseEvents("vo_nicolbolas_sep_153", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_155_v2 = new WwiseEvents("vo_nicolbolas_sep_155_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_156_v2 = new WwiseEvents("vo_nicolbolas_sep_156_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_157_v2 = new WwiseEvents("vo_nicolbolas_sep_157_v2", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_158 = new WwiseEvents("vo_nicolbolas_sep_158", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_160 = new WwiseEvents("vo_nicolbolas_sep_160", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_160_alt = new WwiseEvents("vo_nicolbolas_sep_160_alt", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_162 = new WwiseEvents("vo_nicolbolas_sep_162", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_163 = new WwiseEvents("vo_nicolbolas_sep_163", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_164 = new WwiseEvents("vo_nicolbolas_sep_164", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_165 = new WwiseEvents("vo_nicolbolas_sep_165", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_sep_166 = new WwiseEvents("vo_nicolbolas_sep_166", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_111 = new WwiseEvents("vo_sparky_111", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_112_v1 = new WwiseEvents("vo_sparky_112_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_123 = new WwiseEvents("vo_sparky_123", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_124_a = new WwiseEvents("vo_sparky_124_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_124_b = new WwiseEvents("vo_sparky_124_b", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_130 = new WwiseEvents("vo_sparky_130", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_110_a_v1 = new WwiseEvents("vo_nicolbolas_110_a_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_110_v1 = new WwiseEvents("vo_nicolbolas_110_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_113 = new WwiseEvents("vo_nicolbolas_113", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_113_a = new WwiseEvents("vo_nicolbolas_113_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_114 = new WwiseEvents("vo_nicolbolas_114", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_114_c = new WwiseEvents("vo_nicolbolas_114_c", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_115 = new WwiseEvents("vo_nicolbolas_115", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_115_d = new WwiseEvents("vo_nicolbolas_115_d", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_116 = new WwiseEvents("vo_nicolbolas_116", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_117 = new WwiseEvents("vo_nicolbolas_117", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_117_e = new WwiseEvents("vo_nicolbolas_117_e", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_118 = new WwiseEvents("vo_nicolbolas_118", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_118_v1 = new WwiseEvents("vo_nicolbolas_118_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_119_huh = new WwiseEvents("vo_nicolbolas_119_huh", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_120 = new WwiseEvents("vo_nicolbolas_120", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_121 = new WwiseEvents("vo_nicolbolas_121", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_122_v1 = new WwiseEvents("vo_nicolbolas_122_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_125 = new WwiseEvents("vo_nicolbolas_125", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_125_g = new WwiseEvents("vo_nicolbolas_125_g", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_126 = new WwiseEvents("vo_nicolbolas_126", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_127_v1 = new WwiseEvents("vo_nicolbolas_127_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_128 = new WwiseEvents("vo_nicolbolas_128", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_129 = new WwiseEvents("vo_nicolbolas_129", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_129_h = new WwiseEvents("vo_nicolbolas_129_h", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_131 = new WwiseEvents("vo_nicolbolas_131", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_132 = new WwiseEvents("vo_nicolbolas_132", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_132_i = new WwiseEvents("vo_nicolbolas_132_i", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_133 = new WwiseEvents("vo_nicolbolas_133", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_134 = new WwiseEvents("vo_nicolbolas_134", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_134_j = new WwiseEvents("vo_nicolbolas_134_j", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_135 = new WwiseEvents("vo_nicolbolas_135", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_135_k = new WwiseEvents("vo_nicolbolas_135_k", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_119_what = new WwiseEvents("vo_nicolbolas_119_what", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_001_v1 = new WwiseEvents("vo_sparky_001_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_002_v1 = new WwiseEvents("vo_sparky_002_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_003_v1 = new WwiseEvents("vo_sparky_003_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_004_v1 = new WwiseEvents("vo_sparky_004_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_005_v1 = new WwiseEvents("vo_sparky_005_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_006_v1 = new WwiseEvents("vo_sparky_006_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_007_v1 = new WwiseEvents("vo_sparky_007_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_008_v1 = new WwiseEvents("vo_sparky_008_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_009_v1 = new WwiseEvents("vo_sparky_009_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_136_a = new WwiseEvents("vo_sparky_136_a", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_136_v1 = new WwiseEvents("vo_sparky_136_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_136_v3 = new WwiseEvents("vo_sparky_136_v3", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_138_v1 = new WwiseEvents("vo_sparky_138_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_140 = new WwiseEvents("vo_sparky_140", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_141_a_v1 = new WwiseEvents("vo_sparky_141_a_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_141_v1 = new WwiseEvents("vo_sparky_141_v1", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_137 = new WwiseEvents("vo_nicolbolas_137", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_nicolbolas_139 = new WwiseEvents("vo_nicolbolas_139", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_aah = new WwiseEvents("vo_sparky_aah", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_argh = new WwiseEvents("vo_sparky_argh", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_awe = new WwiseEvents("vo_sparky_awe", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_giggle = new WwiseEvents("vo_sparky_giggle", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_ha = new WwiseEvents("vo_sparky_ha", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_hey = new WwiseEvents("vo_sparky_hey", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_hmmm = new WwiseEvents("vo_sparky_hmmm", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_huh = new WwiseEvents("vo_sparky_huh", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_waiting = new WwiseEvents("vo_sparky_waiting", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_whimper = new WwiseEvents("vo_sparky_whimper", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_sparky_woah = new WwiseEvents("vo_sparky_woah", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_001 = new WwiseEvents("vo_g1_sparky_001", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_elf_003 = new WwiseEvents("vo_g1_elf_003", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_002 = new WwiseEvents("vo_g1_sparky_002", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_elf_001 = new WwiseEvents("vo_g1_elf_001", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_elf_002 = new WwiseEvents("vo_g1_elf_002", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_elf_007 = new WwiseEvents("vo_g1_elf_007", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_elf_006 = new WwiseEvents("vo_g1_elf_006", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_009 = new WwiseEvents("vo_g1_sparky_009", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_010 = new WwiseEvents("vo_g1_sparky_010", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_011 = new WwiseEvents("vo_g1_sparky_011", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_003 = new WwiseEvents("vo_g1_sparky_003", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_004 = new WwiseEvents("vo_g1_sparky_004", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_007 = new WwiseEvents("vo_g1_sparky_007", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_008 = new WwiseEvents("vo_g1_sparky_008", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_005 = new WwiseEvents("vo_g1_sparky_005", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_elf_005 = new WwiseEvents("vo_g1_elf_005", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_sparky_006 = new WwiseEvents("vo_g1_sparky_006", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_elf_004 = new WwiseEvents("vo_g1_elf_004", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g1_elf_008 = new WwiseEvents("vo_g1_elf_008", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_004 = new WwiseEvents("vo_g2_goblin_004", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_sparky_001 = new WwiseEvents("vo_g2_sparky_001", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_sparky_002 = new WwiseEvents("vo_g2_sparky_002", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblincrowds_jeer = new WwiseEvents("vo_g2_goblincrowds_jeer", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_sparky_005 = new WwiseEvents("vo_g2_sparky_005", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_pain_001 = new WwiseEvents("vo_g2_pain_001", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_009 = new WwiseEvents("vo_g2_goblin_009", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_008 = new WwiseEvents("vo_g2_goblin_008", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_010 = new WwiseEvents("vo_g2_goblin_010", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_sparky_003 = new WwiseEvents("vo_g2_sparky_003", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_sparky_004 = new WwiseEvents("vo_g2_sparky_004", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_003 = new WwiseEvents("vo_g2_goblin_003", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_001 = new WwiseEvents("vo_g2_goblin_001", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_002 = new WwiseEvents("vo_g2_goblin_002", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_012 = new WwiseEvents("vo_g2_goblin_012", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblincrowds_huh = new WwiseEvents("vo_g2_goblincrowds_huh", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_005 = new WwiseEvents("vo_g2_goblin_005", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_011 = new WwiseEvents("vo_g2_goblin_011", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_013 = new WwiseEvents("vo_g2_goblin_013", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_014 = new WwiseEvents("vo_g2_goblin_014", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_015 = new WwiseEvents("vo_g2_goblin_015", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_016 = new WwiseEvents("vo_g2_goblin_016", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_017 = new WwiseEvents("vo_g2_goblin_017", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_006 = new WwiseEvents("vo_g2_goblin_006", 0, EAudioType.Interaction);

	public static readonly WwiseEvents vo_g2_goblin_007 = new WwiseEvents("vo_g2_goblin_007", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_place_whoosh = new WwiseEvents("sfx_basicloc_place_whoosh", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_place_flying_whoosh = new WwiseEvents("sfx_basicloc_place_flying_whoosh", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_hightlight_on_selection = new WwiseEvents("sfx_basicloc_hightlight_on_selection", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_place_impact = new WwiseEvents("sfx_basicloc_place_impact", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_place_impact_flying = new WwiseEvents("sfx_basicloc_place_impact_flying", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_single_card_examine_big_in = new WwiseEvents("sfx_basicloc_single_card_examine_big_in", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_single_card_examine_big_out = new WwiseEvents("sfx_basicloc_single_card_examine_big_out", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_single_card_examine_out = new WwiseEvents("sfx_basicloc_single_card_examine_out", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_single_card_examine_in = new WwiseEvents("sfx_basicloc_single_card_examine_in", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_draw_card_1st = new WwiseEvents("sfx_basicloc_draw_card_1st", 0);

	public static readonly WwiseEvents sfx_basicloc_draw_card_2nd = new WwiseEvents("sfx_basicloc_draw_card_2nd", 0);

	public static readonly WwiseEvents sfx_basicloc_draw_card_multi = new WwiseEvents("sfx_basicloc_draw_card_multi", 0);

	public static readonly WwiseEvents sfx_basicloc_discard = new WwiseEvents("sfx_basicloc_draw_card_2nd", 0);

	public static readonly WwiseEvents sfx_ui_main_quest_claim_reward = new WwiseEvents("sfx_ui_main_quest_claim_reward", 0);

	public static readonly WwiseEvents sfx_ui_eventblade_event_select = new WwiseEvents("sfx_ui_generic_click", 0);

	public static readonly WwiseEvents sfx_ui_main_quest_progress_poof = new WwiseEvents("sfx_ui_main_quest_progress_poof", 0);

	public static readonly WwiseEvents sfx_ui_main_quest_main_bar_stop = new WwiseEvents("sfx_ui_main_quest_main_bar_stop", 0);

	public static readonly WwiseEvents sfx_ui_main_quest_progress_complete = new WwiseEvents("sfx_ui_main_quest_progress_complete", 0);

	public static readonly WwiseEvents sfx_ui_main_quest_progress_dismiss = new WwiseEvents("sfx_ui_main_quest_progress_dismiss", 0);

	public static readonly WwiseEvents sfx_ui_main_quest_main_bar_start = new WwiseEvents("sfx_ui_main_quest_main_bar_start", 0);

	public static readonly WwiseEvents sfx_ui_main_quest_rollover_a = new WwiseEvents("sfx_ui_main_quest_rollover_a", 0);

	public static readonly WwiseEvents sfx_ui_main_quest_rollover_b = new WwiseEvents("sfx_ui_main_quest_rollover_b", 0);

	public static readonly WwiseEvents sfx_ui_main_quest_appear = new WwiseEvents("sfx_ui_main_quest_apear", 0);

	public static readonly WwiseEvents sfx_ui_main_play_match_start = new WwiseEvents("sfx_ui_main_play_match_start", 0);

	public static readonly WwiseEvents sfx_ui_main_play_shelf_close = new WwiseEvents("sfx_ui_main_play_shelf_close", 0);

	public static readonly WwiseEvents sfx_ui_main_play_shelf_open = new WwiseEvents("sfx_ui_main_play_shelf_open", 0);

	public static readonly WwiseEvents sfx_ui_wildcard_redeemdialogue_off = new WwiseEvents("sfx_ui_wildcard_redeemdialogue_off", 0);

	public static readonly WwiseEvents sfx_ui_wildcard_redeemdialogue_on = new WwiseEvents("sfx_ui_wildcard_redeemdialogue_on", 0);

	public static readonly WwiseEvents sfx_ui_wildcard_transform = new WwiseEvents("sfx_ui_wildcard_transform", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_card_tap = new WwiseEvents("sfx_ui_main_rewards_card_tap", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_gem_tap = new WwiseEvents("sfx_ui_main_rewards_gem_tap", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_coins_flipout = new WwiseEvents("sfx_ui_main_rewards_coins_flipout", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_pack_flipout = new WwiseEvents("sfx_ui_main_rewards_pack_flipout", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_card_flipout = new WwiseEvents("sfx_ui_main_rewards_card_flipout", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_gem_flipout = new WwiseEvents("sfx_ui_main_rewards_gem_flipout", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_pack_tap = new WwiseEvents("sfx_ui_main_rewards_pack_tap", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_coins_tap = new WwiseEvents("sfx_ui_main_rewards_coins_tap", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_wild_flipout = new WwiseEvents("sfx_ui_main_rewards_wild_flipout", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_deck_tap = new WwiseEvents("sfx_ui_main_deck_edit", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_deck_flipout = new WwiseEvents("sfx_ui_main_deck_open", 0);

	public static readonly WwiseEvents sfx_ui_main_rewards_rollover_deck = new WwiseEvents("sfx_ui_main_rollover_deck", 0);

	public static readonly WwiseEvents sfx_basicloc_pull_card = new WwiseEvents("sfx_basicloc_pull_card", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_return_card = new WwiseEvents("sfx_basicloc_return_card", 0, EAudioType.Interaction);

	public static readonly WwiseEvents Card_Tap = new WwiseEvents("sfx_basicloc_generic_click", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_touch = new WwiseEvents("sfx_basicloc_touch", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_target_draw_start = new WwiseEvents("sfx_combat_target_draw_start", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_target_draw_stop = new WwiseEvents("sfx_combat_target_draw_stop", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_bounce = new WwiseEvents("sfx_combat_bounce", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_exert_active = new WwiseEvents("sfx_exert_active", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_exert_break = new WwiseEvents("sfx_exert_break", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_npe_target = new WwiseEvents("sfx_npe_target", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_npe_treefall = new WwiseEvents("sfx_npe_treefall", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_declareattackers = new WwiseEvents("sfx_combat_declareattackers", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_declare_no_attackers = new WwiseEvents("sfx_combat_undeclare_atk_def", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_declare_defenders = new WwiseEvents("sfx_combat_declare_defenders", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_declare_no_defenders = new WwiseEvents("sfx_combat_undeclare_atk_def", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_resolve = new WwiseEvents("sfx_ui_accept", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_UI_phasebutton_begincombat = new WwiseEvents("sfx_UI_phasebutton_begincombat", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_UI_phasebutton_endyourturn = new WwiseEvents("sfx_UI_phasebutton_endyourturn", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_UI_phasebutton_startturn = new WwiseEvents("sfx_UI_phasebutton_startturn", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_phasebutton_combatphase_attack = new WwiseEvents("sfx_ui_phasebutton_combatphase_attack", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_phasebutton_combatphase_damage = new WwiseEvents("sfx_ui_phasebutton_combatphase_damage", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_phasebutton_combatphase_block = new WwiseEvents("sfx_ui_phasebutton_combatphase_block", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_phasebutton_combatphase_end = new WwiseEvents("sfx_ui_phasebutton_combatphase_end", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_attacker_rise = new WwiseEvents("sfx_combat_attacker_rise", 0);

	public static readonly WwiseEvents sfx_combat_attacker_fly = new WwiseEvents("sfx_combat_attacker_fly", 0);

	public static readonly WwiseEvents sfx_combat_attacker_hit = new WwiseEvents("sfx_combat_attacker_hit", 0);

	public static readonly WwiseEvents sfx_combat_attacker_return = new WwiseEvents("sfx_combat_attacker_return", 0);

	public static readonly WwiseEvents sfx_spell_fire = new WwiseEvents("sfx_spell_fire", 0);

	public static readonly WwiseEvents sfx_spell_fly = new WwiseEvents("sfx_spell_fly", 0);

	public static readonly WwiseEvents sfx_spell_hit = new WwiseEvents("sfx_spell_hit", 0);

	public static readonly WwiseEvents sfx_basicloc_exile = new WwiseEvents("sfx_exile", 0);

	public static readonly WwiseEvents sfx_avatar_heal = new WwiseEvents("sfx_heal", 0);

	public static readonly WwiseEvents sfx_avatar_damage = new WwiseEvents("sfx_combat_player_hit", 0);

	public static readonly WwiseEvents sfx_combat_tap = new WwiseEvents("sfx_combat_tap", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_untap = new WwiseEvents("sfx_combat_untap", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_buff_attack = new WwiseEvents("sfx_combat_buff_attack", 0);

	public static readonly WwiseEvents sfx_combat_debuff_generic = new WwiseEvents("sfx_combat_debuff_generic", 0);

	public static readonly WwiseEvents sfx_combat_buff_haste = new WwiseEvents("sfx_combat_buff_haste", 0);

	public static readonly WwiseEvents sfx_combat_creature_deth_gen = new WwiseEvents("sfx_combat_creature_deth_gen", 0);

	public static readonly WwiseEvents sfx_combat_boardclear_fumigate = new WwiseEvents("sfx_combat_boardclear_fumigate", 0);

	public static readonly WwiseEvents sfx_combat_phasechange = new WwiseEvents("sfx_ui_phaseladder_start", 0);

	public static readonly WwiseEvents sfx_ui_phaseladder_flip = new WwiseEvents("sfx_ui_phaseladder_flip", 0);

	public static readonly WwiseEvents sfx_ui_phaseladder_start = new WwiseEvents("sfx_ui_phaseladder_start", 0);

	public static readonly WwiseEvents sfx_ui_phaseladder_click = new WwiseEvents("sfx_ui_phaseladder_click", 0);

	public static readonly WwiseEvents sfx_ui_main_whoosh_04 = new WwiseEvents("sfx_ui_main_whoosh_04", 0);

	public static readonly WwiseEvents sfx_ui_main_whoosh_03 = new WwiseEvents("sfx_ui_main_whoosh_03", 0);

	public static readonly WwiseEvents sfx_ui_main_whoosh_02 = new WwiseEvents("sfx_ui_main_whoosh_02", 0);

	public static readonly WwiseEvents sfx_ui_main_whoosh_01 = new WwiseEvents("sfx_ui_main_whoosh_01", 0);

	public static readonly WwiseEvents sfx_ui_yourturn = new WwiseEvents("sfx_ui_yourturn", 0);

	public static readonly WwiseEvents sfx_ui_opponentturn = new WwiseEvents("sfx_ui_opponentturn", 0);

	public static readonly WwiseEvents sfx_ui_gain_priority = new WwiseEvents("sfx_priority_on", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_lose_priority = new WwiseEvents("sfx_priority_off", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_manawheel_open = new WwiseEvents("sfx_combat_manawheel_open", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_mana_payout = new WwiseEvents("sfx_mana_payout", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_combat_manawheel_close = new WwiseEvents("sfx_combat_manawheel_close", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_mana_hit = new WwiseEvents("sfx_mana_hit", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_mana_swish = new WwiseEvents("sfx_mana_swish", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_energy_hit = new WwiseEvents("sfx_energy_hit", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_energy_swish = new WwiseEvents("sfx_energy_swish", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_home_open = new WwiseEvents("sfx_ui_home_open", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_store_open = new WwiseEvents("sfx_ui_main_store_open", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_packmanager_open = new WwiseEvents("sfx_ui_main_packmanager_open", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_deckmanager_open = new WwiseEvents("sfx_ui_main_deckmanager_open", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_pull_card = new WwiseEvents("sfx_ui_main_pull_card", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_return_card = new WwiseEvents("sfx_ui_main_return_card", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_deckbuilding_card_pickup = new WwiseEvents("sfx_ui_deckbuilding_card_pickup", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_deckbuilding_card_place = new WwiseEvents("sfx_ui_deckbuilding_card_place", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_rollover = new WwiseEvents("sfx_ui_main_rollover", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_rollover_big = new WwiseEvents("sfx_ui_main_rollover_large_buttons", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_card_rollover = new WwiseEvents("sfx_ui_main_rollover_card", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_card_cosmetic_editing_browser_rollover = new WwiseEvents("sfx_ui_main_card_cosmetic_editing_browser_rollover", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_page_state_change = new WwiseEvents("sfx_ui_main_page_state_change", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_deck_page_turn = new WwiseEvents("sfx_ui_main_deck_page_turn", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_deck_select = new WwiseEvents("sfx_ui_main_deck_edit", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_deck_open = new WwiseEvents("sfx_ui_main_deck_open", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_deck_new = new WwiseEvents("sfx_ui_main_deck_edit", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_deck_done = new WwiseEvents("sfx_ui_main_deck_done", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_deck_remove_card = new WwiseEvents("sfx_ui_main_deck_remove_card", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_deck_add_card = new WwiseEvents("sfx_ui_main_deck_add_card", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_booster_animate_open_start = new WwiseEvents("sfx_basicloc_flip_a_loop_speed2_start", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_booster_animate_open_stop = new WwiseEvents("sfx_basicloc_flip_a_loop_speed2_stop", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_booster_animate_nav_start = new WwiseEvents("sfx_basicloc_flip_d_loop_speed2_start", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_booster_animate_nav_stop = new WwiseEvents("sfx_basicloc_flip_d_loop_speed2_stop", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_flip_a_loop_speed1_start = new WwiseEvents("sfx_basicloc_flip_a_loop_speed1_start", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_flip_a_loop_speed1_stop = new WwiseEvents("sfx_basicloc_flip_a_loop_speed1_stop", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_flip_a_loop_speed2_start = new WwiseEvents("sfx_basicloc_flip_a_loop_speed2_start", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_basicloc_flip_a_loop_speed2_stop = new WwiseEvents("sfx_basicloc_flip_a_loop_speed2_stop", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_walker_prep_ability_gotostack = new WwiseEvents("sfx_walker_prep_ability_gotostack", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_coin_payment = new WwiseEvents("sfx_ui_coin_payment", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_gems_payment = new WwiseEvents("sfx_ui_gems_payment", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_cardpack_open = new WwiseEvents("sfx_ui_main_cardpack_open", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_cardpack_rare_flip = new WwiseEvents("sfx_ui_main_cardpack_rare_flip", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_cardpack_rare_appear = new WwiseEvents("sfx_ui_main_cardpack_rare_appear", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_generic_click = new WwiseEvents("sfx_ui_generic_click", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_cancel = new WwiseEvents("sfx_ui_cancel", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_back = new WwiseEvents("sfx_ui_cancel", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_logout = new WwiseEvents("sfx_ui_generic_click", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_inputfield = new WwiseEvents("sfx_ui_generic_click", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_invalid = new WwiseEvents("sfx_ui_invalid", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_accept = new WwiseEvents("sfx_ui_accept", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_gold_payout = new WwiseEvents("sfx_ui_gold_payout", 0);

	public static readonly WwiseEvents sfx_ui_purchase_pack = new WwiseEvents("sfx_ui_gold_payout", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_filter_toggle = new WwiseEvents("sfx_ui_main_accept_small", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_drop_event = new WwiseEvents("sfx_ui_cancel", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_join_event = new WwiseEvents("sfx_ui_acceptbig_01", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_play = new WwiseEvents("sfx_ui_play", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_toggle = new WwiseEvents("sfx_ui_generic_click", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_quest_complete = new WwiseEvents("sfx_ui_gold_payout", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_quest_dismiss = new WwiseEvents("sfx_ui_cancel", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_done = new WwiseEvents("sfx_ui_accept", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_click_pane = new WwiseEvents("sfx_ui_accept", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_submit = new WwiseEvents("sfx_ui_accept", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_mulligan_discard = new WwiseEvents("sfx_ui_mulligan_discard", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_mulligan_keep = new WwiseEvents("sfx_ui_mulligan_keep", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_accept_small = new WwiseEvents("sfx_ui_main_accept_small", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_mana_type_filter_rollover = new WwiseEvents("sfx_mana_type_filter_rollover", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_mana_type_filter_select = new WwiseEvents("sfx_mana_type_filter_select", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_rewards_coins_rollover = new WwiseEvents("sfx_ui_main_rewards_coins_rollover", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_rewards_gem_rollover = new WwiseEvents("sfx_ui_main_rewards_gem_rollover", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_rewards_pack_rollover = new WwiseEvents("sfx_ui_main_rewards_pack_rollover", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_pack_open_pack = new WwiseEvents("sfx_ui_boost_pack_open_pack", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_pack_accept_cards = new WwiseEvents("sfx_ui_boost_pack_accept_cards", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_card_flip_common = new WwiseEvents("sfx_ui_boost_card_flip_common", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_card_flip_mythic_rare = new WwiseEvents("sfx_ui_boost_card_flip_mythic_rare", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_pack_depress = new WwiseEvents("sfx_ui_boost_pack_depress", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_pack_rolloff = new WwiseEvents("sfx_ui_boost_pack_rolloff", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_card_flip_rare = new WwiseEvents("sfx_ui_boost_card_flip_rare", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_card_rare_appear = new WwiseEvents("sfx_ui_boost_card_rare_appear", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_card_rare_appear_forceoff = new WwiseEvents("sfx_ui_boost_card_rare_appear_forceoff", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_pack_rollover = new WwiseEvents("sfx_ui_boost_pack_rollover", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_boost_pack_release = new WwiseEvents("sfx_ui_boost_pack_release", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_deckbuilding_box_open = new WwiseEvents("sfx_ui_deckbuilding_box_open", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_deckbuilding_box_close = new WwiseEvents("sfx_ui_deckbuilding_box_close", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_masterytree_rollover = new WwiseEvents("sfx_ui_masterytree_rollover", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_masterytree_orb_click = new WwiseEvents("sfx_ui_masterytree_orb_click", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_masterytree_orb_set = new WwiseEvents("sfx_ui_masterytree_orb_set", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_masterytree_orb_flying = new WwiseEvents("sfx_ui_masterytree_orb_flying", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_masterytree_orb_acquire = new WwiseEvents("sfx_ui_masterytree_orb_acquire", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_masterytree_orb_reward = new WwiseEvents("sfx_ui_masterytree_orb_reward", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_epp_levelup_nextlevel = new WwiseEvents("sfx_ui_epp_levelup_nextlevel", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_epp_levelup_start = new WwiseEvents("sfx_ui_epp_levelup_start", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_epp_levelup_stop = new WwiseEvents("sfx_ui_epp_levelup_stop", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_rewards_chest_open = new WwiseEvents("sfx_ui_main_rewards_chest_open", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_new_crafting_depthart_unlocked = new WwiseEvents("sfx_ui_new_crafting_depthart_unlocked", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_main_season_end_banners = new WwiseEvents("sfx_ui_main_season_end_banners", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_reward_rollover_initial = new WwiseEvents("sfx_ui_reward_rollover_initial", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_reward_rollover_reveal_coin = new WwiseEvents("sfx_ui_reward_rollover_reveal_coin", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_reward_rollover_reveal_gem = new WwiseEvents("sfx_ui_reward_rollover_reveal_gem", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_reward_rollover_reveal_pack = new WwiseEvents("sfx_ui_reward_rollover_reveal_pack", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_reward_rollover_reveal_card = new WwiseEvents("sfx_ui_reward_rollover_reveal_card", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_reward_rollover_reveal_orb = new WwiseEvents("sfx_ui_reward_rollover_reveal_orb", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_reward_rollover_reveal_pet_cat = new WwiseEvents("sfx_ui_reward_rollover_reveal_pet_cat", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_reward_rollover_reveal_generic = new WwiseEvents("sfx_ui_reward_rollover_reveal_generic", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_xp_boost_popup = new WwiseEvents("sfx_ui_xp_boost_popup", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_xp_boost_dismiss = new WwiseEvents("sfx_ui_xp_boost_dismiss", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_xp_boost_claim = new WwiseEvents("sfx_ui_xp_boost_claim", 0, EAudioType.Interaction);

	public static readonly WwiseEvents sfx_ui_reward_reveal = new WwiseEvents("sfx_ui_reward_reveal", 0, EAudioType.Interaction);

	public static readonly WwiseEvents SFX_CARD_ETB_MONSTROSITY = new WwiseEvents("ETB_ManBearPig", 500, EAudioType.ETB);

	public static readonly WwiseEvents SFX_CARD_ETB_ARTIFACT = new WwiseEvents("ETB_Artifact", 501, EAudioType.ETB);

	public static readonly WwiseEvents SFX_CARD_ETB_SERVO = new WwiseEvents("ETB_Servo", 502, EAudioType.ETB);

	public static readonly WwiseEvents SFX_CARD_ETB_GREEN = new WwiseEvents("ETB_Green_Growing", 503, EAudioType.ETB);

	public static readonly WwiseEvents SFX_CARD_ETB_BIRD = new WwiseEvents("ETB_Bird", 504, EAudioType.ETB);

	public static readonly WwiseEvents SFX_CARD_ETB_ELEPHANT = new WwiseEvents("ETB_Elephant", 505, EAudioType.ETB);

	public static readonly WwiseEvents sfx_special_card_scorpion_god_enter = new WwiseEvents("sfx_special_card_scorpion_god_enter", 506, EAudioType.ETB);

	public static readonly WwiseEvents sfx_c_type_etb_metatype_pirateship = new WwiseEvents("sfx_c_type_etb_vehicle_pirateship", 507, EAudioType.ETB);

	public static readonly WwiseEvents sfx_c_type_etb_elemental_lightning = new WwiseEvents("sfx_c_type_etb_elemental_lightning", 508, EAudioType.ETB);

	public static readonly WwiseEvents sfx_c_type_etb_elemental_water = new WwiseEvents("sfx_c_type_etb_elemental_water", 509, EAudioType.ETB);

	public static readonly WwiseEvents sfx_c_type_etb_elemental_wind = new WwiseEvents("sfx_c_type_etb_elemental_wind", 510, EAudioType.ETB);

	public static readonly WwiseEvents sfx_c_type_etb_elemental_energy = new WwiseEvents("sfx_c_type_etb_elemental_energy", 511, EAudioType.ETB);

	public static readonly WwiseEvents sfx_c_type_etb_elemental_fire = new WwiseEvents("sfx_c_type_etb_elemental_fire", 512, EAudioType.ETB);

	public static readonly WwiseEvents sfx_c_type_etb_elemental_earthwood = new WwiseEvents("sfx_c_type_etb_elemental_earthwood", 513, EAudioType.ETB);

	public static readonly WwiseEvents sfx_c_type_etb_equipment = new WwiseEvents("sfx_c_type_etb_equipment", 514, EAudioType.ETB);

	public static readonly WwiseEvents sfx_c_type_etb_crocodile = new WwiseEvents("sfx_c_type_etb_crocodile", 515, EAudioType.ETB);

	public static readonly WwiseEvents VO_Voice_ETB_Servo_Exhibition = new WwiseEvents("VO_Voice_ETB_Servo_Exhibition", 1300, EAudioType.ETB);

	public static readonly WwiseEvents VO_Voice_ETB_Rishkars_Expertise = new WwiseEvents("VO_Voice_ETB_Rishkars_Expertise", 1301, EAudioType.ETB);

	public static readonly WwiseEvents VO_Voice_ETB_Druid_of_the_Cowl = new WwiseEvents("VO_Voice_ETB_Druid_of_the_Cowl", 1302, EAudioType.ETB);

	public static readonly WwiseEvents VO_Voice_ETB_Audacious_Infiltrator = new WwiseEvents("VO_Voice_ETB_Audacious_Infiltrator", 1303, EAudioType.ETB);

	public static readonly WwiseEvents VO_Foley_ETB_Druid_of_the_Cowl = new WwiseEvents("VO_Foley_ETB_Druid_of_the_Cowl", 1304, EAudioType.ETB);

	public static readonly WwiseEvents VO_Foley_ETB_Audacious_Infiltrator = new WwiseEvents("VO_Foley_ETB_Audacious_Infiltrator", 1305, EAudioType.ETB);

	public static readonly WwiseEvents VO_Voice_ETB_Cheif_of_the_Foundry = new WwiseEvents("VO_Voice_ETB_Cheif_of_the_Foundry", 1306, EAudioType.ETB);

	public static readonly WwiseEvents VO_Voice_ETB_Seedsculptor = new WwiseEvents("VO_Voice_ETB_Seedsculptor", 1307, EAudioType.ETB);

	public static readonly WwiseEvents VO_Voice_ETB_Sera_Angel = new WwiseEvents("VO_Voice_ETB_Sera_Angel", 1308, EAudioType.ETB);

	public static readonly WwiseEvents sfx_click_rock = new WwiseEvents("sfx_click_rock", 0);

	public static readonly WwiseEvents sfx_basicloc_shuff = new WwiseEvents("sfx_basicloc_shuff", 0);

	public static readonly WwiseEvents match_making_cancel = new WwiseEvents("match_making_cancle", 0);

	public static readonly WwiseEvents match_making_find_match = new WwiseEvents("match_making_find_match", 0);

	public static readonly WwiseEvents match_making_match_found = new WwiseEvents("match_making_match_found", 0);

	public static readonly WwiseEvents match_making_XLN_intro = new WwiseEvents("match_making_XLN_intro", 0);

	public static readonly WwiseEvents sfx_player_appear = new WwiseEvents("sfx_player_appear", 0);

	public static readonly WwiseEvents sfx_player_death = new WwiseEvents("sfx_player_death", 0);

	public static readonly WwiseEvents SFX_PHASEL_PIP_SLIDE = new WwiseEvents("Phase_Ladder_Pip_Slide", 0);

	public static readonly WwiseEvents SFX_PHASEL_SETSTOP_ON = new WwiseEvents("Phase_Ladder_Set_Stop_On", 0);

	public static readonly WwiseEvents SFX_PHASEL_SETSTOP_OFF = new WwiseEvents("Phase_Ladder_Set_Stop_Off", 0);

	public static readonly WwiseEvents sfx_ui_preferred_printing_expand = new WwiseEvents("sfx_ui_preferred_printing_expand", 0);

	public static readonly WwiseEvents sfx_ui_preferred_printing_retract = new WwiseEvents("sfx_ui_preferred_printing_retract", 0);

	public static readonly WwiseEvents sfx_ui_preferred_printing_select_card = new WwiseEvents("sfx_ui_preferred_printing_select_card", 0);

	private static readonly Dictionary<EAudioType, List<WwiseEvents>> _audioEvents = new Dictionary<EAudioType, List<WwiseEvents>>();

	public string EventName { get; private set; }

	public int SerializedID { get; private set; }

	public EAudioType AudioType { get; private set; }

	private static void InitializeDictionary()
	{
		if (_audioEvents.Count != 0)
		{
			return;
		}
		foreach (object value in EnumHelper.GetValues(typeof(EAudioType)))
		{
			_audioEvents.Add((EAudioType)value, new List<WwiseEvents>());
		}
		FieldInfo[] fields = typeof(WwiseEvents).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			if (fields[i].GetValue(null) is WwiseEvents wwiseEvents)
			{
				_audioEvents[wwiseEvents.AudioType].Add(wwiseEvents);
			}
		}
	}

	public static List<WwiseEvents> GetAllEvents()
	{
		InitializeDictionary();
		List<WwiseEvents> list = new List<WwiseEvents>();
		foreach (KeyValuePair<EAudioType, List<WwiseEvents>> audioEvent in _audioEvents)
		{
			list.AddRange(audioEvent.Value);
		}
		return list;
	}

	public static List<WwiseEvents> GetEventsOfType(EAudioType audioType)
	{
		InitializeDictionary();
		return _audioEvents[audioType].Concat(new List<WwiseEvents> { INVALID }).ToList();
	}

	public static WwiseEvents GetEventById(int id)
	{
		InitializeDictionary();
		WwiseEvents wwiseEvents = null;
		if ((wwiseEvents = _audioEvents[EAudioType.Generic].Find((WwiseEvents x) => x.SerializedID == id)) != null)
		{
			return wwiseEvents;
		}
		if ((wwiseEvents = _audioEvents[EAudioType.Interaction].Find((WwiseEvents x) => x.SerializedID == id)) != null)
		{
			return wwiseEvents;
		}
		if ((wwiseEvents = _audioEvents[EAudioType.ETB].Find((WwiseEvents x) => x.SerializedID == id)) != null)
		{
			return wwiseEvents;
		}
		return INVALID;
	}

	public static WwiseEvents GetEventByName(string name)
	{
		InitializeDictionary();
		WwiseEvents wwiseEvents = null;
		if ((wwiseEvents = _audioEvents[EAudioType.Generic].Find((WwiseEvents x) => x.EventName == name)) != null)
		{
			return wwiseEvents;
		}
		if ((wwiseEvents = _audioEvents[EAudioType.Interaction].Find((WwiseEvents x) => x.EventName == name)) != null)
		{
			return wwiseEvents;
		}
		if ((wwiseEvents = _audioEvents[EAudioType.ETB].Find((WwiseEvents x) => x.EventName == name)) != null)
		{
			return wwiseEvents;
		}
		return INVALID;
	}

	private WwiseEvents(string eventName, int serializedId, EAudioType audioType = EAudioType.Generic)
	{
		EventName = eventName;
		SerializedID = serializedId;
		AudioType = audioType;
	}

	public WwiseEvents(string eventName)
	{
		EventName = eventName;
	}
}
