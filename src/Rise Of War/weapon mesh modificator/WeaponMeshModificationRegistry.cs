using System.Collections.Generic;
using System.Xml.Serialization;
using System.Collections;
using System;

using UnityEngine;
using HarmonyLib;

namespace RiseOfWar.WeaponMeshModificator
{
    public static class WeaponMeshModificationRegistry
    {
        public static List<WeaponMeshModification> weaponMeshModifications = new List<WeaponMeshModification>();

        public static List<WeaponMeshModification> GetWeaponMeshModifications(Weapon weapon)
        {
            List<WeaponMeshModification> _result = new List<WeaponMeshModification>();

            foreach(WeaponMeshModification _modification in weaponMeshModifications)
            {
                if (_modification.weapon == weapon.name || _modification.weapon == WeaponRegistry.GetRealName(weapon))
                {
                    _result.Add(_modification);
                }
            }

            //Plugin.Log("WeaponMeshModificationRegistry: " + _result.Count + " results.");
            return _result;
        }
    }
}