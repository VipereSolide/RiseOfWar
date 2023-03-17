using HarmonyLib;

namespace RiseOfWar
{
    using Events;
    using UnityEngine;

    public class ActorPatcher
    {
        [HarmonyPatch(typeof(Actor), "Die")]
        [HarmonyPrefix]
        static bool DiePatch(Actor __instance, DamageInfo info, bool isSilentKill)
        {
            if (__instance.dead)
            {
                return false;
            }

            EventManager.onActorDie.Invoke(new OnActorDieEvent(info, __instance, isSilentKill));
            return true;
        }

        [HarmonyPatch(typeof(Actor), "Damage")]
        [HarmonyPrefix]
        static bool DamagePatch(Actor __instance, DamageInfo info)
        {
            if (__instance.dead)
            {
                return false;
            }

            if (!__instance.GetAdditionalData().CanGetDamaged())
            {
                return false;
            }

            __instance.GetAdditionalData().lastTimeHit = Time.time;
            Plugin.Log("ActorPatcher: An actor took damage. Damage took = " + info.healthDamage + "; Point where projectile landed = " + info.point);

            if (info.sourceActor.team == __instance.team && info.sourceActor != __instance)
            {
                return false;
            }

            if (__instance.isInvulnerable)
            {
                return false;
            }

            __instance.onTakeDamage.Invoke(__instance, info.sourceActor, info);
            if (__instance.onTakeDamage.isConsumed)
            {
                return false;
            }

            IngameUi.OnDamageDealt(info, new HitInfo(__instance));

            __instance.controller.ReceivedDamage(false, info.healthDamage, info.balanceDamage, info.point, info.direction, info.impactForce);
            __instance.health -= info.healthDamage;

            int num4 = Mathf.Min(Mathf.CeilToInt(info.healthDamage / 10f), 16);
            float d = 0.1f;

            switch (BloodParticle.BLOOD_PARTICLE_SETTING)
            {
                case BloodParticle.BloodParticleType.Reduced:
                    num4 /= 2;
                    break;
                case BloodParticle.BloodParticleType.BloodExplosions:
                    d = 0.3f;
                    break;
            }

            Vector3 baseVelocity = Vector3.ClampMagnitude(info.impactForce * d, 5f);

            if (BloodParticle.BLOOD_PARTICLE_SETTING != BloodParticle.BloodParticleType.None)
            {
                for (int i = 0; i < num4; i++)
                {
                    // We use team 1 so all actors have red blood.
                    DecalManager.CreateBloodDrop(info.point, baseVelocity, 1);
                }
            }

            int num5 = Mathf.Clamp(num4, 1, 2);
            for (int j = 0; j < num5; j++)
            {
                // We use team 1 so all actors have red blood.
                DecalManager.EmitBloodEffect(info.point, baseVelocity, 1);
            }

            bool flag5 = info.isSplashDamage && !__instance.fallenOver && info.balanceDamage > 200f;

            if (__instance.health <= 0f)
            {
                __instance.Kill(info);
            }
            else if (__instance.ragdoll.IsRagdoll())
            {
                __instance.ApplyRigidbodyForce(info.impactForce);
            }

            if (__instance.ragdoll.IsRagdoll() && flag5)
            {
                Vector3 b = UnityEngine.Random.insideUnitSphere.ToGround() * 5f;
                Vector3 a = __instance.Position();
                Rigidbody[] rigidbodies = __instance.ragdoll.rigidbodies;
                for (int k = 0; k < rigidbodies.Length; k++)
                {
                    rigidbodies[k].AddForceAtPosition(new Vector3(0f, 2f, 0f), a + b, ForceMode.VelocityChange);
                }
            }

            if (!__instance.aiControlled)
            {
                IngameUi.instance.SetHealth(Mathf.Max(0f, __instance.health));
                float intensity = Mathf.Clamp01(0.3f + (1f - __instance.health / __instance.maxHealth));
                IngameUi.instance.ShowVignette(intensity, 6f);
            }

            else if (info.balanceDamage > 20f)
            {
                __instance.GetProperty<TimedAction>("parachuteDeployStunAction").Start();
            }

            return false;
        }
    }
}