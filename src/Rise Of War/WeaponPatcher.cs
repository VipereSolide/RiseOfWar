using System;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RiseOfWar
{
    public class WeaponPatcher
    {
        [HarmonyPatch(typeof(Weapon), "Awake")]
        [HarmonyPostfix]
        private static void PatchAwake(Weapon __instance)
        {
            if (__instance.UserIsAI())
            {
                return;
            }

            if (!WeaponRegistry.IsCustomWeapon(__instance))
            {
                return;
            }

            // Setting the tuck (is the equivalent of sprinting inside the weapon class) parameter of that weapon
            // to a random string (__null__ here) so we don't have the tuck animation playing. This is so we can
            // implement our own procedural running animation later on.
            Traverse.Create(typeof(Weapon)).Field("TUCK_PARAMETER_HASH").SetValue(Animator.StringToHash("__null__"));
        }

        [HarmonyPatch(typeof(Weapon), "Start")]
        [HarmonyPostfix]
        private static void PatchStart(Weapon __instance)
        {
            Plugin.Log($"WeaponPatcher: Initializing weapon \"{__instance.name.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\\", "\\\\").Replace(" ", "_")}\"...");

            if (__instance.UserIsAI())
            {
                return;
            }

            if (!WeaponRegistry.IsCustomWeapon(__instance))
            {
                return;
            }

            CreateAimingAnchor(__instance);
            CreateAudioSource(__instance);
            CreateRecoilAnchor(__instance);

            if (__instance == null)
            {
                return;
            }

            __instance.configuration = ResourceManager.Instance.GetConfigurationFromProperties(__instance, __instance.weaponProperties());

            if (__instance.weaponProperties() == null || __instance.GetAdditionalData() == null)
            {
                return;
            }

            try
            {
                __instance.GetAdditionalData().modifications = new WeaponModifications();
                __instance.GetAdditionalData().modifications.possibleModifications = ResourceManager.Instance.GetPossibleWeaponModifications(__instance.weaponProperties().name);
            
                if (__instance.GetAdditionalData().modifications.possibleModifications.Count > 0)
                {
                    PlayerUI.instance.SetAllWeaponModificationsActive(false);
                    
                    foreach(RegisteredWeaponModifications _mod in __instance.GetAdditionalData().modifications.possibleModifications)
                    {
                        PlayerUI.instance.SetWeaponModificationActive(_mod.modification.GetModificationType(), true);
                        __instance.GetAdditionalData().modifications.SetModification(_mod.modification, _mod.modification.GetModificationType());
                    }
                }

                __instance.animator.speed -= __instance.GetAdditionalData().modifications.GetModifiedValue(__instance.animator.speed, Modification.Modifications.CHAMBER_TIME);
            }
            catch (Exception _exception)
            {
                Plugin.LogWarning($"WeaponPatcher: Could not load possible modifications for current weapon (\"{__instance.transform.name}\")! " + _exception);
            }

            __instance.configuration = ResourceManager.Instance.GetConfigurationFromProperties(__instance, __instance.weaponProperties());
        }

        private static void CreateAudioSource(Weapon __instance)
        {
            GameObject _audioSource = new GameObject("Audio Source");
            _audioSource.transform.SetParent(__instance.transform, false);
            AudioSource _source = _audioSource.AddComponent<AudioSource>();
            _source.outputAudioMixerGroup = ((AudioSource)Traverse.Create(__instance).Field("audio").GetValue()).outputAudioMixerGroup;
            _source.loop = false;
            _source.playOnAwake = false;
            __instance.GetAdditionalData().source = _source;
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

            __instance.AddData(new WeaponAdditionalData(_aimingAnchor, true));
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
            if (__instance.UserIsAI())
            {
                return;
            }

            if (!WeaponRegistry.IsCustomWeapon(__instance))
            {
                return;
            }

            if (__instance.GetAdditionalData().currentConefire > 0)
            {
                __instance.GetAdditionalData().currentConefire -= Time.deltaTime * __instance.weaponProperties().GetFloat(WeaponXMLProperties.CONE_CONTRACTION_PER_SECOND);
                
                if (__instance.GetAdditionalData().currentConefire < 0)
                {
                    __instance.GetAdditionalData().currentConefire = 0;
                }
            }

            HandleAimingAnchor(__instance);
            HandleRecoil(__instance);

            if (__instance == null || ActorManager.instance.player == null || ActorManager.instance.player.activeWeapon != __instance)
            {
                return;
            }

            PlayerUI.instance.SetCurrentBulletAmount(__instance.ammo);
            PlayerUI.instance.SetCurrentAmmoInReserveAmount(__instance.spareAmmo);

            if (__instance.reloading == false)
            {
                if (__instance.GetAdditionalData().hasCustomDisplayName == false)
                {
                    PlayerUI.instance.HideCustomDisplayName();
                }
                else
                {
                    PlayerUI.instance.SetWeaponCustomDisplayName(__instance.transform.name);
                }
            }
        }

        private static void HandleAimingAnchor(Weapon __instance)
        {
            if (!WeaponRegistry.IsCustomWeapon(__instance))
            {
                Plugin.LogError("WeaponPatcher: Cannot handle aiming anchor for no custom weapon!");
                return;
            }

            if (__instance == null)
            {
                Plugin.LogError("WeaponPatcher: Cannot handle aiming anchor for null weapon!");
                return;
            }

            if (__instance.weaponProperties() == null)
            {
                return;
            }

            Transform _aimingAnchor = __instance.GetAdditionalData().aimingAnchor;

            if (_aimingAnchor == null)
            {
                Plugin.LogError("Weapon Patcher: Cannot handle null aiming anchor! Please run the aiming anchor initialization process again.");
                return;
            }

            Vector3 _targetPosition = Vector3.zero;
            Vector3 _targetRotation = Vector3.zero;

            _targetPosition = __instance.weaponProperties().GetVector3(WeaponXMLProperties.IDLE_POSITION);
            _targetRotation = __instance.weaponProperties().GetVector3(WeaponXMLProperties.IDLE_ROTATION);

            if (__instance.aiming)
            {
                _targetPosition = __instance.weaponProperties().aiming.GetVector3(WeaponXMLProperties.Aiming.POSITION);
                _targetRotation = __instance.weaponProperties().aiming.GetVector3(WeaponXMLProperties.Aiming.ROTATION);
            }

            _aimingAnchor.transform.localPosition = Vector3.Lerp(_aimingAnchor.transform.localPosition, _targetPosition, Time.deltaTime * __instance.weaponProperties().aiming.GetFloat(WeaponXMLProperties.Aiming.SPEED));
            _aimingAnchor.transform.localRotation = Quaternion.Slerp(_aimingAnchor.transform.localRotation, Quaternion.Euler(_targetRotation), Time.deltaTime * __instance.weaponProperties().aiming.GetFloat(WeaponXMLProperties.Aiming.SPEED));
        }

        private static void HandleRecoil(Weapon __instance)
        {
            if (!WeaponRegistry.IsCustomWeapon(__instance) || __instance.UserIsAI() || __instance.weaponProperties() == null)
            {
                return;
            }

            __instance.GetAdditionalData().recoilAnchor.localPosition = Vector3.Lerp(__instance.GetAdditionalData().recoilAnchor.localPosition, Vector3.zero, Time.deltaTime * __instance.weaponProperties().visualRecoil.GetFloat(WeaponXMLProperties.VisualRecoil.SPEED));
            __instance.GetAdditionalData().recoilAnchor.localRotation = Quaternion.Slerp(__instance.GetAdditionalData().recoilAnchor.localRotation, Quaternion.identity, Time.deltaTime * __instance.weaponProperties().visualRecoil.GetFloat(WeaponXMLProperties.VisualRecoil.SPEED));
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

            float _currentSpreadMagnitude = __instance.GetAdditionalData().currentConefire;
            Quaternion _rotation = Quaternion.LookRotation(direction) * Quaternion.AngleAxis(-_currentSpreadMagnitude, Random.insideUnitSphere);
            
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

            __instance.GetAdditionalData().wasAiming = __instance.aiming;

            // If the user is indeed the player, we can skip the animator part as
            // we will add our own aiming and posing system.
            Weapon _activeSubWeapon = __instance.GetActiveSubWeapon();

            if (_activeSubWeapon != __instance)
            {
                _activeSubWeapon.SetAiming(aiming);
                return false;
            }

            if (aiming && !__instance.GetAdditionalData().wasAiming)
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
            if (!WeaponRegistry.IsCustomWeapon(__instance) || __instance.UserIsAI() || __instance.weaponProperties() == null)
            {
                return true;
            }
            else
            {
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
                    GameManager.PlayReflectionSound(__instance.UserIsPlayer(), __instance.configuration.auto, __instance.reflectionSound, __instance.reflectionVolume, __instance.transform.position + __instance.user.controller.FacingDirection() * 30f);
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

                /*float num3 = __instance.configuration.GetConeExpansionPerShot();
                float num4 = __instance.configuration.GetMaximumConeExpansion();

                if (__instance.aiming)
                {
                    num3 = __instance.configuration.GetConeExpansionPerShotAimed();
                    num4 = __instance.configuration.GetMaximumConeExpansionAimed();
                }
                if (__instance._currentConeExpansion < num4)
                {
                    __instance._currentConeExpansion = Mathf.Min(__instance._currentConeExpansion + num3, num4);
                }*/

                __instance.ExecutePrivateMethod("ApplyRecoil");
                __instance.AddWeaponRecoilAmount(__instance.weaponProperties().visualRecoil.GetVector3(WeaponXMLProperties.VisualRecoil.POSITION), __instance.weaponProperties().visualRecoil.GetVector3(WeaponXMLProperties.VisualRecoil.ROTATION));
                PlayerFpParent.instance.AddCameraRecoil(__instance.weaponProperties().cameraRecoil.GetFloat(WeaponXMLProperties.CameraRecoil.UPWARD), __instance.weaponProperties().cameraRecoil.GetFloat(WeaponXMLProperties.CameraRecoil.RIGHTWARD), __instance.weaponProperties().cameraRecoil.GetFloat(WeaponXMLProperties.CameraRecoil.VARIANCE));

                if (__instance.aiming)
                {
                    Plugin.Log("WeaponPatcher: Received conefire expansion data = " + __instance.weaponProperties().aiming.GetFloat(WeaponXMLProperties.Aiming.CONE_EXPANSION_PER_SHOT_AIMED));
                    __instance.GetAdditionalData().currentConefire += __instance.weaponProperties().aiming.GetFloat(WeaponXMLProperties.Aiming.CONE_EXPANSION_PER_SHOT_AIMED);
                }
                else
                {
                    Plugin.Log("WeaponPatcher: Received conefire expansion data = " + __instance.weaponProperties().GetFloat(WeaponXMLProperties.CONE_EXPANSION_PER_SHOT));
                    __instance.GetAdditionalData().currentConefire += __instance.weaponProperties().GetFloat(WeaponXMLProperties.CONE_EXPANSION_PER_SHOT);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(Weapon), "Shoot", new Type[] { typeof(Vector3), typeof(bool) })]
        [HarmonyPostfix]
        private static void ShootPatchAfter(Weapon __instance)
        {
        }


        [HarmonyPatch(typeof(Weapon), "Reload")]
        [HarmonyPostfix]
        private static void ReloadPatch(Weapon __instance)
        {
            PlayerUI.instance.SetWeaponCustomDisplayName("RELOADING");
        }

        [HarmonyPatch(typeof(Weapon), "StartAdvancedReload")]
        [HarmonyPrefix]
        private static void StartAdvancedReloadPatch(Weapon __instance)
        {
        }

        [HarmonyPatch(typeof(Weapon), "AdvancedReloadNextMotion")]
        [HarmonyPrefix]
        private static void AdvancedReloadNextMotionPatch(Weapon __instance)
        {
        }

        [HarmonyPatch(typeof(Weapon), "EndAdvancedReload")]
        [HarmonyPrefix]
        private static void EndAdvancedReloadPatch(Weapon __instance)
        {
        }

        public static int lastPlayedIndex = 0;

        public static void AddWeaponRecoilAmount(Weapon __instance, Vector3 _visualRecoil, Vector3 _rotationalRecoil)
        {
            if (__instance == null)
            {
                Plugin.LogError("WeaponPatcher: Cannot apply recoil amount for null weapon!");
                return;
            }

            if (!WeaponRegistry.IsCustomWeapon(__instance) || __instance.UserIsAI() || __instance.weaponProperties() == null)
            {
                return;
            }

            __instance.GetAdditionalData().recoilAnchor.localPosition = _visualRecoil;
            __instance.GetAdditionalData().recoilAnchor.localRotation = Quaternion.Euler(_rotationalRecoil);
        }

        [HarmonyPatch(typeof(Weapon), "ApplyRecoil")]
        [HarmonyPrefix]
        private static bool ApplyRecoil(Weapon __instance)
        {
            if (!WeaponRegistry.IsCustomWeapon(__instance))
            {
                return true;
            }

            return false;
        }
    }
}