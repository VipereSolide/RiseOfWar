using HarmonyLib;
using UnityEngine;

namespace RiseOfWar
{
    public class SeatPatcher
    {
        [HarmonyPatch(typeof(Seat), "OccupantLeft")]
        [HarmonyPrefix]
        private static void PatchOccupantLeft(Seat __instance)
        {
            Actor _actor = __instance.occupant;
            if (_actor.controller == FpsActorController.instance)
            {
                FpsActorController.instance.SetSeatCamera(null);
            }
        }

        [HarmonyPatch(typeof(Seat), "SwitchToThirdPersonCamera")]
        [HarmonyPrefix]
        private static void PatchSwitchToThirdPersonCamera(Seat __instance)
        {
            Camera _thirdPersonCamera = (Camera)Traverse.Create(__instance).Field("thirdPersonCamera").GetValue();

            if (_thirdPersonCamera != null && FpsActorController.instance != null)
            {
                FpsActorController.instance.SetSeatCamera(_thirdPersonCamera);
            }
        }

        [HarmonyPatch(typeof(Seat), "SwitchToDefaultCamera")]
        [HarmonyPrefix]
        private static void PatchSwitchToDefaultCamera(Seat __instance)
        {
            if (FpsActorController.instance != null)
            {
                FpsActorController.instance.SetSeatCamera(null);
            }
        }
    }
}