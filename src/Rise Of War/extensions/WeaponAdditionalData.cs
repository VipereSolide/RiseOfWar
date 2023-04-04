using System;
using UnityEngine;

namespace RiseOfWar
{
    [Serializable]
    public class WeaponAdditionalData
    {
        public WeaponModifications modifications;
        public bool hasCustomDisplayName;
        public float damageToVehicles;
        public Transform aimingAnchor;
        public Transform recoilAnchor;
        public GameObject projectile;
        public float currentConefire;
        public bool isCustomWeapon;
        public bool wasAiming;

        public Animation animation;
        public string fireAnimationName;
        public string reloadAnimationName;
        public string idleAnimationName;
        public string unholsterAnimationName;

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