using System;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

        [HarmonyPatch(typeof(Projectile), "Update")]
        [HarmonyPostfix]
        private static void UpdatePatch(Projectile __instance)
        {
            __instance.configuration.gravityMultiplier = __instance.velocity.magnitude * Time.deltaTime;
        }
    }
}