using HarmonyLib;
using System.Collections.Generic;

namespace RiseOfWar
{
    public class WeaponManagerPatcher
    {
        [HarmonyPatch(typeof(WeaponManager), "GetWeaponEntriesOfSlot")]
        [HarmonyPrefix]
        public static bool GetWeaponEntriesOfSlotPatch(ref List<WeaponManager.WeaponEntry> __result, WeaponManager.WeaponSlot slot)
        {
            List<WeaponManager.WeaponEntry> _weaponEntries = new List<WeaponManager.WeaponEntry>();

            foreach (WeaponManager.WeaponEntry _weaponEntry in WeaponManager.instance.allWeapons)
            {
                if (_weaponEntry.slot == slot && WeaponRegistry.IsCustomWeapon(_weaponEntry))
                {
                    _weaponEntries.Add(_weaponEntry);
                }
            }

            __result = _weaponEntries;
            return false;
        }
    }
}