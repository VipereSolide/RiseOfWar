using HarmonyLib;
using UnityStandardAssets.Characters.FirstPerson;

namespace RiseOfWar
{
    public class FirstPersonControllerPatcher
    {
        [HarmonyPatch(typeof(FirstPersonController), "SetInput")]
        [HarmonyPrefix]
        static bool SetInputPatch(FirstPersonController __instance, float horizontal, float vertical, bool jump, float aimHorizontal, float aimVertical)
        {
            Traverse.Create(__instance).Field("horizontalInput").SetValue(horizontal);
            Traverse.Create(__instance).Field("verticalInput").SetValue(vertical);
            Traverse.Create(__instance).Field("jumpInput").SetValue(jump);
            MouseController.instance.SetMouseInput(aimHorizontal, aimVertical);

            return false;
        }
    }
}