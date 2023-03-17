using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace RiseOfWar
{
    public class OptionsPatcher
    {
        [HarmonyPatch(typeof(Options), "Awake")]
        [HarmonyPostfix]
        static void AwakePatch(Options __instance)
        {
            OptionSlider[] _sliders = (OptionSlider[])Traverse.Create(__instance).Field("sliders").GetValue();

            foreach (OptionSlider _slider in _sliders)
            {
                if (_slider.id != OptionSlider.Id.Fov)
                {
                    continue;
                }

                _slider.defaultValue = 60;
                _slider.GetUiElement().minValue = 55;
                _slider.GetUiElement().maxValue = 80;

                return;
            }
        }
    }
}