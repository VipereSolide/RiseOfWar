using HarmonyLib;
using UnityEngine;

namespace RiseOfWar
{
    public class GameManagerPatcher
    {
        [HarmonyPatch(typeof(GameManager), "Awake")]
        [HarmonyPostfix]
        static void AwakePatch()
        {
            SoundManager.instance.ResetAudioMixerGroup();
        }

        [HarmonyPatch(typeof(GameManager), "StartGame")]
        [HarmonyPostfix]
        static void StartGamePatch(GameManager __instance)
        {
            DecalManager.SetBloodDecalColor(0, Random.ColorHSV(0, 0.05f, 0.5f, 1f, 0.1f, 0.5f, 0.1f, 0.125f));
            DecalManager.SetBloodDecalColor(1, Random.ColorHSV(0, 0.05f, 0.5f, 1f, 0.1f, 0.5f, 0.1f, 0.125f));
        }
    }
}