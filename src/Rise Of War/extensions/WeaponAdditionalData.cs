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

        public void InitWeaponModifications()
        {
            modifications = new WeaponModifications();
        }

        public WeaponAdditionalData()
        {
            aimingAnchor = null;
            isCustomWeapon = false;
            InitWeaponModifications();
        }

        public WeaponAdditionalData(Transform _aimingAnchor)
        {
            aimingAnchor = _aimingAnchor;
            isCustomWeapon = true;
            InitWeaponModifications();
        }
        public WeaponAdditionalData(Transform _aimingAnchor, bool _isCustomWeapon)
        {
            aimingAnchor = _aimingAnchor;
            isCustomWeapon = _isCustomWeapon;
            InitWeaponModifications();
        }
    }
}