using System;
using UnityEngine;
using HarmonyLib;
using System.Collections;

namespace RiseOfWar
{
    public class GameModeBasePatcher
    {
        [HarmonyPatch(typeof(GameModeBase), "TimeToPlayerRespawn")]
        [HarmonyPrefix]
        public static bool TimeToPlayerRespawnPatch(ref int __result)
        {
            __result = 2;

            return false;
        }

        [HarmonyPatch(typeof(GameModeBase), "StartRecurringSpawnWave")]
        [HarmonyPrefix]
        private static bool StartRecurringSpawnWavePatch(GameModeBase __instance)
        {
            __instance.OnSpawnWave();
            __instance.SetProperty<TimedAction>("respawnWaveAction", new TimedAction(0f, false));
            __instance.Invoke("StartRecurringSpawnWave", 0.5f);
            GameManager.GameParameters().respawnTime = 0;
            __instance.ForceSpawnDeadActors();

            return false;
        }
    }
}