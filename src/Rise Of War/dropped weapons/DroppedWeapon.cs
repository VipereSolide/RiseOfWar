using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    public class DroppedWeapon : MonoBehaviour
    {
        public WeaponManager.WeaponEntry prefab;
        public Weapon.Configuration configuration;

        public int currentAmmo;
        public int currentSpareAmmo;
        public WeaponModifications modifications;

        public Dictionary<string, object> settings = new Dictionary<string, object>();
        private Weapon _weapon;

        public virtual void Pickup(Actor picker)
        {
            if (prefab != null)
            {
                _weapon = picker.EquipNewWeaponEntry(prefab, 0, true);
                _weapon.configuration = configuration;
                _weapon.ResetSetup();

                Invoke(nameof(ApplyWeaponSpecs), 0.1f);
            }

            gameObject.SetActive(false);
        }

        private void ApplyWeaponSpecs()
        {
            Plugin.Log("DroppedWeapon: Setting up correct weapon stats...");

            _weapon.ammo = currentAmmo;
            _weapon.spareAmmo = currentSpareAmmo;
            _weapon.GetAdditionalData().modifications = modifications;

            Plugin.Log("DroppedWeapon: Setted up correct weapon stats.");
        }
    }
}