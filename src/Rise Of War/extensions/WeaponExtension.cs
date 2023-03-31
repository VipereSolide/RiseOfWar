using System.Runtime.CompilerServices;
using System.Reflection;
using System;

using Random = UnityEngine.Random;
using UnityEngine;

namespace RiseOfWar
{
    using Events;

    public static class WeaponExtension
    {
        private static ConditionalWeakTable<Weapon, WeaponAdditionalData> _data = new ConditionalWeakTable<Weapon, WeaponAdditionalData>();

        public static void ResetSetup(this Weapon weapon, bool fullReset = false)
        {
            weapon.ammo = weapon.configuration.ammo;
            weapon.spareAmmo = weapon.configuration.spareAmmo;
        }

        public static void HandleCurrentConefire(this Weapon weapon)
        {
            if (weapon == null)
            {
                return;
            }

            WeaponAdditionalData _additionalData = weapon.GetAdditionalData();
            WeaponXMLProperties _weaponProperties = weapon.weaponProperties();

            if (_additionalData == null || _weaponProperties == null)
            {
                return;
            }

            if (_additionalData.currentConefire > 0)
            {
                _additionalData.currentConefire -= Time.deltaTime * _weaponProperties.GetFloat(WeaponXMLProperties.CONE_CONTRACTION_PER_SECOND);

                if (_additionalData.currentConefire < 0)
                {
                    _additionalData.currentConefire = 0;
                }
            }
        }

        public static void HandleAimingAnchor(this Weapon weapon)
        {
            if (weapon == null)
            {
                Plugin.LogError("WeaponExtension: Cannot handle aiming anchor for null weapon!");
                return;
            }

            if (!WeaponRegistry.IsCustomWeapon(weapon))
            {
                Plugin.LogError("WeaponExtension: Cannot handle aiming anchor for no custom weapon!");
                return;
            }

            WeaponAdditionalData _additionalData = weapon.GetAdditionalData();
            WeaponXMLProperties _weaponProperties = weapon.weaponProperties();

            if (_additionalData == null || _weaponProperties == null)
            {
                return;
            }

            Transform _aimingAnchor = _additionalData.aimingAnchor;

            if (_aimingAnchor == null)
            {
                Plugin.LogError("Weapon Patcher: Cannot handle null aiming anchor! Please run the aiming anchor initialization process again.");
                return;
            }

            Vector3 _targetPosition;
            Vector3 _targetRotation;

            _targetPosition = _weaponProperties.GetVector3(WeaponXMLProperties.IDLE_POSITION);
            _targetRotation = _weaponProperties.GetVector3(WeaponXMLProperties.IDLE_ROTATION);

            if (weapon.aiming)
            {
                _targetPosition = _weaponProperties.aiming.GetVector3(WeaponXMLProperties.Aiming.POSITION);
                _targetRotation = _weaponProperties.aiming.GetVector3(WeaponXMLProperties.Aiming.ROTATION);
            }

            float _aimingSpeed = _weaponProperties.aiming.GetFloat(WeaponXMLProperties.Aiming.SPEED);

            _aimingAnchor.transform.localPosition = Vector3.Lerp(_aimingAnchor.transform.localPosition, _targetPosition, Time.deltaTime * _aimingSpeed);
            _aimingAnchor.transform.localRotation = Quaternion.Slerp(_aimingAnchor.transform.localRotation, Quaternion.Euler(_targetRotation), Time.deltaTime * _aimingSpeed);
        }

        public static void HandleRecoil(this Weapon weapon)
        {
            if (weapon == null || !WeaponRegistry.IsCustomWeapon(weapon))
            {
                return;
            }

            WeaponAdditionalData _additionalData = weapon.GetAdditionalData();
            WeaponXMLProperties _weaponProperties = weapon.weaponProperties();

            if (_additionalData == null || _weaponProperties == null)
            {
                return;
            }

            float _visualRecoilSpeed = _weaponProperties.visualRecoil.GetFloat(WeaponXMLProperties.VisualRecoil.SPEED);

            _additionalData.recoilAnchor.localPosition = Vector3.Lerp(_additionalData.recoilAnchor.localPosition, Vector3.zero, Time.deltaTime * _visualRecoilSpeed);
            _additionalData.recoilAnchor.localRotation = Quaternion.Slerp(_additionalData.recoilAnchor.localRotation, Quaternion.identity, Time.deltaTime * _visualRecoilSpeed);
        }

        public static void HandlePlayerUI(this Weapon weapon)
        {
            PlayerUI _playerUI = PlayerUI.instance;
            _playerUI.SetCurrentBulletAmount(weapon.ammo);
            _playerUI.SetCurrentAmmoInReserveAmount(weapon.spareAmmo);

            if (weapon.reloading)
            {
                return;
            }

            if (weapon.GetAdditionalData().hasCustomDisplayName == false)
            {
                _playerUI.HideCustomDisplayName();
            }
            else
            {
                _playerUI.SetWeaponCustomDisplayName(weapon.transform.name);
            }
        }

        public static void AddWeaponRecoilAmount(this Weapon weapon, Vector3 _visualRecoil, Vector3 _rotationalRecoil)
        {
            if (weapon == null)
            {
                Plugin.LogError("WeaponPatcher: Cannot apply recoil amount for null weapon!");
                return;
            }

            WeaponAdditionalData _additionalData = weapon.GetAdditionalData();

            if (!WeaponRegistry.IsCustomWeapon(weapon) || weapon.UserIsAI() || weapon.weaponProperties() == null || _additionalData == null)
            {
                return;
            }

            _additionalData.recoilAnchor.localPosition = _visualRecoil;
            _additionalData.recoilAnchor.localRotation = Quaternion.Euler(_rotationalRecoil);
        }

        public static void PlayFireSound(this Weapon _weapon)
        {
            if (_weapon == null)
            {
                return;
            }

            WeaponXMLProperties _weaponProperties = _weapon.weaponProperties();

            if (_weaponProperties == null || _weaponProperties.soundRegisters.Count <= 0)
            {
                return;
            }

            // ((AudioSource)Traverse.Create(_weapon).Field("audio").GetValue()).volume = 0;

            AudioClip[] _fireAudioClips = _weaponProperties.soundRegisters[0].clips.ToArray();
            int _randomIndex = Random.Range(0, _fireAudioClips.Length);

            // _weapon.GetAdditionalData().source.PlayOneShot(_clips[_i]);
            SoundManager.instance.ForcePlaySound(_fireAudioClips[_randomIndex]);

            WeaponPatcher.lastPlayedFireAudioClipIndex = _randomIndex;
        }

        public static void PlayAimInSound(this Weapon _weapon)
        {
            if (_weapon == null)
            {
                return;
            }

            WeaponXMLProperties _weaponProperties = _weapon.weaponProperties();

            if (_weaponProperties == null || _weaponProperties.soundRegisters.Count <= 0)
            {
                return;
            }

            // ((AudioSource)Traverse.Create(_weapon).Field("audio").GetValue()).volume = 0;

            AudioClip[] _aimingAudioClips = _weaponProperties.soundRegisters[1].clips.ToArray();
            int _randomIndex = Random.Range(0, _aimingAudioClips.Length);

            // if (_weapon.GetAdditionalData().source == null)
            // {
            //     return;
            // }

            // _weapon.GetAdditionalData().source.PlayOneShot(_clips[_i]);
            SoundManager.instance.ForcePlaySound(_aimingAudioClips[_randomIndex]);
            // WeaponPatcher.lastPlayedFireAudioClipIndex = _randomIndex;
        }

        public static bool activeAnimator(this Weapon _weapon)
        {
            return _weapon.animator != null && _weapon.animator.isActiveAndEnabled;
        }

        public static void ExecutePrivateMethod(this Weapon _weapon, string _methodName, object[] _args)
        {
            MethodInfo _privateMethod = _weapon.GetType().GetMethod(_methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            _privateMethod.Invoke(_weapon, _args);
        }

        public static void ExecutePrivateMethod(this Weapon _weapon, string _methodName)
        {
            _weapon.ExecutePrivateMethod(_methodName, new object[] { });
        }

        public static WeaponAdditionalData GetAdditionalData(this Weapon _weapon)
        {
            if (_weapon == null)
            {
                return null;
            }

            if (_data == null)
            {
                _data = new ConditionalWeakTable<Weapon, WeaponAdditionalData>();
            }

            return _data.GetOrCreateValue(_weapon);
        }

        public static WeaponXMLProperties weaponProperties(this Weapon _weapon)
        {
            return ResourceManager.GetWeaponProperties(_weapon);
        }

        public static void AddData(this Weapon _weapon, WeaponAdditionalData _value)
        {
            try
            {
                _value.isCustomWeapon = true;
                _data.Add(_weapon, _value);
            }
            catch (Exception _ex)
            {
                Plugin.LogError($"WeaponExtension: Couldn't add data to weapon \"{_weapon.name}\": " + _ex);
            }
        }
    }
}