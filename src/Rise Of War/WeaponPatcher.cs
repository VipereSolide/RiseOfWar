using System;

using Random = UnityEngine.Random;
using UnityEngine;
using HarmonyLib;

namespace RiseOfWar
{
    using WeaponMeshModificator;

    public class WeaponPatcher
    {
        public static int lastPlayedFireAudioClipIndex = 0;

        [HarmonyPatch(typeof(Weapon), "Awake")]
        [HarmonyPostfix]
        private static void PatchAwake(Weapon __instance)
        {
            if (__instance.UserIsAI())
            {
                return;
            }

            if (WeaponRegistry.IsCustomWeapon(__instance) == false)
            {
                return;
            }

            __instance.AddData(new WeaponAdditionalData());

            // Setting the tuck (is the equivalent of sprinting inside the weapon class) parameter of that weapon
            // to a random string (__null__ here) so we don't have the tuck animation playing. This is so we can
            // implement our own procedural running animation later on.
            __instance.SetProperty("TUCK_PARAMETER_HASH", Animator.StringToHash("__null__"));
        }

        [HarmonyPatch(typeof(Weapon), "Start")]
        [HarmonyPostfix]
        private static void PatchStart(Weapon __instance)
        {
            Plugin.Log($"WeaponPatcher: Initializing weapon \"{__instance.name}\"...");

            if (__instance == null)
            {
                return;
            }

            if (WeaponRegistry.IsCustomWeapon(__instance) == false)
            {
                return;
            }

            if (__instance.UserIsAI())
            {
                __instance.configuration = ResourceManager.Instance.GetWeaponConfigurationForBots(__instance, __instance.weaponProperties());
                return;
            }

            // __instance.AddWeaponAnimationSetup();

            CreateAimingAnchor(__instance);
            CreateRecoilAnchor(__instance);

            __instance.configuration = ResourceManager.Instance.GetWeaponConfigurationFromProperties(__instance, __instance.weaponProperties());
            __instance.ResetSetup();

            WeaponAdditionalData _additionalData = __instance.GetAdditionalData();
            WeaponXMLProperties _weaponProperties = __instance.weaponProperties();

            if (_weaponProperties == null || _additionalData == null)
            {
                return;
            }

            try
            {
                _additionalData.modifications = new WeaponModifications
                {
                    possibleModifications = ResourceManager.Instance.GetPossibleWeaponModifications(_weaponProperties.name)
                };

                PlayerUI.instance.SetAllWeaponModificationsActive(false);

                if (_additionalData.modifications.possibleModifications.Count > 0)
                {
                    foreach (RegisteredWeaponModifications _modification in _additionalData.modifications.possibleModifications)
                    {
                        _additionalData.modifications.SetModification(_modification.modification, _modification.modification.GetModificationType());
                    }
                }

                __instance.animator.speed -= _additionalData.modifications.GetModifiedValue(__instance.animator.speed, Modification.Modifications.CHAMBER_TIME);
            }
            catch (Exception _exception)
            {
                Plugin.LogWarning($"WeaponPatcher: Could not load possible modifications for current weapon (\"{__instance.transform.name}\")! " + _exception);
            }

            // __instance.configuration = ResourceManager.Instance.GetConfigurationFromProperties(__instance, _weaponProperties);

            foreach (var _badge in FpsActorController.instance.GetAdditionalData().playerBadges)
            {
                _badge.Execute(__instance);
            }
        }


        [HarmonyPatch(typeof(Weapon), "LateUpdate")]
        [HarmonyPrefix]
        private static bool LateUpdatePatch(Weapon __instance)
        {
            if (__instance != ReferenceManager.player.activeWeapon)
            {
                return false;
            }

            foreach (WeaponMeshModification _modification in WeaponMeshModificationRegistry.GetWeaponMeshModifications(__instance))
            {
                WeaponMeshModificationReader.ApplyWeaponMeshModification(__instance, _modification);
            }

            return false;
        }

        private static void CreateAimingAnchor(Weapon __instance)
        {
            if (__instance == null)
            {
                Plugin.LogError("WeaponPatcher: Cannot initialize aiming anchor for null weapon!");
                return;
            }

            Transform _aimingAnchor = new GameObject($"Aiming Anchor ({__instance.name})").transform;
            _aimingAnchor.SetParent(__instance.transform.parent);
            _aimingAnchor.localPosition = Vector3.zero;
            _aimingAnchor.localRotation = Quaternion.identity;

            __instance.GetAdditionalData().aimingAnchor = _aimingAnchor;
            __instance.transform.SetParent(_aimingAnchor.transform, true);

            Plugin.Log($"WeaponPatcher: Successfully initialized aiming anchor for weapon \"{__instance.name}\"!");
        }

        private static void CreateRecoilAnchor(Weapon __instance)
        {
            if (__instance == null)
            {
                Plugin.LogError("WeaponPatcher: Cannot initialize recoil anchor for null weapon!");
                return;
            }

            Transform _recoilAnchor = new GameObject($"Recoil Anchor ({__instance.name})").transform;
            _recoilAnchor.SetParent(__instance.transform.parent);
            _recoilAnchor.localPosition = Vector3.zero;
            _recoilAnchor.localRotation = Quaternion.identity;

            __instance.GetAdditionalData().recoilAnchor = _recoilAnchor;
            __instance.transform.SetParent(_recoilAnchor.transform, true);

            Plugin.Log($"WeaponPatcher: Successfully initialized recoil anchor for weapon \"{__instance.name}\"!");
        }

        [HarmonyPatch(typeof(Weapon), "Update")]
        [HarmonyPostfix]
        private static void PatchUpdate(Weapon __instance)
        {
            if (__instance == null || __instance.UserIsAI() || WeaponRegistry.IsCustomWeapon(__instance) == false)
            {
                return;
            }

            Actor _player = ActorManager.instance.player;

            if (_player == null || _player.activeWeapon != __instance)
            {
                return;
            }

            foreach (Lua.ScriptedBehaviour _scriptedBehaviour in __instance.GetComponents<Lua.ScriptedBehaviour>())
            {
                if (_scriptedBehaviour.enabled == false)
                {
                    continue;
                }

                Plugin.Log($"WeaponPatcher: Found scripted behaviour on weapon \"{WeaponRegistry.GetRealName(__instance)}\"...");
                _scriptedBehaviour.enabled = false;
            }

            __instance.HandleCurrentConefire();
            __instance.HandleAimingAnchor();
            __instance.HandleRecoil();

            __instance.HandlePlayerUI();
        }

        [HarmonyPatch(typeof(Weapon), "SpawnProjectile")]
        [HarmonyPrefix]
        private static bool PatchSpawnProjectile(Weapon __instance, ref Projectile __result, ref Vector3 direction, ref Vector3 muzzlePosition)
        {
            // I don't want the AIs to shoot from the player's eyes.
            if (__instance.UserIsAI())
            {
                return true;
            }

            // We don't want to modify the mounted weapons/turrets because otherwise they
            // tend to behave weirdly, shooting behind you or even at you.
            if (__instance is MountedWeapon || __instance is MountedTurret)
            {
                return true;
            }

            // Setting the origin of the bullet and it's direction to the center of the player's camera so it actually
            // shoots straight (not from the weapon's muzzle).
            direction = FpsActorController.instance.fpParent.fpCameraParent.transform.forward;
            muzzlePosition = FpsActorController.instance.fpParent.fpCameraParent.transform.position;

            float _currentSpreadMagnitude = __instance.GetAdditionalData().currentConefire + FpsActorController.instance.GetAdditionalData().stamina / 10;
            Quaternion _rotation = Quaternion.LookRotation(direction) * Quaternion.AngleAxis(-_currentSpreadMagnitude, Random.insideUnitSphere);

            /*
            GameObject _tester = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _tester.transform.localScale = new Vector3(0.1f, 0.1f, 0.5f);
            _tester.transform.position = muzzlePosition;
            _tester.transform.rotation = _rotation;
            */

            Projectile _projectile = ProjectilePoolManager.InstantiateProjectile(__instance.configuration.projectilePrefab, muzzlePosition, _rotation);
            _projectile.source = __instance.user;
            _projectile.sourceWeapon = __instance;

            if (__instance.targetTracker != null)
            {
                TargetSeekingMissile _targetSeekingMissile = _projectile as TargetSeekingMissile;
                if (_targetSeekingMissile != null)
                {
                    _targetSeekingMissile.ClearTrackers();

                    if (__instance.targetTracker.HasVehicleTarget() && __instance.targetTracker.TargetIsLocked())
                    {
                        _targetSeekingMissile.SetTrackerTarget(__instance.targetTracker.vehicleTarget);
                    }
                    else if (__instance.targetTracker.HasPointTarget())
                    {
                        _targetSeekingMissile.SetPointTargetProvider(__instance.targetTracker);
                    }
                }
            }

            if (!__instance.IsMountedWeapon() && (__instance.UserIsPlayer() || !__instance.hasAnyAttachedColliders))
            {
                _projectile.performInfantryInitialMuzzleTravel = true;

                Vector3 _lhs = muzzlePosition - __instance.user.controller.WeaponParent().position;
                _projectile.initialMuzzleTravelDistance = Vector3.Dot(_lhs, direction);
            }
            else
            {
                _projectile.performInfantryInitialMuzzleTravel = false;
                _projectile.initialMuzzleTravelDistance = 0f;
            }

            _projectile.StartTravelling();

            __result = _projectile;
            return false;
        }

        [HarmonyPatch(typeof(Weapon), "SetAiming")]
        [HarmonyPrefix]
        private static bool PatchSetAiming(Weapon __instance, bool aiming)
        {
            // If the user is a normal AI, execute the default method instead.
            if (__instance.UserIsAI())
            {
                DefaultSetAiming(__instance, aiming);
                return false;
            }

            WeaponAdditionalData _additionalData = __instance.GetAdditionalData();
            _additionalData.wasAiming = __instance.aiming;

            // If the user is indeed the player, we can skip the animator part as
            // we will add our own aiming and posing system.
            Weapon _activeSubWeapon = __instance.GetActiveSubWeapon();

            if (_activeSubWeapon != __instance)
            {
                _activeSubWeapon.SetAiming(aiming);
                return false;
            }

            if (aiming && _additionalData.wasAiming == false)
            {
                __instance.PlayAimInSound();
            }

            __instance.aiming = aiming;
            return false;
        }

        private static void DefaultSetAiming(Weapon __instance, bool aiming)
        {
            Weapon _activeSubWeapon = __instance.GetActiveSubWeapon();

            if (_activeSubWeapon != __instance)
            {
                _activeSubWeapon.SetAiming(aiming);
                return;
            }

            __instance.aiming = aiming;

            if (__instance.activeAnimator())
            {
                __instance.animator.SetBool(Weapon.AIM_PARAMETER_HASH, aiming);
            }
        }

        [HarmonyPatch(typeof(Weapon), "Shoot", new Type[] { typeof(Vector3), typeof(bool) })]
        [HarmonyPrefix]
        private static bool ShootPatch(Weapon __instance, Vector3 direction, bool useMuzzleDirection)
        {
            if (__instance == null)
            {
                return true;
            }

            WeaponXMLProperties _weaponProperties = __instance.weaponProperties();
            WeaponAdditionalData _additionalData = __instance.GetAdditionalData();

            if (WeaponRegistry.IsCustomWeapon(__instance) == false || __instance.UserIsAI() || _weaponProperties == null)
            {
                return true;
            }

            if (__instance.aiming == false)
            {
                float _coneExpansion = _weaponProperties.GetFloat(WeaponXMLProperties.CONE_EXPANSION_PER_SHOT);
                _additionalData.currentConefire += _coneExpansion;

                Plugin.Log("WeaponPatcher: Received conefire expansion data = " + _coneExpansion);
            }

            Vector3 _fireDirection = PlayerFpParent.instance.fpCamera.transform.forward;

            float _lastFiredTimestamp = (float)Traverse.Create(__instance).Field("lastFiredTimestamp").GetValue();

            if (_lastFiredTimestamp + __instance.configuration.cooldown > Time.time || __instance.ammo <= 0)
            {
                return false;
            }

            __instance.PlayFireSound();

            byte _currentMuzzleIndex = (byte)Traverse.Create(__instance).Field("currentMuzzleIndex").GetValue();

            __instance.onFire.Invoke();
            if (__instance.onFireScriptable != null && __instance.onFireScriptable.isConsumed)
            {
                return false;
            }

            if (__instance.configuration.loud)
            {
                __instance.user.Highlight(4f);
            }

            Traverse.Create(__instance).Field("lastFiredTimestamp").SetValue(Time.time);

            if (__instance.activeAnimator())
            {
                if (!__instance.configuration.fireFromAllMuzzles && __instance.configuration.muzzles.Length > 1)
                {
                    __instance.animator.SetInteger(Weapon.MUZZLE_PARAMETER_HASH, _currentMuzzleIndex);
                }

                __instance.animator.SetTrigger(Weapon.FIRE_PARAMETER_HASH);
            }

            if (__instance.configuration.fireFromAllMuzzles)
            {
                for (int i = 0; i < __instance.configuration.muzzles.Length; i++)
                {
                    __instance.ExecutePrivateMethod("FireFromMuzzle", new object[] { i, _fireDirection, useMuzzleDirection });
                }
            }
            else if (__instance.configuration.muzzles.Length != 0)
            {
                __instance.ExecutePrivateMethod("FireFromMuzzle", new object[] { _currentMuzzleIndex, _fireDirection, useMuzzleDirection });
            }

            if (__instance.ammo != -1)
            {
                __instance.ammo--;
            }

            __instance.OnAmmoChanged();

            if (__instance.reflectionVolume > 0f && __instance.reflectionSound != Weapon.ReflectionSound.None)
            {
                GameManager.PlayReflectionSound(
                    __instance.UserIsPlayer(),
                    __instance.configuration.auto,
                    __instance.reflectionSound,
                    __instance.reflectionVolume,
                    __instance.transform.position + __instance.user.controller.FacingDirection() * 30f
                );
            }

            __instance.ExecutePrivateMethod("UpdateSoundOutputGroup", new object[] { });

            if (!__instance.configuration.auto)
            {
                Traverse.Create(__instance).Field("hasFiredSingleRoundThisTrigger").SetValue(true);
            }

            if (__instance.configuration.loud && __instance.user != null && !__instance.IsMeleeWeapon())
            {
                try
                {
                    MuzzleFlashManager.RegisterMuzzleFlash(__instance.configuration.muzzles[_currentMuzzleIndex].position, __instance.user, __instance.IsMountedWeapon());
                }
                catch (Exception _exception)
                {
                    Plugin.LogError("WeaponPatcher: " + _exception);
                }
            }

            __instance.ExecutePrivateMethod("NextMuzzle", new object[] { });

            __instance.ExecutePrivateMethod("ApplyRecoil");
            __instance.AddWeaponRecoilAmount(_weaponProperties.visualRecoil.GetVector3(WeaponXMLProperties.VisualRecoil.POSITION), _weaponProperties.visualRecoil.GetVector3(WeaponXMLProperties.VisualRecoil.ROTATION));

            float _upwardRecoil = _weaponProperties.cameraRecoil.GetFloat(WeaponXMLProperties.CameraRecoil.UPWARD);
            float _rightwardRecoil = _weaponProperties.cameraRecoil.GetFloat(WeaponXMLProperties.CameraRecoil.RIGHTWARD);
            float _recoilVariance = _weaponProperties.cameraRecoil.GetFloat(WeaponXMLProperties.CameraRecoil.VARIANCE);

            if (__instance.user.controller.Prone())
            {
                _upwardRecoil /= 2;
                _recoilVariance /= 4;
                _rightwardRecoil /= 4;
            }
            else if (__instance.user.controller.Crouch())
            {
                _upwardRecoil /= 1.25f;
                _recoilVariance /= 2;
                _rightwardRecoil /= 2;
            }

            PlayerFpParent.instance.AddCameraRecoil(_upwardRecoil, _rightwardRecoil, _recoilVariance);

            if (__instance.aiming)
            {
                float _coneExpansion = _weaponProperties.aiming.GetFloat(WeaponXMLProperties.Aiming.CONE_EXPANSION_PER_SHOT_AIMED);
                __instance.GetAdditionalData().currentConefire += _coneExpansion;

                Plugin.Log("WeaponPatcher: Received conefire expansion data (aimed) = " + _coneExpansion);
            }

            __instance.PlayAnimation(WeaponAnimationType.Fire, true);

            // Animation _animation = __instance.GetAdditionalData().animation;
            // Plugin.Log($"WeaponPatcher: Playing fire animation named \"{__instance.GetAdditionalData().fireAnimationName}\"");
            // Plugin.Log($"WeaponPatcher: Is animation legacy = {_animation.clip.legacy}");

            return false;
        }

        [HarmonyPatch(typeof(Weapon), "Reload")]
        [HarmonyPostfix]
        private static void ReloadPatch(Weapon __instance)
        {
            __instance.PlayAnimation(WeaponAnimationType.Reload, true);
            PlayerUI.instance.SetWeaponCustomDisplayName("RELOADING");
        }

        [HarmonyPatch(typeof(Weapon), "ReloadDone")]
        [HarmonyPostfix]
        private static void ReloadDonePatch(Weapon __instance)
        {
            __instance.PlayAnimation(WeaponAnimationType.Idle, true);
        }

        [HarmonyPatch(typeof(Weapon), "Unholster")]
        [HarmonyPostfix]
        private static void UnholsterPatch(Weapon __instance)
        {
            try
            {
            __instance.PlayAnimation(WeaponAnimationType.Draw, true);
            }
            catch { }
        }

        [HarmonyPatch(typeof(Weapon), "ApplyRecoil")]
        [HarmonyPrefix]
        private static bool ApplyRecoil(Weapon __instance)
        {
            if (WeaponRegistry.IsCustomWeapon(__instance) == false)
            {
                return true;
            }

            return false;
        }
    }
}