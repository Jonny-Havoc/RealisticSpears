using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace RealisticSpears
{
    [BepInPlugin("com.havocmods.realisticspears", "Realistic Spears", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static HarmonyLib.Harmony HarmonyInstance;

        private void Awake()
        {
            HarmonyInstance = new HarmonyLib.Harmony("com.havocmods.realisticspears");
            HarmonyInstance.PatchAll();

            // AnimationSpeedManager: 2x only when player is doing the swapped sword anim on a spear.
            AnimationSpeedManager.Add((character, speed) =>
            {
                var player = character as Player;
                if (player == null || !player.InAttack())
                    return speed;

                var atk  = AccessTools.Field(typeof(Humanoid), "m_currentAttack").GetValue(player) as Attack;
                var wep  = player.GetCurrentWeapon();
                var sdat = wep?.m_shared;

                if (atk == null || sdat == null) return speed;

                if (SpearUtils.IsSpearWithSwordAttack_Shared(sdat) &&
                    string.Equals(atk.m_attackAnimation, "sword_secondary", System.StringComparison.OrdinalIgnoreCase))
                {
                    return speed * 2.2f;
                }

                return speed;
            });

            Logger.LogInfo("[RealisticSpears] Loaded.");
        }

        private void OnDestroy() => HarmonyInstance?.UnpatchSelf();
    }
}
