﻿using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    using Events;

    public class ActorPatcher
    {
        private static Dictionary<Actor, bool> _hasDroppedWeapon = new Dictionary<Actor, bool>();

        [HarmonyPatch(typeof(Actor), "Awake")]
        [HarmonyPostfix]
        private static void Awake(Actor __instance)
        {
            __instance.AddData(new ActorAdditionalData(__instance));
        }

        [HarmonyPatch(typeof(Actor), "Update")]
        [HarmonyPostfix]
        private static void Update(Actor __instance)
        {
            if (__instance.scoreboardEntry == null || __instance.scoreboardEntry.fText == null || __instance.GetAdditionalData() == null)
            {
                return;
            }

            __instance.scoreboardEntry.flagCaptures = __instance.GetAdditionalData().score;
            __instance.scoreboardEntry.fText.text = __instance.scoreboardEntry.flagCaptures.ToString();

            __instance.UpdateInsideFlag();
        }

        [HarmonyPatch(typeof(Actor), "SpawnAt")]
        [HarmonyPostfix]
        static void SpawnAtPatch(Actor __instance)
        {
            EventManager.onActorSpawn.Invoke(new OnActorSpawnEvent(__instance));

            if (_hasDroppedWeapon.ContainsKey(__instance))
            {
                _hasDroppedWeapon[__instance] = false;
            }
            else
            {
                _hasDroppedWeapon.Add(__instance, false);
            }
        }

        [HarmonyPatch(typeof(Actor), "SpawnWeapon")]
        [HarmonyPrefix]
        static bool SpawnWeaponPatch(Actor __instance, ref Weapon __result, WeaponManager.WeaponEntry entry, int slotNumber)
        {
            if (WeaponRegistry.IsCustomWeapon(entry) == false)
            {
                __result = null;
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(Actor), "Die")]
        [HarmonyPrefix]
        static bool DiePatch(Actor __instance, DamageInfo info, bool isSilentKill)
        {
            Plugin.Log($"ActorPatcher: Actor \"{__instance.name}\" died.");
            ActorAdditionalData _actorAdditional = __instance.GetAdditionalData();

            EventManager.onActorDie.Invoke(new OnActorDieEvent(info, __instance, isSilentKill));

            DropActorWeapon(__instance);
            return true;
        }

        static void DropActorWeapon(Actor _actor)
        {
            Weapon _weapon = _actor.activeWeapon;

            if (_hasDroppedWeapon.ContainsKey(_actor))
            {
                if (_hasDroppedWeapon[_actor])
                {
                    return;
                }

                _hasDroppedWeapon[_actor] = true;
            }
            else
            {
                _hasDroppedWeapon.Add(_actor, true);
            }

            if (_weapon == null || _actor.dead)
            {
                return;
            }

            Plugin.Log($"ActorPatcher: Initializing weapon drop for weapon \"{_weapon.name}\" of actor \"{_actor.name}\"...");

            GameObject _weaponPlaceholder = GameObject.Instantiate(_weapon.transform).gameObject;

            Vector3 _position = _weapon.transform.position;
            _position.y = _actor.Position().y + 0.15f;

            _weaponPlaceholder.transform.position = _position;
            _weaponPlaceholder.transform.localScale = Vector3.one * 1.15f;

            GameObject.Destroy(_weaponPlaceholder.GetComponent<Weapon>());
            _weaponPlaceholder.transform.SetLayerRecursively(GameConfiguration.interractableLayer);

            BoxCollider _collider = _weaponPlaceholder.AddComponent<BoxCollider>();
            _collider.center = Vector3.zero;
            _collider.size = new Vector3(0.25f, 0.25f, 0.6f);

            DroppedWeapon _dropped = _weaponPlaceholder.AddComponent<DroppedWeapon>();
            _dropped.prefab = _weapon.weaponEntry;
            _dropped.configuration = _weapon.configuration;
            _dropped.currentAmmo = _weapon.ammo;
            _dropped.currentSpareAmmo = _weapon.spareAmmo;
            _dropped.modifications = _weapon.GetAdditionalData().modifications;

            foreach (Transform _child in _weaponPlaceholder.transform)
            {
                string _childName = _child.name.ToLower();

                if (_childName == "armature" || _childName == "hands")
                {
                    GameObject.Destroy(_child.gameObject);
                }
            }
        }

        [HarmonyPatch(typeof(Actor), "Damage")]
        [HarmonyPrefix]
        static bool DamagePatch(Actor __instance, DamageInfo info)
        {
            if (__instance == null)
            {
                return false;
            }

            if (info.sourceActor.team == __instance.team && info.sourceActor != __instance)
            {
                return false;
            }

            Plugin.EndLogGroup();

            Plugin.Log($"ActorPatcher: Dealing damage to actor \"{__instance.name}\"...");

            if (info.sourceActor != null)
            {
                Plugin.Log($"ActorPatcher: Damage of type \"{info.type}\" from source (Actor) \"{info.sourceActor.name}\".");
            }
            else
            {
                Plugin.Log($"ActorPatcher: Damage of type \"{info.type}\".");
            }

            if (__instance.dead)
            {
                Plugin.LogWarning("ActorPatcher: Cannot deal damage to dead actors.");
                Plugin.EndLogGroup();

                return false;
            }

            if (__instance.seat != null && __instance.seat.enclosed && !__instance.seat.vehicle.burning)
            {
                Plugin.LogWarning("ActorPatcher: Cannot deal damage to actors inside enclosed vehicles.");
                Plugin.EndLogGroup();

                return false;
            }

            ActorAdditionalData _additionalData = __instance.GetAdditionalData();

            if (_additionalData == null)
            {
                __instance.AddData(new ActorAdditionalData());
                _additionalData = __instance.GetAdditionalData();

                if (_additionalData == null)
                {
                    Plugin.LogWarning($"ActorPatcher: Actor additional data is null! Could not proceed for actor \"{__instance.name}\".");
                    Plugin.EndLogGroup();

                    return false;
                }
            }

            if (_additionalData.CanGetDamaged() == false)
            {
                Plugin.LogWarning($"ActorPatcher: Cannot damage actors more than every {GameConfiguration.actorDamageInvulnerabilityTime} seconds.");
                Plugin.EndLogGroup();

                return false;
            }

            if (__instance.isInvulnerable)
            {
                Plugin.LogWarning("ActorPatcher: Cannot deal damage to invulnerable actors.");
                Plugin.EndLogGroup();

                return false;
            }

            __instance.onTakeDamage.Invoke(__instance, info.sourceActor, info);

            if (__instance.onTakeDamage.isConsumed)
            {
                Plugin.LogWarning("ActorPatcher: On take damage event was consumed and thus damage could not be applied.");
                Plugin.EndLogGroup();

                return false;
            }

            if (_additionalData != null)
            {
                _additionalData.lastTimeHit = Time.time;
            }

            if (__instance == ActorManager.instance.player && info.healthDamage > 0.1f)
            {
                SoundManager.instance.ForcePlaySound(ResourceManager.Instance.HurtAudioClips[Random.Range(0, ResourceManager.Instance.HurtAudioClips.Length)]);
            }

            Plugin.Log($"ActorPatcher: Dealt {info.healthDamage} damage to actor \"{__instance.name}\".");
            Plugin.Log($"ActorPatcher: Projectile landed at position ({info.point.x}; {info.point.y}; {info.point.z}).");

            IngameUi.OnDamageDealt(info, new HitInfo(__instance));

            __instance.controller.ReceivedDamage(false, info.healthDamage, info.balanceDamage, info.point, info.direction, info.impactForce);
            float _healthBeforeDamage = __instance.health;
            __instance.health -= info.healthDamage;

            Plugin.Log($"ActorPatcher: Health amount before damage was dealt = {_healthBeforeDamage}.");
            Plugin.Log($"ActorPatcher: Current health = {__instance.health}.");

            if (__instance.health <= 0f)
            {
                Plugin.Log($"ActorPatcher: Current health is below 0 ({__instance.health}).");

                // Making sure to kill actors that *should* die.
                // Trying to use the private method Die instead of Kill to see if it
                // would fix the invincible actors problem.
                __instance.CallPrivateMethod("Die", new object[] { info, false });
            }
            else if (__instance.ragdoll.IsRagdoll())
            {
                __instance.ApplyRigidbodyForce(info.impactForce);
            }

            int _bloodParticlesAmount = Mathf.Min(Mathf.CeilToInt(info.healthDamage / 10f), 16);
            float _bloodParticlesForce = 0.1f;

            switch (BloodParticle.BLOOD_PARTICLE_SETTING)
            {
                case BloodParticle.BloodParticleType.Reduced:
                    _bloodParticlesAmount /= 2;
                    break;
                case BloodParticle.BloodParticleType.BloodExplosions:
                    _bloodParticlesForce = 0.3f;
                    break;
            }

            Vector3 _bloodParticlesBaseVelocity = Vector3.ClampMagnitude(info.impactForce * _bloodParticlesForce, 5f);

            if (BloodParticle.BLOOD_PARTICLE_SETTING != BloodParticle.BloodParticleType.None)
            {
                for (int i = 0; i < _bloodParticlesAmount; i++)
                {
                    // We use team 1 so all actors have red blood.
                    DecalManager.CreateBloodDrop(info.point, _bloodParticlesBaseVelocity, 1);
                }
            }

            int _finalParticlesCount = Mathf.Clamp(_bloodParticlesAmount, 1, 2);
            for (int _particlesCountIndex = 0; _particlesCountIndex < _finalParticlesCount; _particlesCountIndex++)
            {
                // We use team 1 so all actors have red blood.
                DecalManager.EmitBloodEffect(info.point, _bloodParticlesBaseVelocity, 1);
            }

            bool _shouldTurnToRagdoll = info.isSplashDamage && !__instance.fallenOver && info.balanceDamage > 200f;
            if (__instance.ragdoll.IsRagdoll() && _shouldTurnToRagdoll)
            {
                Vector3 _force = Random.insideUnitSphere.ToGround() * 5f;
                Vector3 _position = __instance.Position();

                Rigidbody[] _rigidbodies = __instance.ragdoll.rigidbodies;
                for (int _rigidbodyIndex = 0; _rigidbodyIndex < _rigidbodies.Length; _rigidbodyIndex++)
                {
                    _rigidbodies[_rigidbodyIndex].AddForceAtPosition(new Vector3(0f, 2f, 0f), _position + _force, ForceMode.VelocityChange);
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

            Plugin.EndLogGroup();
            return false;
        }

        [HarmonyPatch(typeof(Actor), "EnterSeat")]
        [HarmonyPrefix]
        private static void EnterSeatPatch(Actor __instance, ref Seat seat)
        {
            if (seat == null)
            {
                return;
            }

            Vehicle _vehicle = seat.vehicle;

            if (_vehicle == null)
            {
                Plugin.LogError("ActorPatcher: Cannot calculate enter seat actions if the seat's vehicle is null!");
                return;
            }

            if (seat == _vehicle.seats[0])
            {
                if (_vehicle.GetAdditionalData().owner == null)
                {
                    return;
                }

                if (_vehicle.GetAdditionalData().owner != __instance)
                {
                    Seat _leftOverSeat = _vehicle.GetEmptySeat(false);

                    if (_leftOverSeat == null)
                    {
                        seat = null;
                        return;
                    }

                    seat = _leftOverSeat;
                }
            }

            return;
        }
    }
}