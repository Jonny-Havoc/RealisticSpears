using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace RealisticSpears
{
    // 1) Swap spear_poke -> sword_secondary on EQUIP and tweak attack params
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    public static class Patch_Humanoid_EquipItem
    {
        static void Prefix(Humanoid __instance, ItemDrop.ItemData item)
        {
            if (item == null) return;

            var s = item.m_shared;
            if (!SpearUtils.IsSpearWithPokeAttack_Shared(s)) return;

            // Swap animation
            s.m_attack.m_attackAnimation = "sword_secondary";

            // stabby tweaks
            s.m_attack.m_attackRange = 3.0f;  // default 1.9
            s.m_attack.m_attackHeight = 1.0f;  // default 1.5
            s.m_attack.m_attackAngle = 30;    // default 40 
            s.m_attack.m_attackRayWidth = 0.4f;  // default 0.5 
        }
    }

    // 2) Rotate the equipped spear's mesh
    // We patch UpdateEquipment (runs whenever visuals refresh). We find the right-hand instance
    // and rotate the "attach"
    // local transform and update the instance on every visual refresh so we can toggle
    // rotation on/off while keeping a stable original baseline. Very messy but hey, it works.
    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.CustomUpdate))]
    public static class Patch_VisEquipment_CustomUpdate
    {
        static readonly FieldInfo RightItemField = AccessTools.Field(typeof(VisEquipment), "m_rightItemInstance");

        public static void Postfix(VisEquipment __instance, float deltaTime, float time)
        {
            if (__instance == null || RightItemField == null) return;

            var hum = __instance.GetComponent<Humanoid>();
            var wep = hum?.GetCurrentWeapon();
            var s = wep?.m_shared;

            if (s == null || !SpearUtils.IsSpearWithSwordAttack_Shared(s))
                return;

            // Get the equipped right-hand GO
            var rightGO = RightItemField.GetValue(__instance) as GameObject;
            if (!rightGO) return;

            // Choose a stable child to rotate: exact/contains "attach", else first MeshRenderer, else root.
            var tf = FindBestTargetTransform(rightGO);
            if (tf == null) return;

            bool isFang = SpearUtils.IsFangSpear_Shared(s);

            // determine whether to skip rotation when the player is in the swapped secondary attack
            bool skipRotation = false;
            var player = hum as Player;
            if (player != null && player.InAttack())
            {
                var atk = AccessTools.Field(typeof(Humanoid), "m_currentAttack").GetValue(hum) as Attack;
                if (atk != null && string.Equals(atk.m_attackAnimation, "spear_throw", System.StringComparison.OrdinalIgnoreCase))
                {
                    skipRotation = true;
                }
            }

            // Ensure we have a marker that holds the original local transform and the current applied state.
            var marker = tf.GetComponent<SpearRotatedMarker>();
            if (marker == null)
            {
                marker = tf.gameObject.AddComponent<SpearRotatedMarker>();
                marker.origLocalPos = tf.localPosition;
                marker.origLocalRot = tf.localRotation;
                marker.rotationApplied = false;
                marker.initialized = true;
            }

            // Apply the correct state each update (idempotent): rotation may be toggled on/off at runtime.
            SpearPositioner.ApplyState(tf.gameObject, marker, applyRotation: !skipRotation, isFangSpear: isFang);
        }

        private static Transform FindBestTargetTransform(GameObject inst)
        {
            // exact "attach"
            var exact = FindDeepChild_Internal(inst.transform, "attach", exact: true);
            if (exact) return exact;

            // contains "attach"
            var fuzzy = FindDeepChild_Internal(inst.transform, "attach", exact: false);
            if (fuzzy) return fuzzy;

            // first MeshRenderer
            var mr = inst.GetComponentInChildren<MeshRenderer>(true);
            if (mr) return mr.transform;

            // fallback root
            return inst.transform;
        }

        private static Transform FindDeepChild_Internal(Transform root, string name, bool exact)
        {
            var all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (exact)
                {
                    if (string.Equals(t.name, name, System.StringComparison.OrdinalIgnoreCase)) return t;
                }
                else
                {
                    if (t.name.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0) return t;
                }
            }
            return null;
        }
    }
}
