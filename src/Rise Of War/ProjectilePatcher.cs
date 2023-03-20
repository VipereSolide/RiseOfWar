using System.Collections;
using System;

using UnityEngine.UI;
using UnityEngine;
using HarmonyLib;
using TMPro;

namespace RiseOfWar
{
    public class ProjectilePatcher
    {
        [HarmonyPatch(typeof(Projectile), "Awake")]
        [HarmonyPrefix]
        private static void AwakePatch(Projectile __instance)
        {
            __instance.configuration.gravityMultiplier = 2;
            __instance.configuration.lifetime = GameConfiguration.projectileLifetime;
        }

        [HarmonyPatch(typeof(Projectile), "Awake")]
        [HarmonyPostfix]
        private static void AwakePatchPostfix(Projectile __instance)
        {
            // __instance.gameObject.AddComponent<CustomProjectile>();
        }

        [HarmonyPatch(typeof(Projectile), "Update")]
        [HarmonyPostfix]
        private static void UpdatePatch(Projectile __instance)
        {
            __instance.configuration.gravityMultiplier = __instance.velocity.magnitude * Time.deltaTime;
        }
    }
}