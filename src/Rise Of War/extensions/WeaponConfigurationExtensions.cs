using System;
using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    public static class WeaponConfigurationExtensions
    {
        public static Projectile projectile(this Weapon.Configuration _configuration)
        {
            if (_configuration.projectilePrefab == null)
            {
                return null;
            }

            return _configuration.projectilePrefab.GetComponent<Projectile>();
        }
    }
}