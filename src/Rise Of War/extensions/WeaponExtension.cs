using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Random = UnityEngine.Random;

namespace RiseOfWar
{
    public static class WeaponExtension
    {
        private static readonly ConditionalWeakTable<Weapon, WeaponAdditionalData> _data = new ConditionalWeakTable<Weapon, WeaponAdditionalData>();

        public static void PlayFireSound(this Weapon _weapon)
        {
            if (_weapon == null)
            {
                return;
            }

            if (_weapon.weaponProperties() == null)
            {
                return;
            }

            if (_weapon.weaponProperties().soundRegisters.Count <= 0)
            {
                return;
            }
            
            ((AudioSource)Traverse.Create(_weapon).Field("audio").GetValue()).volume = 0;

            AudioClip[] _clips = _weapon.weaponProperties().soundRegisters[0].clips.ToArray();

            int _i = Random.Range(0, _clips.Length);

            _weapon.GetAdditionalData().source.PlayOneShot(_clips[_i]);
            WeaponPatcher.lastPlayedIndex = _i;
        }

        public static void PlayAimInSound(this Weapon _weapon)
        {
            if (_weapon == null)
            {
                return;
            }

            if (_weapon.weaponProperties() == null)
            {
                return;
            }

            if (_weapon.weaponProperties().soundRegisters.Count <= 1)
            {
                return;
            }

            ((AudioSource)Traverse.Create(_weapon).Field("audio").GetValue()).volume = 0;

            AudioClip[] _clips = _weapon.weaponProperties().soundRegisters[1].clips.ToArray();

            int _i = Random.Range(0, _clips.Length);

            if (_weapon.GetAdditionalData().source == null)
            {
                return;
            }

            _weapon.GetAdditionalData().source.PlayOneShot(_clips[_i]);
            WeaponPatcher.lastPlayedIndex = _i;
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
            _weapon.ExecutePrivateMethod(_methodName, new object[] {});
        }

        public static WeaponAdditionalData GetAdditionalData(this Weapon _weapon)
        {
            return _data.GetOrCreateValue(_weapon);
        }

        public static WeaponXMLProperties weaponProperties(this Weapon _weapon)
        {
            return ResourceManager.GetWeaponProperties(_weapon);
        }

        public static void AddWeaponRecoilAmount(this Weapon _weapon, Vector3 _visualRecoil, Vector3 _rotationalRecoil)
        {
            WeaponPatcher.AddWeaponRecoilAmount(_weapon, _visualRecoil, _rotationalRecoil);
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