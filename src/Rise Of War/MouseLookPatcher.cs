using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

namespace RiseOfWar
{
    public class MouseLookPatcher
    {
        [HarmonyPatch(typeof(MouseLook), "LookRotation")]
        [HarmonyPrefix]
        static bool LookRotationPatch(MouseLook __instance, Transform character, Transform camera)
        {
			return false;
		}
    }
}