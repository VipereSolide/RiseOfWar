using UnityEngine;

using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace RiseOfWar
{
    [XmlRoot(ElementName = "weapon")]
    [Serializable]
    public class WeaponXMLProperties : XMLParamHolderBase
    {
        public static readonly string BULLETS = "bullets";
        public static readonly string MAGAZINES = "magazines";
        public static readonly string SWAY = "sway";
        public static readonly string ROUNDS_PER_MINUTE = "rpm";
        public static readonly string IDLE_POSITION = "idlePosition";
        public static readonly string IDLE_ROTATION = "idleRotation";
        public static readonly string CONE_EXPANSION_PER_SHOT = "coneExpansionPerShot";
        public static readonly string CONE_CONTRACTION_PER_SECOND = "coneContractionPerSecond";

        public static readonly string IS_ADVANCED_RELOAD = "isAdvancedReload";
        public static readonly string DROP_AMMO_WHEN_RELOADING = "dropAmmoWhenReloading";
        public static readonly string USE_MAX_AMMO_PER_RELOAD = "useMaxAmmoPerReload";

        public static readonly string HAS_SPECIAL_PROJECTILES = "hasSpecialProjectiles";

        [XmlElement(ElementName = "name")]
        public string name;

        [XmlElement(ElementName = "damage")]
        public Damage damage;

        [XmlElement(ElementName = "aiming")]
        public Aiming aiming;

        [XmlElement(ElementName = "visualRecoil")]
        public VisualRecoil visualRecoil;

        [XmlElement(ElementName = "cameraRecoil")]
        public CameraRecoil cameraRecoil;

        [XmlElement(ElementName = "projectile")]
        public Projectile projectile;

        public List<SoundRegister> soundRegisters = new List<SoundRegister>();

        [Serializable]
        public class SoundRegister
        {
            public string registerName;

            public List<AudioClip> clips = new List<AudioClip>();
        }

        [XmlRoot(ElementName = "damage")]
        [Serializable]
        public class Damage : XMLParamHolderBase
        {
            public static readonly string SHORT_DISTANCE = "shortDistance";
            public static readonly string LONG_DISTANCE = "longDistance";
            public static readonly string SHORT_DAMAGE = "shortDamage";
            public static readonly string LONG_DAMAGE = "longDamage";
            public static readonly string VEHICLE_DAMAGE = "vehicleDamage";
        }

        [XmlRoot(ElementName = "aiming")]
        [Serializable]
        public class Aiming : XMLParamHolderBase
        {
            public static readonly string POSITION = "position";
            public static readonly string ROTATION = "rotation";
            public static readonly string SPEED = "speed";
            public static readonly string AIM_FOV = "aimFov";
            public static readonly string CONE_EXPANSION_PER_SHOT_AIMED = "coneExpansionPerShotAimed";
        }

        [XmlRoot(ElementName = "visualRecoil")]
        [Serializable]
        public class VisualRecoil : XMLParamHolderBase
        {
            public static readonly string POSITION = "position";
            public static readonly string ROTATION = "rotation";
            public static readonly string SPEED = "speed";
        }

        [XmlRoot(ElementName = "cameraRecoil")]
        [Serializable]
        public class CameraRecoil : XMLParamHolderBase
        {
            public static readonly string UPWARD = "upward";
            public static readonly string RIGHTWARD = "rightward";
            public static readonly string VARIANCE = "variance";
        }

        [XmlRoot(ElementName = "projectile")]
        [Serializable]
        public class Projectile : XMLParamHolderBase
        {
            public static readonly string DAMAGE_MULTIPLIER = "damageMultiplier";
            public static readonly string VELOCITY = "velocity";
        }

    }
}