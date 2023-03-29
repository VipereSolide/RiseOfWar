using System;
using System.Xml.Serialization;

namespace RiseOfWar
{
    [XmlRoot(ElementName = "param")]
    [Serializable]
    public class Param
    {
        public enum Type
        {
            Bool,
            String,
            Int,
            Float,
            Vector3,
            Vector2,
        }

        public static readonly string OPERATION_ADD = "addition";
        public static readonly string OPERATION_SUBSTRACTION = "substraction";
        public static readonly string OPERATION_DIVISION = "division";
        public static readonly string OPERATION_MULTIPLICATION = "multiplication";
        public static readonly string OPERATION_ADDED_PERCENTAGE = "addedPercentage";
        public static readonly string OPERATION_SUBSTRACTED_PERCENTAGE = "substractedPercentage";
        public static readonly string OPERATION_OVERRIDE_PERCENTAGE = "overridePercentage";

        [XmlAttribute(AttributeName = "name")]
        public string name;

        [XmlAttribute(AttributeName = "type")]
        public string type;

        [XmlText]
        public string text;

        [XmlAttribute(AttributeName = "vx")]
        public string vx;

        [XmlAttribute(AttributeName = "vy")]
        public string vy;

        [XmlAttribute(AttributeName = "vz")]
        public string vz;

        [XmlAttribute(AttributeName = "operation")]
        public string operation;

        public bool GetBool()
        {
            return bool.Parse(text);
        }

        public float GetFloat()
        {
            return MathHelper.FloatHelper.Parse(text);
        }

        public int GetInt()
        {
            return UnityEngine.Mathf.RoundToInt(MathHelper.FloatHelper.Parse(text));
        }

        public UnityEngine.Vector3 GetVector3()
        {
            float _x = MathHelper.FloatHelper.Parse(vx);
            float _y = MathHelper.FloatHelper.Parse(vy);
            float _z = MathHelper.FloatHelper.Parse(vz);
            
            return new UnityEngine.Vector3(_x, _y, _z);
        }

        public UnityEngine.Vector2 GetVector2()
        {
            float _x = MathHelper.FloatHelper.Parse(vx);
            float _y = MathHelper.FloatHelper.Parse(vy);
            
            return new UnityEngine.Vector2(_x, _y);
        }
    }
}