using System.Collections.Generic;
using System;

using UnityEngine.UI;
using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    public static class WeaponSwitchGroupPatcher
    {
        [HarmonyPatch(typeof(WeaponSwitchGroup), "SetEntries")]
        [HarmonyPrefix]
        static bool SetEntriesPatch(WeaponSwitchGroup __instance, ref Dictionary<WeaponManager.WeaponEntry, Toggle> __result, List<WeaponManager.WeaponEntry> entries)
        {
            float _screenBasePadding = Mathf.FloorToInt((float)Screen.width / 250f);
            float _screenWidth = 1f / _screenBasePadding;
            int _entryHeight = 0;

            __instance.height = 0f;
            __instance.SetProperty("weaponToggles", new Dictionary<WeaponManager.WeaponEntry, Toggle>());

            foreach (WeaponManager.WeaponEntry _weaponEntry in entries)
            {
                if (WeaponRegistry.IsCustomWeapon(_weaponEntry) == false)
                {
                    continue;
                }

                RectTransform _instantiatedEntry = GameObject.Instantiate(__instance.entryPrefab).GetComponent<RectTransform>();
                _instantiatedEntry.name = WeaponRegistry.GetRealName(_weaponEntry);
                _instantiatedEntry.SetParent(__instance.container, false);
                _instantiatedEntry.anchorMin = new Vector2((float)_entryHeight * _screenWidth, 1f);
                _instantiatedEntry.anchorMax = new Vector2((float)(_entryHeight + 1) * _screenWidth, 1f);
                _instantiatedEntry.anchoredPosition = new Vector2(5f, -__instance.height - 5f);
                _instantiatedEntry.GetComponentInChildren<Text>().text = WeaponRegistry.GetRealName(_weaponEntry);

                Toggle _entryToggle = _instantiatedEntry.GetComponentInChildren<Toggle>();
                _entryToggle.GetComponentInChildren<Text>().color = (_weaponEntry.usableByAi ? Color.white : Color.gray);
                
                _entryToggle.onValueChanged.AddListener((bool _isToggled) =>
                {
                    WeaponSwitch.instance.OnEntryToggled(_weaponEntry, _isToggled);
                });

                __instance.GetProperty<Dictionary<WeaponManager.WeaponEntry, Toggle>>("weaponToggles").Add(_weaponEntry, _instantiatedEntry.GetComponentInChildren<Toggle>());
                _entryHeight++;

                if (_entryHeight >= _screenBasePadding)
                {
                    _entryHeight = 0;
                    __instance.height += 30f;
                }
            }

            if (_entryHeight > 0)
            {
                __instance.height += 30f;
            }

            __instance.container.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, __instance.height);
            __result = __instance.GetProperty<Dictionary<WeaponManager.WeaponEntry, Toggle>>("weaponToggles");

            return false;
        }
    }
}