using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace RiseOfWar
{
    public static class OptionSliderExtensions
    {
        public static Slider GetUiElement(this OptionSlider _optionSlider)
        {
            Slider _slider = (Slider)Traverse.Create(_optionSlider).Field("uiElement").GetValue();

            if (_slider == null)
            {
                _slider = _optionSlider.transform.GetComponentInChildren<Slider>();
            }

            return _slider;
        }
    }
}