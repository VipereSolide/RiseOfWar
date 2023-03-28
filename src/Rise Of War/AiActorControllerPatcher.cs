using HarmonyLib;

namespace RiseOfWar
{
    public class AiActorControllerPatcher
    {
        [HarmonyPatch(typeof(AiActorController), "FillDriverSeat")]
        [HarmonyPrefix]
        private static bool FillDriverSeatPatch(AiActorController __instance)
        {
            return false;
        }
    }
}