using System;
using UnityEngine;

namespace RiseOfWar
{
    [Serializable]
    public class WeaponAdditionalData
    {
        public Transform aimingAnchor;
        public Transform recoilAnchor;
        public AudioSource source;
        public bool isCustomWeapon;
        public bool wasAiming;
        public WeaponModifications modifications;
        public bool hasCustomDisplayName;

        public float currentConefire;

        public WeaponAdditionalData()
        {
            aimingAnchor = null;
            isCustomWeapon = false;
            modifications = new WeaponModifications();
        }

        public WeaponAdditionalData(Transform _aimingAnchor)
        {
            aimingAnchor = _aimingAnchor;
            isCustomWeapon = true;
        }
        public WeaponAdditionalData(Transform _aimingAnchor, bool _isCustomWeapon)
        {
            aimingAnchor = _aimingAnchor;
            isCustomWeapon = _isCustomWeapon;
        }
    }
}