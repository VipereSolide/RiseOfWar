using HarmonyLib;
using System;
using UnityEngine;

namespace RiseOfWar
{
    using Events;

    public static class HitboxPatcher
    {
        [HarmonyPatch(typeof(HitboxPatcher), "ProjectileHit")]
        [HarmonyPrefix]
        private static bool ProjectileHit(Hitbox __instance, ref bool __result, Projectile p, Vector3 position)
        {
            Plugin.Log("HitboxPatcher: Projectile Hit. Is height based multiplier = " + __instance.heightBasedMultiplier);

            float _damageMultiplier = __instance.multiplier;
            bool result;

            if (__instance.heightBasedMultiplier)
            {
                float _multiplier = __instance.transform.worldToLocalMatrix.MultiplyPoint(position).x;
                float _clamped = Mathf.Clamp01(0.95f - _multiplier);
                _damageMultiplier = Mathf.Lerp(__instance.minMultiplier, __instance.multiplier, _clamped);
                Plugin.Log("HitboxPatcher: Damage multiplier = " + _damageMultiplier + ". Default multiplier = " + __instance.multiplier);
            }

            try
            {
                DamageInfo info = new DamageInfo(DamageInfo.DamageSourceType.Projectile, p.source, p.sourceWeapon)
                {
                    healthDamage = p.Damage() * _damageMultiplier,
                    balanceDamage = p.BalanceDamage(),
                    isPiercing = p.configuration.piercing,
                    isCriticalHit = (_damageMultiplier > 1.01f),
                    point = position,
                    direction = p.transform.forward,
                    impactForce = p.configuration.impactForce * p.transform.forward
                };

                result = __instance.parent.Damage(info);
            }
            catch (Exception)
            {
                result = false;
            }

            OnProjectileHitHitboxEvent _event = new OnProjectileHitHitboxEvent(p, position, __instance);
            EventManager.onProjectileHitHitbox.Invoke(_event);

            __result = result;
            return false;
        }
    }
}