using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSettings
{
    public class WeaponProperties
    {
        public string name;
        public float roundPerMinute;
        public float aimInSpeed;
        public Vector3 aimingPosition;
        public Vector3 aimingEuler;
        public float swayAmountMultiplier;
        public float aimInFovOverride;

        public Vector3 visualRecoil;
        public Vector3 visualRotationalRecoil;
        public float visualRecoilReturnSpeed;
        public float upwardRecoil;
        public float rightwardRecoil;
        public float recoilVariance;

        public List<DamagePoint> points = new List<DamagePoint>();

        [Serializable]
        public class DamagePoint
        {
            public float distance;
            public float damage;

            public DamagePoint(float _distance, float _damage)
            {
                distance = _distance;
                damage = _damage;
            }

            public static DamagePoint Parse(string _value)
            {
                _value = _value.Replace(" ", "");
                _value = _value.Replace("{", "").Replace("}", "");
                string[] _values = _value.Split(',');

                return new DamagePoint(float.Parse(_values[0].Replace("distance:", "")), float.Parse(_values[1].Replace("damage:", "")));
            }
        }

        public float bulletVelocity;
        public float damageMultiplier;
        public int magazineCapacity;
        public int magazineCountInReserve;

        public List<SoundRegister> soundRegisters = new List<SoundRegister>();

        [System.Serializable]
        public class SoundRegister
        {
            public string registerName;
            public List<string> sounds = new List<string>();

            [NonSerialized]
            public List<AudioClip> soundClips = new List<AudioClip>();
        }
    }
}