using UnityEngine;
using HarmonyLib;
namespace RiseOfWar
{
    public class ModManagerPatcher
    {
        [HarmonyPatch(typeof(ModManager), "FinalizeLoadedModContent")]
        [HarmonyPostfix]
        static void FinalizeLoadedModContentPatch(ModManager __instance)
        {
            ResourceManager.Instance.DisableLoadingScreen();
        }
    }
}