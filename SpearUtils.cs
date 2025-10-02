using UnityEngine;

namespace RealisticSpears
{
    internal static class SpearUtils
    {
        internal static bool IsSpearWithPokeAttack_Shared(ItemDrop.ItemData.SharedData shared)
        {
            return shared != null
                && shared.m_skillType == Skills.SkillType.Spears
                && shared.m_attack != null
                && shared.m_attack.m_attackAnimation == "spear_poke";
        }

        internal static bool IsSpearWithSwordAttack_Shared(ItemDrop.ItemData.SharedData shared)
        {
            return shared != null
                && shared.m_skillType == Skills.SkillType.Spears
                && shared.m_attack != null
                && shared.m_attack.m_attackAnimation == "sword_secondary";
        }

        internal static bool IsFangSpear_Shared(ItemDrop.ItemData.SharedData shared)
        {
            // vanilla token is "$item_spear_wolffang"
            return shared != null
                && !string.IsNullOrEmpty(shared.m_name)
                && shared.m_name == "$item_spear_wolffang";
        }
    }
}
