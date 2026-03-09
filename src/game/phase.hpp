#pragma once

enum class Phase {
    Untap,
    Upkeep,
    Draw,
    Main_1,
    Beginning_Of_Combat,
    Declare_Attackers,
    Declare_Blockers,
    First_Strike_Damage,
    Combat_Damage,
    End_Of_Combat,
    Main_2,
    End_Step,
    Cleanup,
};
