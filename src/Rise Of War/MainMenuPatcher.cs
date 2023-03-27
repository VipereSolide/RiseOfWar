using HarmonyLib;

namespace RiseOfWar
{
    public class MainMenuPatcher
    {
        [HarmonyPatch(typeof(MainMenu), "PlayMusic")]
        [HarmonyPrefix]
        static void PlayMusicPatch(MainMenu __instance)
        {
            ResourceManager.Instance.GetAndApplyMusicThemes();
        }
    }
}