using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace RiseOfWar
{
    public static class MouseLookExtensions
    {
        public static Vector2 GetScriptedRotationInput(this MouseLook _mouseLook)
        {
            return (Vector2)Traverse.Create(_mouseLook).Field("scriptedRotationInput").GetValue();
        }

        public static void SetScriptedRotationInput(this MouseLook _mouseLook, Vector2 _value)
        {
            Traverse.Create(_mouseLook).Field("scriptedRotationInput").SetValue(_value);
        }
    }
}