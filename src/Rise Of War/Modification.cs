using System.Xml.Serialization;
using System;

namespace RiseOfWar
{
    [XmlRoot(ElementName = "weaponModification")]
    [Serializable]
    public class Modification : XMLParamHolderBase
    {
        [XmlElement(ElementName = "name")]
        public string name;

        [XmlElement(ElementName = "weaponName")]
        public string weaponName;

        [XmlElement(ElementName = "modificationType")]
        public string modificationType;

        [XmlElement(ElementName = "modifications")]
        public Modifications modifications;

        public static readonly string COST_PER_SHOT = "costPerShot";
        public static readonly string FIELD_MAINTENANCE = "fieldMaintenance";
        public static readonly string BUY_COST = "buyCost";

        public WeaponModificationType GetModificationType()
        {
            return (WeaponModificationType)Enum.Parse(typeof(WeaponModificationType), modificationType);
        }

        [XmlRoot(ElementName = "modifications")]
        [Serializable]
        public class Modifications : XMLParamHolderBase
        {
            public static readonly string SHORT_DAMAGE = "shortDamage";
            public static readonly string LONG_DAMAGE = "longDamage";
            public static readonly string SHORT_DISTANCE = "shortDistance";
            public static readonly string LONG_DISTANCE = "longDistance";
            public static readonly string VELOCITY = "velocity";
            public static readonly string CONEFIRE = "conefire";
            public static readonly string SWAY_STAND = "swayStandMode";
            public static readonly string SWAY_CROUCH = "swayCrouchMode";
            public static readonly string SWAY_PRONE = "swayProneMode";
            public static readonly string SWAY_AIM = "swayAimMode";
            public static readonly string CONE_CONTRACTION = "coneContractionPerSecond";
            public static readonly string CAMERA_RECOIL_UP = "cameraRecoilUp";
            public static readonly string CAMERA_RECOIL_RIGHT = "cameraRecoilRight";
            public static readonly string CAMERA_RECOIL_VARIANCE = "cameraRecoilVariance";
            public static readonly string ROUNDS_PER_MINUTE = "rateOfFire";
            public static readonly string CHAMBER_TIME = "chamberTime";
        }
    }
}